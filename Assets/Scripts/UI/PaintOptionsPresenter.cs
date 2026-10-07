using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la rangée d'options de la barre des outils de sol (décision 216), pour l'outil en main : les
    /// formes au mode Forme ; les styles au tracé, et au polygone du mode Forme ; les symétries dans tous les modes ; le
    /// nombre de copies à l'étoile. Chaque rangée n'est refaite que quand elle paraît ou disparaît ; ce qui est pris est
    /// enfoncé. Un clic choisit chez les options de l'outil (PaintOptions).
    /// </summary>
    internal sealed class PaintOptionsPresenter : IDisposable
    {
        private static readonly IReadOnlyList<PaintBarChoice<PaintShape>> NoShapes =
            Array.Empty<PaintBarChoice<PaintShape>>();
        private static readonly IReadOnlyList<PaintBarChoice<PaintTraceStyle>> NoStyles =
            Array.Empty<PaintBarChoice<PaintTraceStyle>>();
        private static readonly IReadOnlyList<PaintBarChoice<int>> NoStars = Array.Empty<PaintBarChoice<int>>();
        private readonly IPaintOptionsView _view;
        private readonly TextTable _texts;
        private readonly Func<PaintTool> _held;
        private bool _shapesShown;
        private bool _stylesShown;
        private bool _symmetriesShown;
        private bool _starsShown;

        public PaintOptionsPresenter(IPaintOptionsView view, TextTable texts, Func<PaintTool> held)
        {
            _view = view;
            _texts = texts;
            _held = held;
            _view.ShapeChosen += OnShapeChosen;
            _view.StyleChosen += OnStyleChosen;
            _view.SymmetryChosen += OnSymmetryChosen;
            _view.StarChosen += OnStarChosen;
        }

        public void Dispose()
        {
            _view.ShapeChosen -= OnShapeChosen;
            _view.StyleChosen -= OnStyleChosen;
            _view.SymmetryChosen -= OnSymmetryChosen;
            _view.StarChosen -= OnStarChosen;
        }

        /// <summary>Montre les options de cet outil, en main.</summary>
        public void Show(PaintTool held)
        {
            PaintOptions options = held.Options;
            bool shapes = held.Mode == PaintToolMode.Shape;
            if (shapes != _shapesShown)
            {
                _shapesShown = shapes;
                _view.ShowShapes(shapes ? PaintBarContent.Shapes(_texts) : NoShapes);
            }
            bool styles = held.Mode == PaintToolMode.Trace || shapes && options.Shape == PaintShape.Polygon;
            if (styles != _stylesShown)
            {
                _stylesShown = styles;
                _view.ShowStyles(styles ? PaintBarContent.Styles(_texts) : NoStyles);
            }
            if (!_symmetriesShown)
            {
                _symmetriesShown = true;
                _view.ShowSymmetries(PaintBarContent.Symmetries(_texts));
            }
            bool stars = options.Symmetry == PaintSymmetry.Star;
            if (stars != _starsShown)
            {
                _starsShown = stars;
                _view.ShowStars(stars ? PaintBarContent.Stars(_texts, options.StarCopiesRange) : NoStars);
            }
            _view.SelectShape(options.Shape);
            _view.SelectStyle(options.Style);
            _view.SelectSymmetry(options.Symmetry);
            _view.SelectStar(options.StarCopies);
        }

        private void OnShapeChosen(PaintShape shape)
        {
            _held()?.Options.Choose(shape);
        }

        private void OnStyleChosen(PaintTraceStyle style)
        {
            _held()?.Options.Choose(style);
        }

        private void OnSymmetryChosen(PaintSymmetry symmetry)
        {
            _held()?.Options.Choose(symmetry);
        }

        private void OnStarChosen(int copies)
        {
            _held()?.Options.ChooseStar(copies);
        }
    }
}
