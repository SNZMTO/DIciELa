using System;
using System.Collections.Generic;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Une vue de la barre de construction qui note ce que le présentateur lui montre, et joue les clics du joueur.
    /// </summary>
    internal sealed class NotedBuildBarView : IBuildBarView
    {
        public event Action<int> FamilyChosen;
        public event Action<int> StructureChosen;

        public List<string> Families { get; } = new List<string>();
        public List<string> Structures { get; } = new List<string>();
        public int OpenFamily { get; private set; } = -1;
        public int Selected { get; private set; } = -1;
        public string Reason { get; private set; } = "";

        public void Choose(int structure)
        {
            StructureChosen?.Invoke(structure);
        }

        public void Open(int family)
        {
            FamilyChosen?.Invoke(family);
        }

        public void ShowFamilies(IReadOnlyList<BuildBarFamily> families, int open)
        {
            Families.Clear();
            foreach (BuildBarFamily family in families)
                Families.Add(family.Name);
            OpenFamily = open;
        }

        public void ShowStructures(IReadOnlyList<BuildBarCard> cards)
        {
            Structures.Clear();
            foreach (BuildBarCard card in cards)
                Structures.Add(card.Name);
        }

        public void SelectStructure(int index)
        {
            Selected = index;
        }

        public void ShowReason(string text)
        {
            Reason = text;
        }
    }
}
