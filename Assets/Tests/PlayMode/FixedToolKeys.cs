using Diciela.Controls;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Les touches des outils, écrites ici.</summary>
    internal sealed class FixedToolKeys : IToolKeys
    {
        public string WheelKey => "Tab";

        public string KeyOf(ToolChoice choice)
        {
            return choice == ToolChoice.Build ? "B" : "X";
        }
    }
}
