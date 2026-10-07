using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Une vue du curseur des outils qui note ce que le présentateur lui montre.</summary>
    internal sealed class NotedToolCursorView : IToolCursorView
    {
        /// <summary>L'allure montrée ; null pour la flèche.</summary>
        public ToolCursorLook? Look { get; private set; }

        public bool IsRefused { get; private set; }

        public void ShowArrow()
        {
            Look = null;
        }

        public void ShowLook(ToolCursorLook look)
        {
            Look = look;
        }

        public void ShowRefused(bool refused)
        {
            IsRefused = refused;
        }
    }
}
