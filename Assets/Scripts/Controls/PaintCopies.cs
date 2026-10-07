using System.Collections.Generic;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Les copies que la symétrie d'un outil de sol fait de ce qu'il trace, gomme ou pose (décision 216) : la forme
    /// elle-même d'abord ; en miroir, son reflet par l'axe de l'épingle ; en double miroir, ses reflets par les deux axes
    /// et son demi-tour ; en étoile, ses tours autour de l'épingle, à parts égales.
    /// </summary>
    internal static class PaintCopies
    {
        /// <summary>Met dans copies celles que demandent ces options, la forme elle-même la première.</summary>
        public static void Fill(PaintOptions options, List<GroundCopy> copies)
        {
            copies.Clear();
            float axis = options.AxisDegrees;
            Vector2 center = options.PinCenter;
            copies.Add(GroundCopy.Turned(center, 0f));
            switch (options.Symmetry)
            {
                case PaintSymmetry.Mirror:
                    copies.Add(GroundCopy.Mirrored(center, axis));
                    break;
                case PaintSymmetry.DoubleMirror:
                    copies.Add(GroundCopy.Mirrored(center, axis));
                    copies.Add(GroundCopy.Mirrored(center, axis + 90f));
                    copies.Add(GroundCopy.Turned(center, 180f));
                    break;
                case PaintSymmetry.Star:
                    for (int i = 1; i < options.StarCopies; i++)
                        copies.Add(GroundCopy.Turned(center, 360f * i / options.StarCopies));
                    break;
            }
        }

        /// <summary>Cette forme et ses copies, en une seule forme ; sans symétrie, la forme elle-même.</summary>
        public static GroundShape Apply(GroundShape shape, IReadOnlyList<GroundCopy> copies)
        {
            if (shape == null || copies.Count <= 1)
                return shape;
            var shapes = new GroundShape[copies.Count];
            shapes[0] = shape;
            for (int i = 1; i < copies.Count; i++)
                shapes[i] = shape.Copied(copies[i]);
            return new GroundUnion(shapes);
        }
    }
}
