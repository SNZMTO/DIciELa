using System;
using System.Collections.Generic;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Une vue de l'écran de création qui note ce que le présentateur lui montre, et joue les flèches et la validation du
    /// joueur.
    /// </summary>
    internal sealed class NotedCreationView : ICreationView
    {
        public event Action<int, int> Stepped;
        public event Action Confirmed;

        public string Title { get; private set; } = "";
        public List<string> Labels { get; } = new List<string>();
        public List<string> Choices { get; } = new List<string>();

        public void Step(int row, int direction)
        {
            Stepped?.Invoke(row, direction);
        }

        public void Confirm()
        {
            Confirmed?.Invoke();
        }

        public void ShowRows(string title, string confirm, string previous, string next, IReadOnlyList<string> labels)
        {
            Title = title;
            Labels.Clear();
            Labels.AddRange(labels);
            Choices.Clear();
            for (int i = 0; i < labels.Count; i++)
                Choices.Add("");
        }

        public void ShowChoice(int row, string choice)
        {
            Choices[row] = choice;
        }
    }
}
