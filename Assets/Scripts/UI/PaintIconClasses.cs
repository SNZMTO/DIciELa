using Diciela.Controls;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Les icônes des outils de sol (décision 221), écrites une seule fois pour la roue et la carte : des classes de la
    /// feuille de style déduites des noms, pour chaque mode, chaque forme, chaque style de tracé, chaque symétrie et la
    /// zone ; et le visage de ce qu'un outil pose, la zone ou la vignette d'un sol, la texture de son calque (un
    /// remplaçant).
    /// </summary>
    internal static class PaintIconClasses
    {
        public const string Zone = "place-icone-superficie";

        /// <summary>L'icône d'un mode (CartSpot → place-icone-cartspot).</summary>
        public static string Mode(PaintToolMode mode)
        {
            return "place-icone-" + mode.ToString().ToLowerInvariant();
        }

        /// <summary>L'icône d'une forme du mode Forme (Disc → place-forme-disc).</summary>
        public static string Shape(PaintShape shape)
        {
            return "place-forme-" + shape.ToString().ToLowerInvariant();
        }

        /// <summary>L'icône d'un style de tracé (Curved → place-style-curved).</summary>
        public static string Style(PaintTraceStyle style)
        {
            return "place-style-" + style.ToString().ToLowerInvariant();
        }

        /// <summary>L'icône d'une symétrie (DoubleMirror → place-symetrie-doublemirror).</summary>
        public static string Symmetry(PaintSymmetry symmetry)
        {
            return "place-symetrie-" + symmetry.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Le visage de ce qu'un outil pose : l'icône de la zone, ou la vignette d'un sol, la texture de son calque ;
        /// rien pour ce qui ne pose rien.
        /// </summary>
        public static HudFace Ink(PaintInk ink)
        {
            if (ink.IsZone)
                return HudFace.Styled(Zone);
            FloorDefinition floor = ink.Floor;
            return floor != null && floor.Layer != null ? HudFace.Photo(floor.Layer.diffuseTexture) : default;
        }
    }
}
