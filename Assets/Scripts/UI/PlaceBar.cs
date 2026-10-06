using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La barre de la Place, en bas au centre, à la place du panneau Construire qu'on a fermé en prenant la carte
    /// (décision 206) : un cadre de bois ; ses modes en feutres, chacun son icône et sa touche, celui qui est pris
    /// enfoncé, son nom et son usage dans l'infobulle ; dessous, dans des creux de lin, la superficie, les places de
    /// charrette et la taille du pinceau. La raison d'un refus passe par la bulle des raisons. C'est la vue (le bloc « place-barre » de Hud.uxml) ; son présentateur décide.
    /// </summary>
    public sealed class PlaceBar : IPlaceBarView
    {
        private static readonly string[] ModeClasses = { "feutre", "sable", "pan-10", "place-mode" };
        private static readonly string[] KeyClasses = { "creux", "pan-4", "touche" };
        private const string ModeIconClass = "place-mode-icone";
        private const string ModeIconPrefix = "place-icone-";
        private const string KeyTextClass = "touche-texte";

        private readonly VisualElement _bar;
        private readonly VisualElement _modes;
        private readonly Label _area;
        private readonly Label _marks;
        private readonly VisualElement _sizeHollow;
        private readonly Label _size;
        private readonly Tooltips _tooltips;
        private readonly ReasonBubble _reasons;
        private readonly List<IDisposable> _tips = new List<IDisposable>();
        private PlaceBarPresenter _presenter;

        public PlaceBar(UIDocument document, Tooltips tooltips, ReasonBubble reasons)
        {
            VisualElement root = document != null ? document.rootVisualElement : null;
            _bar = root?.Q("place-barre");
            _modes = root?.Q("place-modes");
            _area = root?.Q<Label>("place-superficie");
            _marks = root?.Q<Label>("place-marques");
            _sizeHollow = root?.Q("place-taille-creux");
            _size = root?.Q<Label>("place-taille");
            if (_bar == null || _modes == null || _area == null || _marks == null || _sizeHollow == null ||
                _size == null || tooltips == null || reasons == null)
                throw new InvalidOperationException("La barre de la Place ne trouve pas ses éléments (UXML).");
            _tooltips = tooltips;
            _reasons = reasons;
        }

        public event Action<PlaceToolMode> ModeChosen;

        /// <summary>Relie la barre à l'outil de la Place de ce monde ; cachée tant que l'outil n'est pas en main.</summary>
        public void Show(TextTable texts, PlaceTool tool, World world, IToolKeys keys)
        {
            Hide();
            _presenter = new PlaceBarPresenter(this, texts, tool, world, keys);
        }

        /// <summary>Cache la barre et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            ClearModes();
            ShowShown(false);
        }

        public void ShowShown(bool shown)
        {
            _bar.EnableInClassList(StyleClasses.Hidden, !shown);
        }

        public void ShowModes(IReadOnlyList<PlaceBarMode> modes)
        {
            ClearModes();
            foreach (PlaceBarMode mode in modes)
            {
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
                _modes[i].EnableInClassList(StyleClasses.Open, i == (int)mode);
        }

        public void ShowValues(string area, string marks, string brushSize)
        {
            _area.text = area;
            _marks.text = marks;
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
            _tooltips.Hide();
        }
    }
}
