using Diciela.Controls;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Les touches des outils, écrites ici.</summary>
    internal sealed class FixedToolKeys : IToolKeys
    {
        public string WheelKey => "Tab";

        public string KeyOf(ToolChoice choice)
        {
            switch (choice)
            {
                case ToolChoice.Build:
                    return "B";
                case ToolChoice.Clear:
                    return "X";
                case ToolChoice.Paths:
                    return "C";
                case ToolChoice.Plaza:
                    return "P";
                default:
                    return "M";
            }
        }

        public string KeyOfCard(int index)
        {
            return index >= 0 && index < 9 ? (index + 1).ToString() : "";
        }
    }
}
