using Diciela.Data;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Ce qu'on lit de la forme d'une pièce du pack pour son réglage de départ (AttachmentFitter) : sa frange (une
    /// coiffure) et si son fond est fermé (un chapeau low poly que la tête traverse).
    /// </summary>
    internal static class PieceShape
    {
        // Le fond est fermé quand, sous le milieu de la pièce, le premier triangle vu d'en bas est dans ce bas de sa hauteur.
        private const float ClosedBottomShare = 0.2f;

        /// <summary>Le bas de la moitié avant d'une coiffure, autour de son pivot : sa frange.</summary>
        public static float Fringe(PackMeasures.Geometry geometry)
        {
            float fringe = float.MaxValue;
            foreach (Vector3 position in geometry.Positions)
            {
                if (position.z > geometry.Box.center.z)
                    fringe = Mathf.Min(fringe, position.y);
            }
            return fringe == float.MaxValue ? geometry.Box.min.y : fringe;
        }

        /// <summary>Vrai si, sous son milieu, la face du dessous de la pièce est tout en bas : son fond est fermé.</summary>
        public static bool ClosedBottom(SurfaceMap under, Bounds box)
        {
            float bottom = under.Sample(new Vector2(box.center.x, box.center.z));
            return bottom > SurfaceMap.Empty && bottom < box.min.y + ClosedBottomShare * box.size.y;
        }

    }
}
