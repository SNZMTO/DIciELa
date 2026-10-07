using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La barre des outils de sol, en bas au centre, à la place du panneau Construire (décisions 206, 210, 214 et 216) :
    /// un cadre de bois ; au-dessus, ce que l'outil peut poser en feutres, la zone (son icône) ou un sol (sa vignette) ;
    /// puis ses modes en familles, chacun son icône et sa touche ; dessous, ses options : les formes du mode Forme, la
    /// taille du pinceau ; puis, dans des creux de lin, la superficie et les places de charrette ; ce qui est pris est
    /// enfoncé, le nom et l'usage de chaque feutre dans l'infobulle, un creux vide caché. La raison d'un refus passe par
    /// la bulle des raisons. C'est la vue (le bloc « place-barre » de Hud.uxml) ; son présentateur décide.
    /// </summary>
    public sealed class PaintBar : IPaintBarView
    {
        private static readonly string[] ModeClasses = { "feutre", "sable", "pan-10", "place-mode" };
        private static readonly string[] OptionClasses = { "feutre", "sable", "pan-10", "place-option" };
        private const string IconClass = "place-mode-icone";
        private const string ModeIconPrefix = "place-icone-";
        private const string ShapeIconPrefix = "place-forme-";
        private const string ZoneIconClass = "place-icone-superficie";
        private const string InkPictureClass = "place-encre-vignette";
        private readonly VisualElement _bar;
        private readonly VisualElement _shapesRow;
        private readonly FeltRow<PaintInk> _inks;
        private readonly FeltRow<PaintToolMode> _modes;
        private readonly FeltRow<PaintShape> _shapes;
        private readonly VisualElement _areaHollow;
        private readonly Label _area;
        private readonly VisualElement _marksHollow;
        private readonly Label _marks;
        private readonly VisualElement _sizeHollow;
        private readonly Label _size;
        private readonly ReasonBubble _reasons;
        private PaintBarPresenter _presenter;

        public PaintBar(UIDocument document, Tooltips tooltips, ReasonBubble reasons)
        {
            VisualElement root = document != null ? document.rootVisualElement : null;
            _bar = root?.Q("place-barre");
            VisualElement inks = root?.Q("place-encres");
            VisualElement modes = root?.Q("place-modes");
            _shapesRow = root?.Q("place-formes");
            _areaHollow = root?.Q("place-superficie-creux");
            _area = root?.Q<Label>("place-superficie");
            _marksHollow = root?.Q("place-marques-creux");
            _marks = root?.Q<Label>("place-marques");
            _sizeHollow = root?.Q("place-taille-creux");
            _size = root?.Q<Label>("place-taille");
            if (_bar == null || inks == null || modes == null || _shapesRow == null || _areaHollow == null ||
                _area == null || _marksHollow == null || _marks == null || _sizeHollow == null || _size == null ||
                tooltips == null || reasons == null)
                throw new InvalidOperationException("La barre des outils de sol ne trouve pas ses éléments (UXML).");
            _reasons = reasons;
            _inks = new FeltRow<PaintInk>(inks, tooltips, ModeClasses);
            _modes = new FeltRow<PaintToolMode>(modes, tooltips, ModeClasses);
            _shapes = new FeltRow<PaintShape>(_shapesRow, tooltips, OptionClasses);
            _inks.Chosen += ink => InkChosen?.Invoke(ink);
            _modes.Chosen += mode => ModeChosen?.Invoke(mode);
            _shapes.Chosen += shape => ShapeChosen?.Invoke(shape);
        }

        public event Action<PaintToolMode> ModeChosen;
        public event Action<PaintShape> ShapeChosen;
        public event Action<PaintInk> InkChosen;

        /// <summary>
        /// Relie la barre aux outils de sol de ce monde, l'outil de la Place et l'outil Chemins ; cachée tant qu'aucun n'est
        /// en main.
        /// </summary>
        public void Show(TextTable texts, PaintTool plaza, PaintTool paths, World world, IToolKeys keys)
        {
            Hide();
            _presenter = new PaintBarPresenter(this, texts, plaza, paths, world, keys);
        }

        /// <summary>Cache la barre et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            _inks.Clear();
            _modes.Clear();
            ShowShapes(Array.Empty<PaintBarChoice<PaintShape>>());
            ShowShown(false);
        }

        public void ShowShown(bool shown)
        {
            _bar.EnableInClassList(StyleClasses.Hidden, !shown);
        }

        public void ShowInks(IReadOnlyList<PaintBarInk> inks)
        {
            _inks.Clear();
            foreach (PaintBarInk ink in inks)
            {
                var face = new VisualElement();
                if (ink.Ink.IsZone)
                    Styled.With(face, IconClass, ZoneIconClass);
                else
                {
                    Styled.With(face, InkPictureClass);
                    if (ink.Picture != null)
                        face.style.backgroundImage = new StyleBackground(ink.Picture);
                }
                _inks.Add(ink.Ink, face, ink.Name, ink.Detail, "");
            }
        }

        public void SelectInk(PaintInk ink)
        {
            _inks.Select(ink);
        }

        public void ShowModes(IReadOnlyList<PaintBarChoice<PaintToolMode>> modes)
        {
            _modes.Clear();
            foreach (PaintBarChoice<PaintToolMode> mode in modes)
            {
                VisualElement face = Styled.With(new VisualElement(), IconClass,
                    ModeIconPrefix + mode.Value.ToString().ToLowerInvariant());
                _modes.Add(mode.Value, face, mode.Name, mode.Detail, mode.Key);
            }
        }

        public void SelectMode(PaintToolMode mode)
        {
            _modes.Select(mode);
        }

        public void ShowShapes(IReadOnlyList<PaintBarChoice<PaintShape>> shapes)
        {
            _shapes.Clear();
            foreach (PaintBarChoice<PaintShape> shape in shapes)
            {
                VisualElement face = Styled.With(new VisualElement(), IconClass,
                    ShapeIconPrefix + shape.Value.ToString().ToLowerInvariant());
                _shapes.Add(shape.Value, face, shape.Name, shape.Detail, shape.Key);
            }
            _shapesRow.EnableInClassList(StyleClasses.Hidden, shapes.Count == 0);
        }

        public void SelectShape(PaintShape shape)
        {
            _shapes.Select(shape);
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
    }
}
