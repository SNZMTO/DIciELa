using System;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Un panneau Construire qui s'ouvre et se ferme, sans rien montrer ; la Place proposée ou non, au choix du test.</summary>
    internal sealed class NotedBuildMenu : IBuildMenu
    {
        public event Action OpenChanged;

        public bool IsOpen { get; private set; }

        public bool OffersPlaza { get; set; } = true;

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
