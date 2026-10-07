using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Une rangée d'options des outils de sol qui note ce que son présentateur lui montre, et joue les clics du joueur.
    /// </summary>
    internal sealed class NotedPaintOptionsView : IPaintOptionsView
    {
        public event Action<PaintShape> ShapeChosen;
        public event Action<PaintTraceStyle> StyleChosen;
        public event Action<PaintSymmetry> SymmetryChosen;
        public event Action<int> StarChosen;

        public List<PaintBarChoice<PaintShape>> Shapes { get; } = new List<PaintBarChoice<PaintShape>>();
        public List<PaintBarChoice<PaintTraceStyle>> Styles { get; } = new List<PaintBarChoice<PaintTraceStyle>>();
        public List<PaintBarChoice<PaintSymmetry>> Symmetries { get; } = new List<PaintBarChoice<PaintSymmetry>>();
        public List<PaintBarChoice<int>> Stars { get; } = new List<PaintBarChoice<int>>();
        public PaintShape SelectedShape { get; private set; }
        public PaintTraceStyle SelectedStyle { get; private set; }
        public PaintSymmetry SelectedSymmetry { get; private set; }
        public int SelectedStar { get; private set; }

        public void ChooseShape(PaintShape shape)
        {
            ShapeChosen?.Invoke(shape);
        }

        public void ChooseStyle(PaintTraceStyle style)
        {
            StyleChosen?.Invoke(style);
        }

        public void ChooseSymmetry(PaintSymmetry symmetry)
        {
            SymmetryChosen?.Invoke(symmetry);
        }

        public void ChooseStar(int branches)
        {
            StarChosen?.Invoke(branches);
        }

        public void ShowShapes(IReadOnlyList<PaintBarChoice<PaintShape>> shapes)
        {
            Shapes.Clear();
            Shapes.AddRange(shapes);
        }

        public void SelectShape(PaintShape shape)
        {
            SelectedShape = shape;
        }

        public void ShowStyles(IReadOnlyList<PaintBarChoice<PaintTraceStyle>> styles)
        {
            Styles.Clear();
            Styles.AddRange(styles);
        }

        public void SelectStyle(PaintTraceStyle style)
        {
            SelectedStyle = style;
        }

        public void ShowSymmetries(IReadOnlyList<PaintBarChoice<PaintSymmetry>> symmetries)
        {
            Symmetries.Clear();
            Symmetries.AddRange(symmetries);
        }

        public void SelectSymmetry(PaintSymmetry symmetry)
        {
            SelectedSymmetry = symmetry;
        }

        public void ShowStars(IReadOnlyList<PaintBarChoice<int>> stars)
        {
            Stars.Clear();
            Stars.AddRange(stars);
        }

        public void SelectStar(int branches)
        {
            SelectedStar = branches;
        }
    }
}
