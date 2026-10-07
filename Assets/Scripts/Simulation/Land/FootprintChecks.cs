using System;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Ce que recouvre l'empreinte d'une construction (GDD, sections 2, 9 et 11) : une autre construction, la Place (on
    /// ne bâtit pas sur elle), un sol ou son fantôme (on l'efface d'abord, décision 209), un arbre ou un rocher, le devant
    /// d'une porte ; et si l'on traverse un rectangle, le devant de sa porte. Les règles de pose les lisent ; ce
    /// qu'occupe la construction déplacée ne compte pas.
    /// </summary>
    internal sealed class FootprintChecks
    {
        private readonly NavGrid _grid;
        private readonly Doorways _doorways;
        private readonly Floors _floors;

        public FootprintChecks(NavGrid grid, Doorways doorways, Floors floors)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _doorways = doorways ?? throw new ArgumentNullException(nameof(doorways));
            _floors = floors ?? throw new ArgumentNullException(nameof(floors));
        }

        /// <summary>
        /// L'empreinte ne recouvre ni une autre construction, ni la Place, ni un sol, ni un arbre ou un rocher, ni le devant
        /// d'une porte. Une construction dessous l'emporte : retirer l'arbre n'y changerait rien ; puis le sol, qu'on
        /// efface. La construction déplacée ne gêne pas : ni ses mailles, ni le devant de sa porte, s'il y en a une.
        /// </summary>
        public PlacementResult Check(RotatedRect footprint, Structure moved, RotatedRect? ownDoor)
        {
            bool onAFloor = false;
            bool natureInTheWay = false;
            bool blocksADoor = false;
            int self = moved == null ? 0 : moved.Id;
            float halfCell = _grid.Metrics.CellSize * 0.5f;
            foreach (Vector2Int cell in _grid.CellsCoveredBy(footprint))
            {
                int occupant = _grid.OccupantAt(cell);
                if (occupant == PlaceZone.Occupant)
                    return PlacementResult.OnThePlace;
                if (occupant > 0 && occupant != self)
                    return PlacementResult.Overlaps;
                if (occupant < 0)
                    natureInTheWay = true;
                if (_floors.HasFloor(_grid.IndexOf(cell)))
                    onAFloor = true;
                int doors = _doorways.CountAt(cell);
                if (ownDoor.HasValue && ownDoor.Value.Overlaps(_grid.CellCenter(cell), halfCell))
                    doors--;
                if (doors > 0)
                    blocksADoor = true;
            }
            if (onAFloor)
                return PlacementResult.OnAFloor;
            if (natureInTheWay)
                return PlacementResult.NatureInTheWay;
            return blocksADoor ? PlacementResult.BlocksADoor : PlacementResult.Ok;
        }

        /// <summary>
        /// Vrai si l'on traverse tout ce rectangle : le devant d'une porte, sur le sol libre ou sur la Place ; le sol que
        /// quitte la construction déplacée compte comme libre.
        /// </summary>
        public bool IsWalkable(RotatedRect rect, Structure moved)
        {
            int self = moved == null ? 0 : moved.Id;
            foreach (Vector2Int cell in _grid.CellsCoveredBy(rect))
            {
                if (!_grid.IsWalkable(cell) && (self == 0 || _grid.OccupantAt(cell) != self))
                    return false;
            }
            return true;
        }
    }
}
