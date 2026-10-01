using System;
using System.Collections.Generic;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les abords d'une construction, ou d'un arbre ou d'un rocher : les mailles libres qui le bordent (l'une de leurs
    /// quatre voisines est à lui). C'est là qu'on se tient pour le servir : prendre dans un stock, livrer un chantier, y
    /// travailler, abattre un arbre. On les cherche
    /// dans son empreinte élargie de deux mailles de chaque côté : une maille couverte déborde d'au plus une maille, et
    /// sa voisine d'une de plus. Rien n'est alloué : la liste est celle de l'appelant.
    /// </summary>
    public sealed class Surroundings
    {
        private const int MarginCells = 2;

        private readonly NavGrid _grid;
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();

        public Surroundings(NavGrid grid)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
        }

        /// <summary>Remplit cette liste des mailles libres qui bordent cette construction, rangée par rangée.</summary>
        public void Collect(Structure structure, List<Vector2Int> cells)
        {
            if (ReferenceEquals(structure, null))
                throw new ArgumentNullException(nameof(structure));
            Collect(PlacementRules.Footprint(structure.Definition, structure.Placement), structure.Id, cells);
        }

        /// <summary>
        /// Remplit cette liste des mailles libres qui bordent ce que cet occupant tient du maillage sous cette empreinte,
        /// rangée par rangée.
        /// </summary>
        public void Collect(RotatedRect footprint, int occupant, List<Vector2Int> cells)
        {
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            cells.Clear();
            float margin = 2f * MarginCells * _grid.Metrics.CellSize;
            var around = new RotatedRect(footprint.Center, footprint.Size + new Vector2(margin, margin),
                footprint.RotationDegrees);
            foreach (Vector2Int cell in _grid.CellsCoveredBy(around))
            {
                if (_grid.IsWalkable(cell) && _grid.Borders(cell, occupant))
                    cells.Add(cell);
            }
        }

        /// <summary>
        /// Le milieu de l'abord de cette construction le plus proche de ce point ; sans abord, ce point. À distance
        /// égale, le premier trouvé : le même à chaque fois.
        /// </summary>
        public Vector2 NearestTo(Structure structure, Vector2 point)
        {
            Collect(structure, _cells);
            Vector2 nearest = point;
            float best = float.MaxValue;
            for (int i = 0; i < _cells.Count; i++)
            {
                Vector2 center = _grid.CellCenter(_cells[i]);
                float distance = Vector2.Distance(center, point);
                if (distance < best)
                {
                    best = distance;
                    nearest = center;
                }
            }
            return nearest;
        }
    }
}
