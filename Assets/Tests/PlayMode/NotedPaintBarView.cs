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
        public event Action<PaintInk> InkChosen;

        public bool IsShown { get; private set; }
        public List<PaintBarMode> Modes { get; } = new List<PaintBarMode>();
        public List<PaintBarInk> Inks { get; } = new List<PaintBarInk>();
        public PaintInk SelectedInk { get; private set; }

        /// <summary>Combien de fois la barre a refait ses encres.</summary>
        public int InksShown { get; private set; }
        public PaintToolMode Selected { get; private set; }
        public string Area { get; private set; } = "";
        public string Marks { get; private set; } = "";
        public string BrushSize { get; private set; } = "";
        public string Reason { get; private set; } = "";

        /// <summary>Le clic du joueur sur un mode.</summary>
        public void Choose(PaintToolMode mode)
        {
            ModeChosen?.Invoke(mode);
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

        public void ShowModes(IReadOnlyList<PaintBarMode> modes)
        {
            Modes.Clear();
            Modes.AddRange(modes);
        }

        public void SelectMode(PaintToolMode mode)
        {
            Selected = mode;
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
