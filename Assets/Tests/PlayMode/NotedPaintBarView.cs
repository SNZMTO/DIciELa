using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Une vue de la barre des outils de sol qui note ce que le présentateur lui montre, et joue les clics du joueur.
    /// </summary>
    internal sealed class NotedPaintBarView : IPaintBarView
    {
        public event Action<PaintToolMode> ModeChosen;
        public event Action<PaintShape> ShapeChosen;
        public event Action<PaintTraceStyle> StyleChosen;
        public event Action<PaintInk> InkChosen;

        public bool IsShown { get; private set; }
        public List<PaintBarChoice<PaintToolMode>> Modes { get; } = new List<PaintBarChoice<PaintToolMode>>();
        public List<PaintBarChoice<PaintShape>> Shapes { get; } = new List<PaintBarChoice<PaintShape>>();
        public List<PaintBarChoice<PaintTraceStyle>> Styles { get; } = new List<PaintBarChoice<PaintTraceStyle>>();
        public List<PaintBarInk> Inks { get; } = new List<PaintBarInk>();
        public PaintInk SelectedInk { get; private set; }

        /// <summary>Combien de fois la barre a refait ses encres.</summary>
        public int InksShown { get; private set; }
        public PaintToolMode Selected { get; private set; }
        public PaintShape SelectedShape { get; private set; }
        public PaintTraceStyle SelectedStyle { get; private set; }
        public string Area { get; private set; } = "";
        public string Marks { get; private set; } = "";
        public string BrushSize { get; private set; } = "";
        public string Reason { get; private set; } = "";

        /// <summary>Le clic du joueur sur un mode.</summary>
        public void Choose(PaintToolMode mode)
        {
            ModeChosen?.Invoke(mode);
        }

        /// <summary>Le clic du joueur sur une forme du mode Forme.</summary>
        public void ChooseShape(PaintShape shape)
        {
            ShapeChosen?.Invoke(shape);
        }

        /// <summary>Le clic du joueur sur un style de tracé.</summary>
        public void ChooseStyle(PaintTraceStyle style)
        {
            StyleChosen?.Invoke(style);
        }

        /// <summary>Le clic du joueur sur ce qu'il veut poser : la zone, ou un sol.</summary>
        public void ChooseInk(PaintInk ink)
        {
            InkChosen?.Invoke(ink);
        }

        public void ShowInks(IReadOnlyList<PaintBarInk> inks)
        {
            Inks.Clear();
            Inks.AddRange(inks);
            InksShown++;
        }

        public void SelectInk(PaintInk ink)
        {
            SelectedInk = ink;
        }

        public void ShowShown(bool shown)
        {
            IsShown = shown;
        }

        public void ShowModes(IReadOnlyList<PaintBarChoice<PaintToolMode>> modes)
        {
            Modes.Clear();
            Modes.AddRange(modes);
        }

        public void SelectMode(PaintToolMode mode)
        {
            Selected = mode;
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

        public void ShowValues(string area, string marks, string brushSize)
        {
            Area = area;
            Marks = marks;
            BrushSize = brushSize;
        }

        public void ShowReason(string text)
        {
            Reason = text;
        }
    }
}
