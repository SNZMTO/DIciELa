using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les formes toutes faites d'un maillage drapé (décision 206) : le tour d'un rectangle tourné, celui d'un cercle,
    /// et une place de charrette, son cadre et la pointe qui montre l'avant de la charrette. Elles se font de traits et
    /// de pointes (DrapedMesh).
    /// </summary>
    internal static class DrapedShapes
    {
        // La pointe d'une place de charrette, en parts de sa demi-largeur : son écart au bord avant, sa longueur, sa
        // demi-base.
        private const float TipMargin = 0.2f;
        private const float TipLength = 0.6f;
        private const float TipHalfBase = 0.45f;

        /// <summary>Le tour d'un rectangle tourné, en traits de cette largeur.</summary>
        public static void AddOutline(this DrapedMesh mesh, RotatedRect rect, float width)
        {
            for (int corner = 0; corner < 4; corner++)
                mesh.AddSegment(rect.Corner(corner), rect.Corner((corner + 1) % 4), width);
        }

        /// <summary>Le tour d'un cercle, en traits de cette largeur, d'au moins seize côtés.</summary>
        public static void AddCircle(this DrapedMesh mesh, Vector2 center, float radius, float width)
        {
            int sides = Mathf.Clamp(Mathf.CeilToInt(2f * Mathf.PI * radius / DrapedMesh.Step), 16, 128);
            Vector2 previous = center + new Vector2(radius, 0f);
            for (int i = 1; i <= sides; i++)
            {
                float angle = 2f * Mathf.PI * i / sides;
                Vector2 next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                mesh.AddSegment(previous, next, width);
                previous = next;
            }
        }

        /// <summary>
        /// Une place de charrette : son cadre, et une pointe près de son bord avant, vers où l'avant de la charrette se
        /// gare (un remplaçant simple, comme le fanion des arbres).
        /// </summary>
        public static void AddMark(this DrapedMesh mesh, RotatedRect footprint, float width)
        {
            mesh.AddOutline(footprint, width);
            float halfWidth = footprint.Size.x * 0.5f, halfLength = footprint.Size.y * 0.5f;
            Vector2 tip = footprint.ToWorld(new Vector2(0f, halfLength - halfWidth * TipMargin));
            float back = halfLength - halfWidth * (TipMargin + TipLength);
            mesh.AddTip(tip, footprint.ToWorld(new Vector2(-halfWidth * TipHalfBase, back)),
                footprint.ToWorld(new Vector2(halfWidth * TipHalfBase, back)));
        }
    }
}
