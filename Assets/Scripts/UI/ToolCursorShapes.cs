using Diciela.Controls;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// La silhouette du curseur de chaque outil, et de chaque mode d'un outil de sol (GDD, section 16 ; décision 221) :
    /// une classe de la feuille de style, et sa pointe, là où l'outil agit, en pixels du curseur de 40 px. Le marteau
    /// (Construire), la hache (Retirer), la main (Déplacer) ; le pinceau, la gomme, la plume (le tracé), le viseur (la
    /// forme) et la place de charrette. Les dessins viennent du générateur des icônes (CursorIcons), sur 24 unités : une
    /// unité y vaut 5/3 de pixel.
    /// </summary>
    internal static class ToolCursorShapes
    {
        public static readonly ToolCursorShape Hammer = new ToolCursorShape("curseur-marteau", new Vector2(25f, 15f));
        public static readonly ToolCursorShape Axe = new ToolCursorShape("curseur-hache", new Vector2(31f, 13f));
        public static readonly ToolCursorShape Hand = new ToolCursorShape("curseur-main", new Vector2(20f, 27f));

        private static readonly ToolCursorShape Brush = new ToolCursorShape("curseur-pinceau", new Vector2(9f, 31f));
        private static readonly ToolCursorShape Eraser = new ToolCursorShape("curseur-gomme", new Vector2(10f, 29f));
        private static readonly ToolCursorShape Quill = new ToolCursorShape("curseur-plume", new Vector2(8f, 32f));
        private static readonly ToolCursorShape Crosshair = new ToolCursorShape("curseur-viseur", new Vector2(20f, 20f));
        private static readonly ToolCursorShape Spot =
            new ToolCursorShape("curseur-place-charrette", new Vector2(20f, 20f));

        /// <summary>La silhouette d'un mode d'outil de sol.</summary>
        public static ToolCursorShape Of(PaintToolMode mode)
        {
            switch (mode)
            {
                case PaintToolMode.Eraser:
                    return Eraser;
                case PaintToolMode.Trace:
                    return Quill;
                case PaintToolMode.Shape:
                    return Crosshair;
                case PaintToolMode.CartSpot:
                    return Spot;
                default:
                    return Brush;
            }
        }
    }
}
