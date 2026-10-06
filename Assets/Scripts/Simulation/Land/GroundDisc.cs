using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>Un disque qu'on peint au sol (une forme toute faite, décision 204) : son milieu et son rayon, en mètres.</summary>
    public sealed class GroundDisc : GroundShape
    {
        public GroundDisc(Vector2 center, float radius)
        {
            if (!(radius > 0f))
                throw new ArgumentOutOfRangeException(nameof(radius), "Un disque a un rayon.");
            Center = center;
            Radius = radius;
        }

        public Vector2 Center { get; }
        public float Radius { get; }

        internal override void CoverCells(NavGrid grid, List<Vector2Int> cells)
        {
            var reach = new Vector2(Radius, Radius);
            Bounds(grid, Center - reach, Center + reach, out Vector2Int from, out Vector2Int to);
            float squared = Radius * Radius;
            for (int y = from.y; y <= to.y; y++)
            {
                for (int x = from.x; x <= to.x; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if ((grid.CellCenter(cell) - Center).sqrMagnitude <= squared)
                        cells.Add(cell);
                }
            }
        }
    }
}
