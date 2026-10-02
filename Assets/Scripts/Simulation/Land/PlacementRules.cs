using System;
using Diciela.Data;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les règles de pose (GDD, sections 2, 9 et 13) : une construction que la ville sait bâtir, et unique, pas encore
    /// posée ; dans la carte, sur un sol assez plat, sans rien recouvrir, sans se poser devant une porte, et la sienne
    /// dégagée. Une seule fonction répond, pour l'aperçu comme pour l'ordre. Les règles disent aussi quel sol une
    /// construction occupe (son empreinte) et lequel elle garde libre (le devant de sa porte). Elles lisent le maillage,
    /// le relief, les constructions posées et le savoir de la ville, sans rien changer ; les constructions les créent.
    /// </summary>
    public sealed class PlacementRules
    {
        private readonly NavGrid _grid;
        private readonly Relief _relief;
        private readonly Structures _structures;
        private readonly Knowledge _knowledge;
        private readonly float _maxSlopeDegrees;
        private readonly float _doorClearanceMeters;

        internal PlacementRules(NavGrid grid, Relief relief, SimulationSettings settings, Structures structures,
            Knowledge knowledge)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));
            if (ReferenceEquals(settings, null))
                throw new ArgumentNullException(nameof(settings));
            if (ReferenceEquals(structures, null))
                throw new ArgumentNullException(nameof(structures));
            if (ReferenceEquals(knowledge, null))
                throw new ArgumentNullException(nameof(knowledge));

            _grid = grid;
            _relief = relief;
            _structures = structures;
            _knowledge = knowledge;
            _maxSlopeDegrees = settings.MaxBuildSlopeDegrees;
            _doorClearanceMeters = settings.DoorClearanceMeters;
        }

        /// <summary>
        /// Vrai si cette construction peut encore se poser quelque part : la ville sait la bâtir (GDD, section 13), et
        /// unique, elle n'est pas encore posée (GDD, section 9). Le panneau ne propose pas ce qui ne se pose pas.
        /// </summary>
        public bool IsAvailable(StructureDefinition definition)
        {
            return AvailabilityOf(definition) == PlacementResult.Ok;
        }

        /// <summary>Peut-on poser cette construction à cette place ? La même réponse pour l'aperçu et pour l'ordre.</summary>
        public PlacementResult Check(StructureDefinition definition, Placement placement)
        {
            PlacementResult availability = AvailabilityOf(definition);
            if (availability != PlacementResult.Ok)
                return availability;

            RotatedRect footprint = Footprint(definition, placement);
            if (!IsInsideMap(footprint))
                return PlacementResult.OutsideMap;
            if (definition.HasDoor && !IsInsideMap(Doorway(definition, placement)))
                return PlacementResult.OutsideMap;
            if (IsTooSteep(footprint))
                return PlacementResult.TooSteep;

            PlacementResult onTheGround = CheckFootprintCells(footprint);
            if (onTheGround != PlacementResult.Ok)
                return onTheGround;
            if (definition.HasDoor && IsOccupied(Doorway(definition, placement)))
                return PlacementResult.DoorBlocked;
            return PlacementResult.Ok;
        }

        // Ce qui empêche cette construction de se poser où que ce soit : qu'on ne sache pas la bâtir, ou qu'elle soit unique
        // et déjà posée.
        private PlacementResult AvailabilityOf(StructureDefinition definition)
        {
            if (ReferenceEquals(definition, null))
                throw new ArgumentNullException(nameof(definition));
            if (!_knowledge.Knows(definition))
                return PlacementResult.Unknown;
            return definition.IsUnique && _structures.IsPlaced(definition)
                ? PlacementResult.AlreadyPlaced
                : PlacementResult.Ok;
        }

        /// <summary>L'empreinte au sol de cette construction à cette place.</summary>
        public static RotatedRect Footprint(StructureDefinition definition, Placement placement)
        {
            return new RotatedRect(placement.Center, new Vector2(definition.WidthMeters, definition.DepthMeters),
                placement.RotationDegrees);
        }

        /// <summary>La zone qui reste libre devant la porte : sur sa largeur, au nord de l'empreinte à l'angle 0.</summary>
        public RotatedRect Doorway(StructureDefinition definition, Placement placement)
        {
            RotatedRect footprint = Footprint(definition, placement);
            var local = new Vector2(definition.DoorOffsetMeters, (definition.DepthMeters + _doorClearanceMeters) * 0.5f);
            return new RotatedRect(footprint.ToWorld(local), new Vector2(definition.DoorWidthMeters, _doorClearanceMeters),
                placement.RotationDegrees);
        }

        private bool IsInsideMap(RotatedRect rect)
        {
            for (int corner = 0; corner < 4; corner++)
            {
                Vector2 point = rect.Corner(corner);
                if (!_relief.Contains(point.x, point.y))
                    return false;
            }
            return true;
        }

        private bool IsTooSteep(RotatedRect footprint)
        {
            foreach (Vector2Int cell in _grid.CellsCoveredBy(footprint))
            {
                Vector2 point = _grid.CellCenter(cell);
                if (_relief.SlopeDegreesAt(point.x, point.y) > _maxSlopeDegrees)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Une empreinte ne recouvre ni une autre construction, ni un arbre ou un rocher, ni le devant d'une porte. Une
        /// construction dessous l'emporte : retirer l'arbre n'y changerait rien.
        /// </summary>
        private PlacementResult CheckFootprintCells(RotatedRect footprint)
        {
            bool natureInTheWay = false;
            bool blocksADoor = false;
            foreach (Vector2Int cell in _grid.CellsCoveredBy(footprint))
            {
                int occupant = _grid.OccupantAt(cell);
                if (occupant > 0)
                    return PlacementResult.Overlaps;
                if (occupant < 0)
                    natureInTheWay = true;
                if (_grid.IsDoorway(cell))
                    blocksADoor = true;
            }
            if (natureInTheWay)
                return PlacementResult.NatureInTheWay;
            return blocksADoor ? PlacementResult.BlocksADoor : PlacementResult.Ok;
        }

        private bool IsOccupied(RotatedRect rect)
        {
            foreach (Vector2Int cell in _grid.CellsCoveredBy(rect))
            {
                if (_grid.OccupantAt(cell) != 0)
                    return true;
            }
            return false;
        }
    }
}
