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

        public string KeyOfCard(int index)
        {
            return index >= 0 && index < 9 ? (index + 1).ToString() : "";
        }
    }
}
