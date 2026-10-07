using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les traits à l'encre des icônes des outils de sol (PaintIcons, SymmetryIcons) : une face pleine, un trait, un tour
    /// fermé, des pointillés, les points d'un cercle. Les coordonnées vont de gauche à droite et de haut en bas.
    /// </summary>
    internal static class InkStrokes
    {
        public const string Ink = "encre";

        public static InterfaceIcons.Face Polygon(string color, float opacity, params float[] points)
        {
            return new InterfaceIcons.Face(color, opacity, points);
        }

        // Un trait de cette largeur, de (x1, y1) à (x2, y2), prolongé d'une demi-largeur à chaque bout.
        public static InterfaceIcons.Face Bar(float x1, float y1, float x2, float y2, float width, float opacity = 1f)
        {
            var a = new Vector2(x1, y1);
            var b = new Vector2(x2, y2);
            Vector2 along = (b - a).normalized * (width * 0.5f);
            var side = new Vector2(-along.y, along.x);
            a -= along;
            b += along;
            return Polygon(Ink, opacity, a.x + side.x, a.y + side.y, b.x + side.x, b.y + side.y, b.x - side.x,
                b.y - side.y, a.x - side.x, a.y - side.y);
        }

        // Le tour fermé de ces points (x, y à la suite), en traits de cette largeur.
        public static IEnumerable<InterfaceIcons.Face> Loop(float[] points, float width)
        {
            int count = points.Length / 2;
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                yield return Bar(points[2 * i], points[2 * i + 1], points[2 * next], points[2 * next + 1], width);
            }
        }

        // Une ligne en pointillés de (x1, y1) à (x2, y2) : des tirets de cette longueur, séparés d'autant.
        public static IEnumerable<InterfaceIcons.Face> Dashes(float x1, float y1, float x2, float y2, float dash,
            float width)
        {
            var a = new Vector2(x1, y1);
            Vector2 line = new Vector2(x2, y2) - a;
            int count = Mathf.Max(1, Mathf.RoundToInt((line.magnitude / dash + 1f) / 2f));
            Vector2 step = line / (2 * count - 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 start = a + step * (2 * i);
                Vector2 end = start + step;
                yield return Bar(start.x, start.y, end.x, end.y, width);
            }
        }

        public static float[] Circle(float x, float y, float radius, int sides)
        {
            var points = new float[sides * 2];
            for (int i = 0; i < sides; i++)
            {
                float angle = 2f * Mathf.PI * i / sides;
                points[2 * i] = x + Mathf.Cos(angle) * radius;
                points[2 * i + 1] = y + Mathf.Sin(angle) * radius;
            }
            return points;
        }
    }
}
