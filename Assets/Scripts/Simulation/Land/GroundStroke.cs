using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Un trait de pinceau au sol, à main levée (décision 204 ; SAVOIR, 2.6) : une polyligne, en mètres, et sa
    /// demi-largeur ; il couvre ce qui est à moins de sa demi-largeur de la ligne. On en garde ce que le geste a laissé,
    /// jamais le geste lui-même. Ses mailles se cherchent segment par segment, chacun dans sa boîte : un long trait
    /// sinueux ne parcourt pas toute la boîte du trait à chaque segment (l'aperçu le refait pendant qu'on peint).
    /// </summary>
    public sealed class GroundStroke : GroundShape
    {
        private readonly Vector2[] _points;
        private readonly HashSet<int> _seen = new HashSet<int>();

        public GroundStroke(IReadOnlyList<Vector2> points, float halfWidth)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("Un trait a au moins un point.", nameof(points));
            if (!(halfWidth > 0f))
                throw new ArgumentOutOfRangeException(nameof(halfWidth), "Un trait a une largeur.");
            _points = new Vector2[points.Count];
            for (int i = 0; i < points.Count; i++)
                _points[i] = points[i];
            HalfWidth = halfWidth;
        }

        public IReadOnlyList<Vector2> Points => _points;
        public float HalfWidth { get; }

        public override GroundShape Copied(GroundCopy copy)
        {
            var points = new Vector2[_points.Length];
            for (int i = 0; i < points.Length; i++)
                points[i] = copy.Apply(_points[i]);
            return new GroundStroke(points, HalfWidth);
        }

        internal override void CoverCells(NavGrid grid, List<Vector2Int> cells)
        {
            _seen.Clear();
            var reach = new Vector2(HalfWidth, HalfWidth);
            float squared = HalfWidth * HalfWidth;
            // Un seul point est un segment de longueur nulle.
            int segments = Mathf.Max(1, _points.Length - 1);
            for (int s = 0; s < segments; s++)
            {
                Vector2 a = _points[s], b = _points[Mathf.Min(s + 1, _points.Length - 1)];
                Bounds(grid, Vector2.Min(a, b) - reach, Vector2.Max(a, b) + reach, out Vector2Int from,
                    out Vector2Int to);
                for (int y = from.y; y <= to.y; y++)
                {
                    for (int x = from.x; x <= to.x; x++)
                    {
                        var cell = new Vector2Int(x, y);
                        if (SquaredDistance(grid.CellCenter(cell), a, b) <= squared && _seen.Add(grid.IndexOf(cell)))
                            cells.Add(cell);
                    }
                }
            }
        }

        // Le carré de la distance de ce point au segment [a, b].
        private static float SquaredDistance(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 segment = b - a;
            float length = segment.sqrMagnitude;
            float along = length > 1e-8f ? Mathf.Clamp01(Vector2.Dot(point - a, segment) / length) : 0f;
            return (point - (a + segment * along)).sqrMagnitude;
        }
    }
}
