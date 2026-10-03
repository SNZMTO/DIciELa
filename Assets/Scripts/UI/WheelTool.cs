using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Un outil de la roue pour l'interface (décision 168) : le nom de son élément dans Hud.uxml (sa touche s'écrit dans
    /// l'élément du même nom suivi de « -touche »), et les clés de son nom et de son usage, que dit la bulle du milieu. La
    /// table est ici, au rang du choix de chaque outil (ToolChoice) ; celle des gestes est ToolAction.
    /// </summary>
    internal readonly struct WheelTool
    {
        public static readonly IReadOnlyList<WheelTool> All = new[]
        {
            new WheelTool("roue-construire", TextKeys.BuildTool, TextKeys.BuildToolDetail),
            new WheelTool("roue-retirer", TextKeys.ClearingTool, TextKeys.ClearingToolDetail),
            new WheelTool("roue-deplacer", TextKeys.MoveTool, TextKeys.MoveToolDetail)
        };

        public readonly string ElementName;
        public readonly string NameKey;
        public readonly string DetailKey;

        private WheelTool(string elementName, string nameKey, string detailKey)
        {
            ElementName = elementName;
            NameKey = nameKey;
            DetailKey = detailKey;
        }

        /// <summary>L'outil de ce choix.</summary>
        public static WheelTool Of(ToolChoice choice)
        {
            return All[(int)choice];
        }
    }
}
