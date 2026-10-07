using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Plusieurs formes peintes d'un seul geste (décision 216 : la symétrie, qui recopie ce qu'on trace) : elles couvrent
    /// ensemble leurs mailles, chacune une fois.
    /// </summary>
    public sealed class GroundUnion : GroundShape
    {
        private readonly GroundShape[] _shapes;
        private readonly List<Vector2Int> _own = new List<Vector2Int>();
        private readonly HashSet<Vector2Int> _seen = new HashSet<Vector2Int>();

        public GroundUnion(IReadOnlyList<GroundShape> shapes)
        {
            if (shapes == null || shapes.Count == 0)
                throw new ArgumentException("Une union a au moins une forme.", nameof(shapes));
            _shapes = new GroundShape[shapes.Count];
            for (int i = 0; i < shapes.Count; i++)
                _shapes[i] = shapes[i] ?? throw new ArgumentException("Une union n'a pas de forme vide.", nameof(shapes));
        }

        public IReadOnlyList<GroundShape> Shapes => _shapes;

        public override GroundShape Copied(GroundCopy copy)
        {
            var copies = new GroundShape[_shapes.Length];
            for (int i = 0; i < _shapes.Length; i++)
                copies[i] = _shapes[i].Copied(copy);
            return new GroundUnion(copies);
        }

        internal override void CoverCells(NavGrid grid, List<Vector2Int> cells)
        {
            _seen.Clear();
            foreach (GroundShape shape in _shapes)
            {
                _own.Clear();
                shape.CoverCells(grid, _own);
                foreach (Vector2Int cell in _own)
                {
                    if (_seen.Add(cell))
                        cells.Add(cell);
                }
            }
        }
    }
}
