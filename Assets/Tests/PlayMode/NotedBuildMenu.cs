using System;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Un panneau Construire qui s'ouvre et se ferme, sans rien montrer.</summary>
    internal sealed class NotedBuildMenu : IBuildMenu
    {
        public event Action OpenChanged;

        public bool IsOpen { get; private set; }

        public void Open()
        {
            IsOpen = true;
            OpenChanged?.Invoke();
        }

        public void Close()
        {
            IsOpen = false;
            OpenChanged?.Invoke();
        }
    }
}
