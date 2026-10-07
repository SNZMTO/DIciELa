using System.Collections.Generic;
using Diciela.Simulation.Land;
using UnityEngine;
using static Diciela.EditorTools.InkStrokes;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les icônes des outils de sol (décisions 206, 209 et 216), à l'encre sur 20 unités : la carte de la Place (une
    /// zone, son contour et deux places de charrette) ; leurs modes : le pinceau, la gomme, le tracé (des points reliés),
    /// la forme (un cercle sur un carré), la place de charrette (un cadre, sa pointe vers l'avant) ; les formes du mode
    /// Forme : le cercle, le rectangle, le polygone ; les styles d'un tracé : droit, en courbe ; et l'outil Chemins,
    /// qui partage leur roue : un chemin qui serpente, en pointillés. Des remplaçants simples : la maquette n'en a pas.
    /// Le menu des images de l'interface les génère avec les autres (InterfaceIcons) ; les traits sont ceux d'InkStrokes.
    /// </summary>
    internal static class PaintIcons
    {
        private const float Size = 20f;

        public static IEnumerable<InterfaceIcons.Icon> All()
        {
            yield return new InterfaceIcons.Icon("place", Size, Card());
            yield return new InterfaceIcons.Icon("pinceau", Size, Brush());
            yield return new InterfaceIcons.Icon("gomme", Size, Eraser());
            var circle = new List<InterfaceIcons.Face> { Polygon(Ink, 0.3f, Circle(10f, 10f, 6.5f, 24)) };
            circle.AddRange(Loop(Circle(10f, 10f, 7.5f, 24), 2f));
            yield return new InterfaceIcons.Icon("cercle", Size, circle.ToArray());
            var box = new List<InterfaceIcons.Face> { Polygon(Ink, 0.3f, 3f, 5f, 17f, 5f, 17f, 15f, 3f, 15f) };
            box.AddRange(Loop(new[] { 3f, 5f, 17f, 5f, 17f, 15f, 3f, 15f }, 2f));
            yield return new InterfaceIcons.Icon("rectangle", Size, box.ToArray());
            yield return new InterfaceIcons.Icon("place-charrette", Size, Spot());
            yield return new InterfaceIcons.Icon("trace", Size, Trace());
            yield return new InterfaceIcons.Icon("forme", Size, Shapes());
            yield return new InterfaceIcons.Icon("polygone", Size, Pentagon());
            yield return new InterfaceIcons.Icon("droit", Size, Line(false));
            yield return new InterfaceIcons.Icon("courbe", Size, Line(true));
            yield return new InterfaceIcons.Icon("chemins", Size, new[]
            {
                Bar(5f, 18f, 7f, 12.5f, 4.5f, 0.45f), Bar(7f, 12.5f, 12.5f, 9f, 4.5f, 0.45f),
                Bar(12.5f, 9f, 15f, 2.5f, 4.5f, 0.45f), Bar(5.8f, 15.6f, 6.3f, 14.2f, 1.2f),
                Bar(9.1f, 11.3f, 10.5f, 10.4f, 1.2f), Bar(13.6f, 6.6f, 14.1f, 5.2f, 1.2f)
            });
        }

        // Le pinceau, ses poils en bas à gauche.
        internal static InterfaceIcons.Face[] Brush()
        {
            return new[]
            {
                Bar(16.5f, 3.5f, 9.5f, 10.5f, 2.2f), Bar(9.8f, 10.2f, 7.6f, 12.4f, 3.4f, 0.6f),
                Polygon(Ink, 1f, Circle(5.6f, 14.4f, 3.4f, 12))
            };
        }

        // La gomme, penchée, sur le trait qu'elle efface.
        internal static InterfaceIcons.Face[] Eraser()
        {
            return new[]
            {
                Polygon(Ink, 0.5f, 2f, 13f, 11f, 4f, 15.5f, 8.5f, 6.5f, 17.5f),
                Polygon(Ink, 1f, 2f, 13f, 6.5f, 8.5f, 11f, 13f, 6.5f, 17.5f), Bar(6f, 18.6f, 18f, 18.6f, 1.2f)
            };
        }

        // La place de charrette : un cadre, sa pointe vers l'avant.
        internal static InterfaceIcons.Face[] Spot()
        {
            var spot = new List<InterfaceIcons.Face>(Loop(new[] { 6f, 2f, 14f, 2f, 14f, 18f, 6f, 18f }, 1.6f))
            {
                Polygon(Ink, 1f, 10f, 4.6f, 12.8f, 8.4f, 7.2f, 8.4f)
            };
            return spot.ToArray();
        }

        // Le tracé : quatre points, reliés de segments droits.
        private static InterfaceIcons.Face[] Trace()
        {
            float[] points = { 3f, 16f, 8f, 6.5f, 13f, 12.5f, 17f, 4f };
            var faces = new List<InterfaceIcons.Face>();
            for (int i = 0; i + 3 < points.Length; i += 2)
                faces.Add(Bar(points[i], points[i + 1], points[i + 2], points[i + 3], 1.4f, 0.7f));
            for (int i = 0; i + 1 < points.Length; i += 2)
                faces.Add(Polygon(Ink, 1f, Circle(points[i], points[i + 1], 1.9f, 10)));
            return faces.ToArray();
        }

        // La forme : un cercle voilé, posé sur un carré.
        private static InterfaceIcons.Face[] Shapes()
        {
            float[] square = { 8f, 8f, 17.5f, 8f, 17.5f, 17.5f, 8f, 17.5f };
            var faces = new List<InterfaceIcons.Face> { Polygon(Ink, 0.3f, square) };
            faces.AddRange(Loop(square, 1.6f));
            faces.Add(Polygon(Ink, 0.3f, Circle(7f, 7f, 4.6f, 20)));
            faces.AddRange(Loop(Circle(7f, 7f, 4.6f, 20), 1.6f));
            return faces.ToArray();
        }

        // Le polygone : cinq coins, voilé, son contour.
        private static InterfaceIcons.Face[] Pentagon()
        {
            float[] corners = { 10f, 2.5f, 17.5f, 8f, 14.5f, 17f, 5.5f, 17f, 2.5f, 8f };
            var faces = new List<InterfaceIcons.Face> { Polygon(Ink, 0.3f, corners) };
            faces.AddRange(Loop(corners, 1.6f));
            return faces.ToArray();
        }

        // Un trait par quatre points : droit, ou en courbe douce, la même que celle des tracés (GroundSpline).
        private static InterfaceIcons.Face[] Line(bool curved)
        {
            var points = new List<Vector2>
            {
                new Vector2(2.5f, 15.5f), new Vector2(8f, 5f), new Vector2(12f, 14f), new Vector2(17.5f, 4.5f)
            };
            var line = new List<Vector2>(points);
            if (curved)
                GroundSpline.Sample(points, false, 0.5f, line);
            var faces = new List<InterfaceIcons.Face>();
            for (int i = 0; i + 1 < line.Count; i++)
                faces.Add(Bar(line[i].x, line[i].y, line[i + 1].x, line[i + 1].y, 1.6f));
            return faces.ToArray();
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
    }
}
