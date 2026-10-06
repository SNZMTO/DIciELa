using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les icônes de la Place (décision 206), à l'encre sur 20 unités : sa carte (une zone, son contour et deux places de
    /// charrette), et les modes de son outil : le pinceau, la gomme, le cercle, le rectangle, la place de charrette (un
    /// cadre, sa pointe vers l'avant). Des remplaçants simples : la maquette n'en a pas. Le menu des images de
    /// l'interface les génère avec les autres (InterfaceIcons).
    /// </summary>
    internal static class PlaceIcons
    {
        private const float Size = 20f;
        private const string Ink = "encre";

        public static IEnumerable<InterfaceIcons.Icon> All()
        {
            yield return new InterfaceIcons.Icon("place", Size, Card());
            yield return new InterfaceIcons.Icon("pinceau", Size, new[]
            {
                Bar(16.5f, 3.5f, 9.5f, 10.5f, 2.2f), Bar(9.8f, 10.2f, 7.6f, 12.4f, 3.4f, 0.6f),
                Polygon(Ink, 1f, Circle(5.6f, 14.4f, 3.4f, 12))
            });
            yield return new InterfaceIcons.Icon("gomme", Size, new[]
            {
                Polygon(Ink, 0.5f, 2f, 13f, 11f, 4f, 15.5f, 8.5f, 6.5f, 17.5f),
                Polygon(Ink, 1f, 2f, 13f, 6.5f, 8.5f, 11f, 13f, 6.5f, 17.5f), Bar(6f, 18.6f, 18f, 18.6f, 1.2f)
            });
            var circle = new List<InterfaceIcons.Face> { Polygon(Ink, 0.3f, Circle(10f, 10f, 6.5f, 24)) };
            circle.AddRange(Loop(Circle(10f, 10f, 7.5f, 24), 2f));
            yield return new InterfaceIcons.Icon("cercle", Size, circle.ToArray());
            var box = new List<InterfaceIcons.Face> { Polygon(Ink, 0.3f, 3f, 5f, 17f, 5f, 17f, 15f, 3f, 15f) };
            box.AddRange(Loop(new[] { 3f, 5f, 17f, 5f, 17f, 15f, 3f, 15f }, 2f));
            yield return new InterfaceIcons.Icon("rectangle", Size, box.ToArray());
            var spot = new List<InterfaceIcons.Face>(Loop(new[] { 6f, 2f, 14f, 2f, 14f, 18f, 6f, 18f }, 1.6f))
            {
                Polygon(Ink, 1f, 10f, 4.6f, 12.8f, 8.4f, 7.2f, 8.4f)
            };
            yield return new InterfaceIcons.Icon("place-charrette", Size, spot.ToArray());
        }

        // Une zone de forme libre, voilée, son contour, et deux places de charrette côte à côte.
        private static InterfaceIcons.Face[] Card()
        {
            float[] zone = { 3f, 6f, 8f, 3f, 14f, 3.5f, 17.5f, 7f, 17f, 13f, 13f, 17f, 6f, 16.5f, 2.5f, 12f };
            var faces = new List<InterfaceIcons.Face> { Polygon(Ink, 0.3f, zone) };
            faces.AddRange(Loop(zone, 1.4f));
            faces.AddRange(Loop(new[] { 6.4f, 7f, 9.2f, 7f, 9.2f, 13f, 6.4f, 13f }, 0.9f));
            faces.AddRange(Loop(new[] { 11f, 7f, 13.8f, 7f, 13.8f, 13f, 11f, 13f }, 0.9f));
            return faces.ToArray();
        }

        private static InterfaceIcons.Face Polygon(string color, float opacity, params float[] points)
        {
            return new InterfaceIcons.Face(color, opacity, points);
        }

        // Un trait de cette largeur, de (x1, y1) à (x2, y2), prolongé d'une demi-largeur à chaque bout.
        private static InterfaceIcons.Face Bar(float x1, float y1, float x2, float y2, float width, float opacity = 1f)
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
        private static IEnumerable<InterfaceIcons.Face> Loop(float[] points, float width)
        {
            int count = points.Length / 2;
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                yield return Bar(points[2 * i], points[2 * i + 1], points[2 * next], points[2 * next + 1], width);
            }
        }

        private static float[] Circle(float x, float y, float radius, int sides)
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
