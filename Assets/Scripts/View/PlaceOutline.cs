using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Le tour que l'aperçu de l'outil de la Place dessine au sol (décision 206) : rien, le cercle du pinceau ou du
    /// disque, le rectangle qu'on tire, ou une place de charrette (son cadre et sa pointe vers l'avant).
    /// </summary>
    public readonly struct PlaceOutline
    {
        public enum Kind
        {
            None = 0,
            Circle = 1,
            Box = 2,
            Mark = 3
        }

        private PlaceOutline(Kind shape, Vector2 center, float radius, RotatedRect rect)
        {
            Shape = shape;
            Center = center;
            Radius = radius;
            Rect = rect;
        }

        public Kind Shape { get; }
        public Vector2 Center { get; }
        public float Radius { get; }
        public RotatedRect Rect { get; }

        public static PlaceOutline None => default;

        public static PlaceOutline Circle(Vector2 center, float radius)
        {
            return new PlaceOutline(Kind.Circle, center, radius, default);
        }

        public static PlaceOutline Box(RotatedRect rect)
        {
            return new PlaceOutline(Kind.Box, rect.Center, 0f, rect);
        }

        public static PlaceOutline Mark(RotatedRect footprint)
        {
            return new PlaceOutline(Kind.Mark, footprint.Center, 0f, footprint);
        }
    }
}
