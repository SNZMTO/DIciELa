using System;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// La sortie d'une construction posée sur un piéton (GDD, section 9 ; décision 88) : la maille libre par où il en
    /// sort, tout droit, en la traversant. Elle borde cette construction (l'une de ses quatre voisines en fait partie) :
    /// il ne vise jamais derrière une autre, mais sa ligne droite peut effleurer le coin d'une voisine collée à celle
    /// qu'il quitte (SAVOIR, 2.1). Parmi ces mailles, celles du plus grand îlot, jamais une poche que la construction
    /// vient de fermer ; sur cet îlot, la plus proche de lui. On cherche par anneaux autour de sa maille. Une
    /// construction est d'un seul tenant : un anneau où il n'en reste aucune maille dit qu'elle est finie, et que toutes
    /// les mailles qui la bordent ont été vues. Rien n'est alloué.
    /// </summary>
    public sealed class WayOutFinder
    {
        private readonly NavGrid _grid;
        private readonly Islands _islands;

        public WayOutFinder(NavGrid grid, Islands islands)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (ReferenceEquals(islands, null))
                throw new ArgumentNullException(nameof(islands));

            _grid = grid;
            _islands = islands;
        }

        /// <summary>
        /// La maille par où sortir de la construction qui couvre ce point ; faux si rien ne le couvre, ou si aucune
        /// maille libre ne la borde (il y est enfermé). À îlot et distance égaux, la première trouvée : la même à chaque
        /// fois.
        /// </summary>
        public bool TryFind(Vector2 point, out Vector2Int exit)
        {
            exit = default;
            Vector2Int center = _grid.CellOf(point);
            if (!_grid.Contains(center) || _grid.IsWalkable(center))
                return false;

            int occupant = _grid.OccupantAt(center);
            bool found = false;
            int bestSize = 0;
            float bestDistance = float.MaxValue;
            int maxRings = Math.Max(_grid.Width, _grid.Depth);
            for (int ring = 1; ring <= maxRings; ring++)
            {
                bool stillInside = false;
                for (int dy = -ring; dy <= ring; dy++)
                {
                    // Sur les rangées du bas et du haut de l'anneau, toutes les mailles ; entre elles, les deux des bords.
                    int stride = dy == -ring || dy == ring ? 1 : 2 * ring;
                    for (int dx = -ring; dx <= ring; dx += stride)
                    {
                        var cell = new Vector2Int(center.x + dx, center.y + dy);
                        if (!_grid.Contains(cell))
                            continue;
                        if (_grid.OccupantAt(cell) == occupant)
                        {
                            stillInside = true;
                            continue;
                        }
                        if (!_grid.IsWalkable(cell) || !_grid.Borders(cell, occupant))
                            continue;
                        int size = _islands.SizeOf(_islands.IslandOf(cell));
                        float distance = Vector2.Distance(_grid.CellCenter(cell), point);
                        if (size > bestSize || (size == bestSize && distance < bestDistance))
                        {
                            found = true;
                            bestSize = size;
                            bestDistance = distance;
                            exit = cell;
                        }
                    }
                }
                // Les mailles qui la bordent sont au plus un anneau plus loin que sa dernière maille.
                if (!stillInside)
                    break;
            }
            return found;
        }
    }
}
