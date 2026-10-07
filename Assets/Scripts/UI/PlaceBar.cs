using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La barre des outils de sol, en bas au centre, à la place du panneau Construire (décisions 206, 210 et 214) : un
    /// cadre de bois ; au-dessus, ce que l'outil peut poser en feutres, la zone (son icône) ou un sol (sa vignette),
    /// celui qui est pris enfoncé ; puis ses modes en feutres, chacun son icône et sa touche, celui qui est pris enfoncé,
    /// son nom et son usage dans l'infobulle ; dessous, dans des creux de lin, la superficie, les places de charrette et
    /// la taille du pinceau, chacun caché quand il est vide. La raison d'un refus passe par la bulle des raisons. C'est
    /// la vue (le bloc « place-barre » de Hud.uxml) ; son présentateur décide.
    /// </summary>
    public sealed class PlaceBar : IPlaceBarView
    {
        private static readonly string[] ModeClasses = { "feutre", "sable", "pan-10", "place-mode" };
        private static readonly string[] KeyClasses = { "creux", "pan-4", "touche" };
        private const string ModeIconClass = "place-mode-icone";
        private const string ModeIconPrefix = "place-icone-";
        private const string KeyTextClass = "touche-texte";
        private const string ZoneIconClass = "place-icone-superficie";
        private const string InkPictureClass = "place-encre-vignette";

        private readonly VisualElement _bar;
        private readonly VisualElement _inks;
        private readonly VisualElement _modes;
        private readonly VisualElement _areaHollow;
        private readonly Label _area;
        private readonly VisualElement _marksHollow;
        private readonly Label _marks;
        private readonly VisualElement _sizeHollow;
        private readonly Label _size;
        private readonly Tooltips _tooltips;
        private readonly ReasonBubble _reasons;
        private readonly List<IDisposable> _tips = new List<IDisposable>();
        private readonly List<IDisposable> _inkTips = new List<IDisposable>();
        private readonly List<PlaceToolMode> _shownModes = new List<PlaceToolMode>();
        private readonly List<FloorDefinition> _shownInks = new List<FloorDefinition>();
        private PlaceBarPresenter _presenter;

        public PlaceBar(UIDocument document, Tooltips tooltips, ReasonBubble reasons)
        {
            VisualElement root = document != null ? document.rootVisualElement : null;
            _bar = root?.Q("place-barre");
            _inks = root?.Q("place-encres");
            _modes = root?.Q("place-modes");
            _areaHollow = root?.Q("place-superficie-creux");
            _area = root?.Q<Label>("place-superficie");
            _marksHollow = root?.Q("place-marques-creux");
            _marks = root?.Q<Label>("place-marques");
            _sizeHollow = root?.Q("place-taille-creux");
            _size = root?.Q<Label>("place-taille");
            if (_bar == null || _inks == null || _modes == null || _areaHollow == null || _area == null ||
                _marksHollow == null || _marks == null || _sizeHollow == null || _size == null || tooltips == null ||
                reasons == null)
                throw new InvalidOperationException("La barre de la Place ne trouve pas ses éléments (UXML).");
            _tooltips = tooltips;
            _reasons = reasons;
        }

        public event Action<PlaceToolMode> ModeChosen;
        public event Action<FloorDefinition> InkChosen;

        /// <summary>
        /// Relie la barre aux outils de sol de ce monde, l'outil de la Place et l'outil Chemins ; cachée tant qu'aucun n'est
        /// en main.
        /// </summary>
        public void Show(TextTable texts, PlaceTool place, PlaceTool paths, World world, IToolKeys keys)
        {
            Hide();
            _presenter = new PlaceBarPresenter(this, texts, place, paths, world, keys);
        }

        /// <summary>Cache la barre et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            ClearModes();
            ClearInks();
            ShowShown(false);
        }

        public void ShowShown(bool shown)
        {
            _bar.EnableInClassList(StyleClasses.Hidden, !shown);
        }

        public void ShowInks(IReadOnlyList<PlaceBarInk> inks)
        {
            ClearInks();
            foreach (PlaceBarInk ink in inks)
            {
                FeltButton button = Styled.With(new FeltButton(), ModeClasses);
                FloorDefinition chosen = ink.Floor;
                button.Clicked += () => InkChosen?.Invoke(chosen);
                var picture = new VisualElement { pickingMode = PickingMode.Ignore };
                if (ink.Floor == null)
                    Styled.With(picture, ModeIconClass, ZoneIconClass);
                else
                {
                    Styled.With(picture, InkPictureClass);
                    if (ink.Picture != null)
                        picture.style.backgroundImage = new StyleBackground(ink.Picture);
                }
                button.Add(picture);
                (string, string) tip = (ink.Name, ink.Detail);
                _inkTips.Add(_tooltips.Attach(button, () => tip, true));
                _inks.Add(button);
                _shownInks.Add(ink.Floor);
            }
        }

        public void SelectInk(FloorDefinition floor)
        {
            for (int i = 0; i < _inks.childCount; i++)
                _inks[i].EnableInClassList(StyleClasses.Open, ReferenceEquals(_shownInks[i], floor));
        }

        public void ShowModes(IReadOnlyList<PlaceBarMode> modes)
        {
            ClearModes();
            foreach (PlaceBarMode mode in modes)
            {
                _shownModes.Add(mode.Mode);
                FeltButton button = Styled.With(new FeltButton(), ModeClasses);
                PlaceToolMode chosen = mode.Mode;
                button.Clicked += () => ModeChosen?.Invoke(chosen);
                var icon = Styled.With(new VisualElement { pickingMode = PickingMode.Ignore }, ModeIconClass);
                icon.AddToClassList(ModeIconPrefix + mode.Mode.ToString().ToLowerInvariant());
                button.Add(icon);
                if (!string.IsNullOrEmpty(mode.Key))
                {
                    Facet key = Styled.With(new Facet { pickingMode = PickingMode.Ignore }, KeyClasses);
                    key.Add(Styled.Text(mode.Key, KeyTextClass));
                    button.Add(key);
                }
                (string, string) tip = (mode.Name, mode.Detail);
                _tips.Add(_tooltips.Attach(button, () => tip, true));
                _modes.Add(button);
            }
        }

        public void SelectMode(PlaceToolMode mode)
        {
            for (int i = 0; i < _modes.childCount; i++)
                _modes[i].EnableInClassList(StyleClasses.Open, _shownModes[i] == mode);
        }

        public void ShowValues(string area, string marks, string brushSize)
        {
            _area.text = area;
            _areaHollow.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(area));
            _marks.text = marks;
            _marksHollow.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(marks));
            _size.text = brushSize;
            _sizeHollow.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(brushSize));
        }

        public void ShowReason(string text)
        {
            _reasons.Say(this, text);
        }

        private void ClearModes()
        {
            foreach (IDisposable tip in _tips)
                tip.Dispose();
            _tips.Clear();
            _modes.Clear();
            _shownModes.Clear();
            _tooltips.Hide();
        }

        private void ClearInks()
        {
            foreach (IDisposable tip in _inkTips)
                tip.Dispose();
            _inkTips.Clear();
            _inks.Clear();
            _shownInks.Clear();
            _tooltips.Hide();
        }
    }
}
