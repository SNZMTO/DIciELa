using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Un outil de la roue des outils pour l'interface (décisions 168 et 221) : sa place sur le plateau (0 en haut, puis
    /// dans le sens des aiguilles d'une montre), son icône et sa couleur (des classes de la feuille de style), et les clés
    /// de son nom et de son usage, que dit la bulle du milieu. La table est ici, au rang du choix de chaque outil
    /// (ToolChoice) ; celle des gestes est ToolAction.
    /// </summary>
    internal readonly struct WheelTool
    {
        public static readonly IReadOnlyList<WheelTool> All = new[]
        {
            new WheelTool(ToolChoice.Build, 0, "marteau", "beurre", TextKeys.BuildTool, TextKeys.BuildToolDetail),
            new WheelTool(ToolChoice.Clear, 4, "hache", "menthe", TextKeys.ClearingTool, TextKeys.ClearingToolDetail),
            new WheelTool(ToolChoice.Move, 2, "fleches", "ciel", TextKeys.MoveTool, TextKeys.MoveToolDetail),
            new WheelTool(ToolChoice.Paths, 6, "chemins", "sable", TextKeys.PathsTool, TextKeys.PathsToolDetail),
            new WheelTool(ToolChoice.Plaza, 7, "roue-place", "peche", TextKeys.PlazaTool, TextKeys.PlazaToolDetail)
        };

        public readonly ToolChoice Choice;
        public readonly int Place;
        public readonly string Icon;
        public readonly string Pastel;
        public readonly string NameKey;
        public readonly string DetailKey;

        private WheelTool(ToolChoice choice, int place, string icon, string pastel, string nameKey, string detailKey)
        {
            Choice = choice;
            Place = place;
            Icon = icon;
            Pastel = pastel;
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
