using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Une vue de la barre de la Place qui note ce que le présentateur lui montre, et joue les clics du joueur.</summary>
    internal sealed class NotedPlaceBarView : IPlaceBarView
    {
        public event Action<PlaceToolMode> ModeChosen;

        public bool IsShown { get; private set; }
        public List<PlaceBarMode> Modes { get; } = new List<PlaceBarMode>();
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
