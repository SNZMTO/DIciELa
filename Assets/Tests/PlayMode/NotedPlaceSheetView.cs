using System;
using System.Collections.Generic;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Une vue de la fiche d'un lieu qui note ce que le présentateur lui montre, et qu'on clique comme le joueur.</summary>
    internal sealed class NotedPlaceSheetView : IPlaceSheetView
    {
        public event Action ButtonClicked;
        public event Action CloseClicked;

        public bool Shown { get; private set; }
        public string Title { get; private set; } = "";
        public List<PlaceSheetLine> Lines { get; } = new List<PlaceSheetLine>();
        public string SlotName { get; private set; } = "";
        public string SlotHolder { get; private set; } = "";
        public List<string> Tasks { get; } = new List<string>();
        public string Button { get; private set; } = "";

        public void ClickButton()
        {
            ButtonClicked?.Invoke();
        }

        public void ClickClose()
        {
            CloseClicked?.Invoke();
        }

        public void ShowSheet(bool shown)
        {
            Shown = shown;
        }

        public void ShowTitle(string title)
        {
            Title = title;
        }

        public void ShowStock(IReadOnlyList<PlaceSheetLine> lines)
        {
            Lines.Clear();
            Lines.AddRange(lines);
        }

        public void ShowSlot(string name, string holder, IReadOnlyList<PlaceSheetTask> tasks)
        {
            SlotName = name;
            SlotHolder = holder;
            Tasks.Clear();
            foreach (PlaceSheetTask task in tasks)
                Tasks.Add(task.Name);
        }

        public void ShowButton(string text)
        {
            Button = text;
        }
    }
}
