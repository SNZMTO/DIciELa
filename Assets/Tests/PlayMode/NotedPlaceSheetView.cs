using System;
using System.Collections.Generic;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Une vue de la fiche d'un lieu qui note ce que le présentateur lui montre, et qu'on clique comme le joueur.</summary>
    internal sealed class NotedPlaceSheetView : IPlaceSheetView
    {
        public event Action<int> ActionClicked;
        public event Action CloseClicked;

        public bool Shown { get; private set; }
        public string Title { get; private set; } = "";
        public List<PlaceSheetLine> Lines { get; } = new List<PlaceSheetLine>();
        public List<PlaceSheetSlot> Slots { get; } = new List<PlaceSheetSlot>();
        public List<PlaceSheetAction> Actions { get; } = new List<PlaceSheetAction>();

        /// <summary>Clique le bouton qui dit ce texte ; faux s'il n'y en a pas.</summary>
        public bool Click(string text)
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i].Text == text)
                {
                    ActionClicked?.Invoke(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Les textes des boutons, dans l'ordre.</summary>
        public string[] ActionTexts()
        {
            var texts = new string[Actions.Count];
            for (int i = 0; i < Actions.Count; i++)
                texts[i] = Actions[i].Text;
            return texts;
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

        public void ShowSlots(IReadOnlyList<PlaceSheetSlot> slots)
        {
            Slots.Clear();
            Slots.AddRange(slots);
        }

        /// <summary>Qui tient chaque poste montré, dans l'ordre.</summary>
        public string[] Holders()
        {
            var holders = new string[Slots.Count];
            for (int i = 0; i < Slots.Count; i++)
                holders[i] = Slots[i].Holder;
            return holders;
        }

        public void ShowActions(IReadOnlyList<PlaceSheetAction> actions)
        {
            Actions.Clear();
            Actions.AddRange(actions);
        }
    }
}
