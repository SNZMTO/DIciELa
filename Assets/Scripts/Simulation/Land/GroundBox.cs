using System;
using System.Collections.Generic;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>Un rectangle qu'on peint au sol (une forme toute faite, décision 204), tourné comme on veut.</summary>
    public sealed class GroundBox : GroundShape
    {
        public GroundBox(RotatedRect rect)
        {
            if (!(rect.Size.x > 0f) || !(rect.Size.y > 0f))
                throw new ArgumentOutOfRangeException(nameof(rect), "Un rectangle a une largeur et une profondeur.");
            Rect = rect;
        }

        public RotatedRect Rect { get; }

        internal override void CoverCells(NavGrid grid, List<Vector2Int> cells)
        {
            Vector2 min = Rect.Corner(0), max = min;
            for (int corner = 1; corner < 4; corner++)
            {
                min = Vector2.Min(min, Rect.Corner(corner));
                max = Vector2.Max(max, Rect.Corner(corner));
            }
            Bounds(grid, min, max, out Vector2Int from, out Vector2Int to);
            for (int y = from.y; y <= to.y; y++)
            {
                for (int x = from.x; x <= to.x; x++)
                {
                    // Un carré de taille nulle : le milieu de la maille est dans le rectangle.
                    var cell = new Vector2Int(x, y);
                    if (Rect.Overlaps(grid.CellCenter(cell), 0f))
                        cells.Add(cell);
                }
            }
        }
    }
}
