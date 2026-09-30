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
        public event Action<int, int> Recolored;
        public event Action Confirmed;

        public string Title { get; private set; } = "";
        public List<string> Labels { get; } = new List<string>();
        public List<string> Choices { get; } = new List<string>();
        public List<string> Colors { get; } = new List<string>();

        public void Step(int row, int direction)
        {
            Stepped?.Invoke(row, direction);
        }

        public void Recolor(int row, int direction)
        {
            Recolored?.Invoke(row, direction);
        }

        public void Confirm()
        {
            Confirmed?.Invoke();
        }

        public void ShowRows(string title, string confirm, IReadOnlyList<string> labels)
        {
            Title = title;
            Labels.Clear();
            Labels.AddRange(labels);
            Choices.Clear();
            Colors.Clear();
            for (int i = 0; i < labels.Count; i++)
            {
                Choices.Add("");
                Colors.Add("");
            }
        }

        public void ShowChoice(int row, string choice, string color)
        {
            Choices[row] = choice;
            Colors[row] = color;
        }
    }
}
