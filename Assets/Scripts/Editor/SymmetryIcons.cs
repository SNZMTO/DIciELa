using System.Collections.Generic;
using UnityEngine;
using static Diciela.EditorTools.InkStrokes;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les icônes des symétries des outils de sol (décision 219), à l'encre sur 20 unités : une aile seule (sans
    /// symétrie) ; l'aile et sa copie de part et d'autre d'un axe en pointillés (miroir) ; quatre ailes autour de deux
    /// axes (double miroir) ; cinq pétales autour de l'épingle (étoile). La forme est pleine, ses copies voilées. Des
    /// remplaçants simples : la maquette n'en a pas. Le menu des images de l'interface les génère avec les autres
    /// (InterfaceIcons).
    /// </summary>
    internal static class SymmetryIcons
    {
        private const float Size = 20f;
        private const float Middle = 10f;
        private const float CopyOpacity = 0.45f;
        private const float Dash = 2f;
        private const float AxisWidth = 1.1f;

        public static IEnumerable<InterfaceIcons.Icon> All()
        {
            float[] wing = { 3f, 14f, 4.5f, 4.5f, 8.5f, 8.5f, 7.5f, 16f };
            yield return new InterfaceIcons.Icon("sans-symetrie", Size, new[] { Polygon(Ink, 1f, Flip(wing, 4f)) });
            var mirror = new List<InterfaceIcons.Face>
            {
                Polygon(Ink, 1f, wing), Polygon(Ink, CopyOpacity, Flip(wing, across: true))
            };
            mirror.AddRange(Dashes(Middle, 1.5f, Middle, 18.5f, Dash, AxisWidth));
            yield return new InterfaceIcons.Icon("miroir", Size, mirror.ToArray());
            float[] corner = { 2.5f, 7.5f, 3.5f, 2.5f, 8f, 4f, 7f, 8.5f };
            var twice = new List<InterfaceIcons.Face>
            {
                Polygon(Ink, 1f, corner), Polygon(Ink, CopyOpacity, Flip(corner, across: true)),
                Polygon(Ink, CopyOpacity, Flip(corner, down: true)),
                Polygon(Ink, CopyOpacity, Flip(corner, across: true, down: true))
            };
            twice.AddRange(Dashes(Middle, 1.5f, Middle, 18.5f, Dash, AxisWidth));
            twice.AddRange(Dashes(1.5f, Middle, 18.5f, Middle, Dash, AxisWidth));
            yield return new InterfaceIcons.Icon("double-miroir", Size, twice.ToArray());
            yield return new InterfaceIcons.Icon("etoile", Size, Star(5));
        }

        // Des pétales autour de l'épingle, le premier vers le haut, plein, les autres voilés.
        private static InterfaceIcons.Face[] Star(int petals)
        {
            float[] petal = { 0f, -8.5f, 2.3f, -5f, 0f, -2.8f, -2.3f, -5f };
            var faces = new List<InterfaceIcons.Face>();
            for (int i = 0; i < petals; i++)
            {
                float angle = 2f * Mathf.PI * i / petals;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                var turned = new float[petal.Length];
                for (int p = 0; p < petal.Length; p += 2)
                {
                    turned[p] = Middle + petal[p] * cos - petal[p + 1] * sin;
                    turned[p + 1] = Middle + petal[p] * sin + petal[p + 1] * cos;
                }
                faces.Add(Polygon(Ink, i == 0 ? 1f : CopyOpacity, turned));
            }
            faces.Add(Polygon(Ink, 1f, Circle(Middle, Middle, 1.3f, 10)));
            return faces.ToArray();
        }

        // Ces points (x, y à la suite) décalés d'autant vers la droite, puis retournés de part et d'autre du milieu : de
        // gauche à droite, de haut en bas, ou les deux.
        private static float[] Flip(float[] points, float right = 0f, bool across = false, bool down = false)
        {
            var flipped = new float[points.Length];
            for (int p = 0; p < points.Length; p += 2)
            {
                float x = points[p] + right;
                flipped[p] = across ? 2f * Middle - x : x;
                flipped[p + 1] = down ? 2f * Middle - points[p + 1] : points[p + 1];
            }
            return flipped;
        }
    }
}
