using System;
using System.Collections.Generic;
using Diciela.Controls;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La rangée d'options de la barre des outils de sol (décision 216), sous ses modes, en petits feutres : les formes du
    /// mode Forme, les styles d'un tracé ou d'un polygone et les symétries, chacun son icône ; dessous, sur leur rangée,
    /// les nombres de branches de l'étoile, en feutres plus petits, chacun son chiffre ; une rangée vide se cache. C'est
    /// une vue ; son présentateur (PaintOptionsPresenter) décide.
    /// </summary>
    internal sealed class PaintOptionsRow : IPaintOptionsView
    {
        private static readonly string[] OptionClasses = { "feutre", "sable", "pan-10", "place-option" };
        private static readonly string[] NumberClasses =
            { "feutre", "sable", "pan-10", "place-option", "place-option-nombre" };
        private const string IconClass = "place-mode-icone";
        private const string NumberClass = "t3";
        private readonly VisualElement _shapesRow;
        private readonly VisualElement _stylesRow;
        private readonly VisualElement _symmetriesRow;
        private readonly VisualElement _starsRow;
        private readonly FeltRow<PaintShape> _shapes;
        private readonly FeltRow<PaintTraceStyle> _styles;
        private readonly FeltRow<PaintSymmetry> _symmetries;
        private readonly FeltRow<int> _stars;

        public PaintOptionsRow(VisualElement root, Tooltips tooltips)
        {
            _shapesRow = root?.Q("place-formes");
            _stylesRow = root?.Q("place-styles");
            _symmetriesRow = root?.Q("place-symetries");
            _starsRow = root?.Q("place-etoiles");
            if (_shapesRow == null || _stylesRow == null || _symmetriesRow == null || _starsRow == null)
                throw new InvalidOperationException("La rangée d'options ne trouve pas ses éléments (UXML).");
            _shapes = new FeltRow<PaintShape>(_shapesRow, tooltips, OptionClasses);
            _styles = new FeltRow<PaintTraceStyle>(_stylesRow, tooltips, OptionClasses);
            _symmetries = new FeltRow<PaintSymmetry>(_symmetriesRow, tooltips, OptionClasses);
            _stars = new FeltRow<int>(_starsRow, tooltips, NumberClasses);
            _shapes.Chosen += shape => ShapeChosen?.Invoke(shape);
            _styles.Chosen += style => StyleChosen?.Invoke(style);
            _symmetries.Chosen += symmetry => SymmetryChosen?.Invoke(symmetry);
            _stars.Chosen += copies => StarChosen?.Invoke(copies);
        }

        public event Action<PaintShape> ShapeChosen;
        public event Action<PaintTraceStyle> StyleChosen;
        public event Action<PaintSymmetry> SymmetryChosen;
        public event Action<int> StarChosen;

        public void ShowShapes(IReadOnlyList<PaintBarChoice<PaintShape>> shapes)
        {
            Show(_shapes, _shapesRow, shapes, "place-forme-");
        }

        public void SelectShape(PaintShape shape)
        {
            _shapes.Select(shape);
        }

        public void ShowStyles(IReadOnlyList<PaintBarChoice<PaintTraceStyle>> styles)
        {
            Show(_styles, _stylesRow, styles, "place-style-");
        }

        public void SelectStyle(PaintTraceStyle style)
        {
            _styles.Select(style);
        }

        public void ShowSymmetries(IReadOnlyList<PaintBarChoice<PaintSymmetry>> symmetries)
        {
            Show(_symmetries, _symmetriesRow, symmetries, "place-symetrie-");
        }

        public void SelectSymmetry(PaintSymmetry symmetry)
        {
            _symmetries.Select(symmetry);
        }

        public void ShowStars(IReadOnlyList<PaintBarChoice<int>> stars)
        {
            _stars.Clear();
            foreach (PaintBarChoice<int> star in stars)
                _stars.Add(star.Value, Styled.Text(star.Name, NumberClass), star.Name, star.Detail, star.Key);
            _starsRow.EnableInClassList(StyleClasses.Hidden, stars.Count == 0);
        }

        public void SelectStar(int copies)
        {
            _stars.Select(copies);
        }

        /// <summary>Vide et cache toutes les rangées.</summary>
        public void Clear()
        {
            ShowShapes(Array.Empty<PaintBarChoice<PaintShape>>());
            ShowStyles(Array.Empty<PaintBarChoice<PaintTraceStyle>>());
            ShowSymmetries(Array.Empty<PaintBarChoice<PaintSymmetry>>());
            ShowStars(Array.Empty<PaintBarChoice<int>>());
        }

        // Une rangée d'options, chacune l'icône de sa valeur ; vide, elle se cache.
        private static void Show<T>(FeltRow<T> row, VisualElement element, IReadOnlyList<PaintBarChoice<T>> choices,
            string iconPrefix)
        {
            row.Clear();
            foreach (PaintBarChoice<T> choice in choices)
            {
                VisualElement face = Styled.With(new VisualElement(), IconClass,
                    iconPrefix + choice.Value.ToString().ToLowerInvariant());
                row.Add(choice.Value, face, choice.Name, choice.Detail, choice.Key);
            }
            element.EnableInClassList(StyleClasses.Hidden, choices.Count == 0);
        }
    }
}
