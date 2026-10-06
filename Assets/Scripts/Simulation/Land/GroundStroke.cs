using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Un trait de pinceau au sol, à main levée (décision 204 ; SAVOIR, 2.6) : une polyligne, en mètres, et sa
    /// demi-largeur ; il couvre ce qui est à moins de sa demi-largeur de la ligne. On en garde ce que le geste a laissé,
    /// jamais le geste lui-même.
    /// </summary>
    public sealed class GroundStroke : GroundShape
    {
        private readonly Vector2[] _points;

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

        internal override void CoverCells(NavGrid grid, List<Vector2Int> cells)
        {
            Vector2 min = _points[0], max = _points[0];
            for (int i = 1; i < _points.Length; i++)
            {
                min = Vector2.Min(min, _points[i]);
                max = Vector2.Max(max, _points[i]);
            }
            var reach = new Vector2(HalfWidth, HalfWidth);
            Bounds(grid, min - reach, max + reach, out Vector2Int from, out Vector2Int to);
            float squared = HalfWidth * HalfWidth;
            for (int y = from.y; y <= to.y; y++)
            {
                for (int x = from.x; x <= to.x; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (SquaredDistanceTo(grid.CellCenter(cell)) <= squared)
                        cells.Add(cell);
                }
            }
        }

        // Le carré de la distance de ce point à la ligne : au plus proche de ses segments, ou de son seul point.
        private float SquaredDistanceTo(Vector2 point)
        {
            if (_points.Length == 1)
                return (point - _points[0]).sqrMagnitude;
            float best = float.MaxValue;
            for (int i = 1; i < _points.Length; i++)
            {
                Vector2 a = _points[i - 1], segment = _points[i] - a;
                float length = segment.sqrMagnitude;
                float along = length > 1e-8f ? Mathf.Clamp01(Vector2.Dot(point - a, segment) / length) : 0f;
                best = Mathf.Min(best, (point - (a + segment * along)).sqrMagnitude);
            }
            return best;
        }
    }
}
