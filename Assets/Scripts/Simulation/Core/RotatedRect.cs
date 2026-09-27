using UnityEngine;

namespace Diciela.Simulation.Core
{
    /// <summary>
    /// Un rectangle posé au sol et tourné (la géométrie du socle, ARCHITECTURE, section 3) : une empreinte, ou la zone
    /// devant une porte. Dans son repère, x va vers sa droite et y vers son avant ; à l'angle 0, l'avant est le nord
    /// (+z) ; un angle positif tourne vers l'est, comme la caméra.
    /// </summary>
    public readonly struct RotatedRect
    {
        private const float Epsilon = 1e-4f;

        public RotatedRect(Vector2 center, Vector2 size, float rotationDegrees)
        {
            Center = center;
            Size = size;
            RotationDegrees = rotationDegrees;
        }

        /// <summary>Le centre, en mètres sur le plan XZ (y du vecteur = z du monde).</summary>
        public Vector2 Center { get; }

        /// <summary>La largeur (x) et la profondeur (y), en mètres.</summary>
        public Vector2 Size { get; }

        public float RotationDegrees { get; }

        /// <summary>Un point exprimé dans le repère du rectangle, ramené dans le monde.</summary>
        public Vector2 ToWorld(Vector2 local)
        {
            float angle = RotationDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
            return Center + new Vector2(local.x * cos + local.y * sin, -local.x * sin + local.y * cos);
        }

        public bool Contains(Vector2 point)
        {
            float angle = RotationDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
            Vector2 offset = point - Center;
            float localX = offset.x * cos - offset.y * sin;
            float localY = offset.x * sin + offset.y * cos;
            return Mathf.Abs(localX) <= Size.x * 0.5f + Epsilon && Mathf.Abs(localY) <= Size.y * 0.5f + Epsilon;
        }

        /// <summary>Le coin numéro 0 à 3 (dans l'ordre : arrière gauche, arrière droit, avant droit, avant gauche).</summary>
        public Vector2 Corner(int index)
        {
            float halfX = Size.x * 0.5f, halfY = Size.y * 0.5f;
            switch (index)
            {
                case 0: return ToWorld(new Vector2(-halfX, -halfY));
                case 1: return ToWorld(new Vector2(halfX, -halfY));
                case 2: return ToWorld(new Vector2(halfX, halfY));
                default: return ToWorld(new Vector2(-halfX, halfY));
            }
        }
    }
}
