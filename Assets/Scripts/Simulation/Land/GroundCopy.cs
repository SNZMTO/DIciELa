using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// La copie d'une forme au sol par symétrie (décision 216) : tournée d'un angle autour d'un centre, ou retournée en
    /// miroir par un axe qui passe par lui. Les angles sont des caps, en degrés, vers l'est depuis le nord (Heading). La
    /// copie par défaut est la forme elle-même.
    /// </summary>
    public readonly struct GroundCopy
    {
        private GroundCopy(Vector2 center, float degrees, bool mirror)
        {
            Center = center;
            Degrees = degrees;
            IsMirror = mirror;
        }

        /// <summary>Tournée de cet angle autour de ce centre.</summary>
        public static GroundCopy Turned(Vector2 center, float degrees)
        {
            return new GroundCopy(center, degrees, false);
        }

        /// <summary>Retournée en miroir par l'axe de ce cap qui passe par ce centre.</summary>
        public static GroundCopy Mirrored(Vector2 center, float axisDegrees)
        {
            return new GroundCopy(center, axisDegrees, true);
        }

        public Vector2 Center { get; }

        /// <summary>L'angle dont elle tourne, ou le cap de l'axe du miroir.</summary>
        public float Degrees { get; }

        public bool IsMirror { get; }

        /// <summary>Ce point du sol, copié.</summary>
        public Vector2 Apply(Vector2 point)
        {
            Vector2 offset = point - Center;
            if (IsMirror)
            {
                Vector2 axis = Heading.Direction(Degrees);
                return Center + 2f * Vector2.Dot(offset, axis) * axis - offset;
            }
            float radians = Degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians), sin = Mathf.Sin(radians);
            return Center + new Vector2(offset.x * cos + offset.y * sin, offset.y * cos - offset.x * sin);
        }

        /// <summary>Ce cap, copié : celui d'une place de charrette, ou les côtés d'un rectangle.</summary>
        public float HeadingOf(float degrees)
        {
            return Mathf.Repeat(IsMirror ? 2f * Degrees - degrees : degrees + Degrees, 360f);
        }
    }
}
