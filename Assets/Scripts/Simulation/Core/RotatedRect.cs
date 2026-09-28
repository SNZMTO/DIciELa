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
        // Ce qui se touche sur moins que cela, en mètres, ne se recouvre pas : deux bords communs restent permis.
        private const float Epsilon = 1e-4f;

        // L'angle se calcule une fois : le parcours du maillage demande le recouvrement de chaque maille.
        private readonly float _cos;
        private readonly float _sin;

        public RotatedRect(Vector2 center, Vector2 size, float rotationDegrees)
        {
            Center = center;
            Size = size;
            RotationDegrees = rotationDegrees;
            float angle = rotationDegrees * Mathf.Deg2Rad;
            _cos = Mathf.Cos(angle);
            _sin = Mathf.Sin(angle);
        }

        /// <summary>Le centre, en mètres sur le plan XZ (y du vecteur = z du monde).</summary>
        public Vector2 Center { get; }

        /// <summary>La largeur (x) et la profondeur (y), en mètres.</summary>
        public Vector2 Size { get; }

        public float RotationDegrees { get; }

        /// <summary>Un point exprimé dans le repère du rectangle, ramené dans le monde.</summary>
        public Vector2 ToWorld(Vector2 local)
        {
            return Center + new Vector2(local.x * _cos + local.y * _sin, -local.x * _sin + local.y * _cos);
        }

        /// <summary>
        /// Vrai si ce rectangle et ce carré posé droit (une maille) se recouvrent, sur plus qu'un bord commun. Deux
        /// rectangles se recouvrent s'ils se recouvrent vus le long de chacun de leurs côtés : les deux axes du monde,
        /// ceux du carré, et les deux axes du rectangle.
        /// </summary>
        public bool Overlaps(Vector2 squareCenter, float squareHalfSize)
        {
            float halfX = Size.x * 0.5f, halfY = Size.y * 0.5f;
            float cos = Mathf.Abs(_cos), sin = Mathf.Abs(_sin);
            Vector2 offset = squareCenter - Center;
            if (Mathf.Abs(offset.x) >= squareHalfSize + halfX * cos + halfY * sin - Epsilon)
                return false;
            if (Mathf.Abs(offset.y) >= squareHalfSize + halfX * sin + halfY * cos - Epsilon)
                return false;
            float squareAlongRect = squareHalfSize * (cos + sin);
            float localX = offset.x * _cos - offset.y * _sin;
            float localY = offset.x * _sin + offset.y * _cos;
            return Mathf.Abs(localX) < halfX + squareAlongRect - Epsilon &&
                   Mathf.Abs(localY) < halfY + squareAlongRect - Epsilon;
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
