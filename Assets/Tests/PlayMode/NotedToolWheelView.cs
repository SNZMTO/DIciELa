using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Une vue de la roue des outils qui note ce que le présentateur lui montre, et joue ce que le joueur vise et choisit.
    /// </summary>
    internal sealed class NotedToolWheelView : IToolWheelView
    {
        public event Action<ToolChoice> Chosen;
        public event Action<ToolChoice?> Pointed;

        public string WheelKey { get; private set; }
        public Dictionary<ToolChoice, string> ToolKeys { get; } = new Dictionary<ToolChoice, string>();
        public (string Name, string Detail) Label { get; private set; }
        public HashSet<ToolChoice> Active { get; } = new HashSet<ToolChoice>();

        public void Choose(ToolChoice choice)
        {
            Chosen?.Invoke(choice);
        }

        public void Point(ToolChoice? choice)
        {
            Pointed?.Invoke(choice);
        }

        public void ShowWheelKey(string key)
        {
            WheelKey = key;
        }

        public void ShowToolKey(ToolChoice tool, string key)
        {
            ToolKeys[tool] = key;
        }

        public void ShowLabel(string name, string detail)
        {
            Label = (name, detail);
        }

        public void ShowActive(ToolChoice tool, bool active)
        {
            if (active)
                Active.Add(tool);
            else
                Active.Remove(tool);
        }
    }
}
