using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Une vue de la barre des outils de sol qui note ce que le présentateur lui montre, et joue les clics du joueur.
    /// </summary>
    internal sealed class NotedPlaceBarView : IPlaceBarView
    {
        public event Action<PlaceToolMode> ModeChosen;
        public event Action<FloorDefinition> InkChosen;

        public bool IsShown { get; private set; }
        public List<PlaceBarMode> Modes { get; } = new List<PlaceBarMode>();
        public List<PlaceBarInk> Inks { get; } = new List<PlaceBarInk>();
        public FloorDefinition SelectedInk { get; private set; }

        /// <summary>Combien de fois la barre a refait ses encres.</summary>
        public int InksShown { get; private set; }
        public PlaceToolMode Selected { get; private set; }
        public string Area { get; private set; } = "";
        public string Marks { get; private set; } = "";
        public string BrushSize { get; private set; } = "";
        public string Reason { get; private set; } = "";

        /// <summary>Le clic du joueur sur un mode.</summary>
        public void Choose(PlaceToolMode mode)
        {
            ModeChosen?.Invoke(mode);
        }

        /// <summary>Le clic du joueur sur ce qu'il veut poser : un sol, ou la zone (null).</summary>
        public void ChooseInk(FloorDefinition floor)
        {
            InkChosen?.Invoke(floor);
        }

        public void ShowInks(IReadOnlyList<PlaceBarInk> inks)
        {
            Inks.Clear();
            Inks.AddRange(inks);
            InksShown++;
        }

        public void SelectInk(FloorDefinition floor)
        {
            SelectedInk = floor;
        }

        public void ShowShown(bool shown)
        {
            IsShown = shown;
        }

        public void ShowModes(IReadOnlyList<PlaceBarMode> modes)
        {
            Modes.Clear();
            Modes.AddRange(modes);
        }

        public void SelectMode(PlaceToolMode mode)
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
