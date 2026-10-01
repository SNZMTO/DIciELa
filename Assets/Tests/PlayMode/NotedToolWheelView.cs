using System;
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

        public (string Wheel, string Build, string Clear) Keys { get; private set; }
        public (string Name, string Detail) Label { get; private set; }
        public bool Building { get; private set; }
        public bool Clearing { get; private set; }

        public void Choose(ToolChoice choice)
        {
            Chosen?.Invoke(choice);
        }

        public void Point(ToolChoice? choice)
        {
            Pointed?.Invoke(choice);
        }

        public void ShowKeys(string wheel, string build, string clear)
        {
            Keys = (wheel, build, clear);
        }

        public void ShowLabel(string name, string detail)
        {
            Label = (name, detail);
        }

        public void ShowActive(bool building, bool clearing)
        {
            Building = building;
            Clearing = clearing;
        }
    }
}
