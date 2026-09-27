using System;
using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les constructions posées sur la carte, et les règles de leur pose (GDD, sections 2 et 9) : dans la carte, sur un
    /// sol assez plat, sans rien recouvrir, sans se poser devant une porte, et la sienne dégagée. Une seule fonction
    /// répond, pour l'aperçu comme pour l'ordre. Elle tient le maillage à jour, qui en est déduit.
    /// </summary>
    public sealed class Structures
    {
        private readonly List<Structure> _all = new List<Structure>();
        private readonly NavGrid _grid;
        private readonly Relief _relief;
        private readonly float _maxSlopeDegrees;
        private readonly float _doorClearanceMeters;
        private int _nextId = 1;

        public Structures(NavGrid grid, Relief relief, SimulationSettings settings)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));
            if (ReferenceEquals(settings, null))
                throw new ArgumentNullException(nameof(settings));

            _grid = grid;
            _relief = relief;
            _maxSlopeDegrees = settings.MaxBuildSlopeDegrees;
            _doorClearanceMeters = settings.DoorClearanceMeters;
        }

        /// <summary>Levé quand une construction est posée.</summary>
        public event Action<Structure> StructurePlaced;

        /// <summary>Levé quand une pose est refusée, avec sa fiche, sa place, son angle et la raison.</summary>
        public event Action<StructureDefinition, Vector2, float, PlacementResult> PlacementRefused;

        public IReadOnlyList<Structure> All => _all;

        /// <summary>Peut-on poser cette construction ici, à cet angle ? La même réponse pour l'aperçu et pour l'ordre.</summary>
        public PlacementResult CheckPlacement(StructureDefinition definition, Vector2 center, float rotationDegrees)
        {
            if (ReferenceEquals(definition, null))
                throw new ArgumentNullException(nameof(definition));

            RotatedRect footprint = Footprint(definition, center, rotationDegrees);
            if (!IsInsideMap(footprint))
                return PlacementResult.OutsideMap;
            if (definition.HasDoor && !IsInsideMap(Doorway(definition, center, rotationDegrees)))
                return PlacementResult.OutsideMap;
            if (IsTooSteep(footprint))
                return PlacementResult.TooSteep;

            PlacementResult onTheGround = CheckFootprintCells(footprint);
            if (onTheGround != PlacementResult.Ok)
                return onTheGround;
            if (definition.HasDoor && IsOccupied(Doorway(definition, center, rotationDegrees)))
                return PlacementResult.DoorBlocked;
            return PlacementResult.Ok;
        }

        /// <summary>Pose la construction si c'est permis ; sinon, lève <see cref="PlacementRefused"/>.</summary>
        public PlacementResult Place(StructureDefinition definition, Vector2 center, float rotationDegrees)
        {
            PlacementResult result = CheckPlacement(definition, center, rotationDegrees);
            if (result != PlacementResult.Ok)
            {
                PlacementRefused?.Invoke(definition, center, rotationDegrees, result);
                return result;
            }

            var structure = new Structure(_nextId++, definition, center, rotationDegrees);
            _all.Add(structure);
            _grid.SetOccupant(Footprint(definition, center, rotationDegrees), structure.Id);
            if (definition.HasDoor)
                _grid.AddDoorway(Doorway(definition, center, rotationDegrees));
            StructurePlaced?.Invoke(structure);
            return result;
        }

        /// <summary>L'empreinte au sol de cette construction à cette place.</summary>
        public static RotatedRect Footprint(StructureDefinition definition, Vector2 center, float rotationDegrees)
        {
            return new RotatedRect(center, new Vector2(definition.WidthMeters, definition.DepthMeters), rotationDegrees);
        }

        /// <summary>La zone qui reste libre devant la porte : sur la largeur de la porte, au nord de l'empreinte à l'angle 0.</summary>
        public RotatedRect Doorway(StructureDefinition definition, Vector2 center, float rotationDegrees)
        {
            RotatedRect footprint = Footprint(definition, center, rotationDegrees);
            var local = new Vector2(definition.DoorOffsetMeters, (definition.DepthMeters + _doorClearanceMeters) * 0.5f);
            return new RotatedRect(footprint.ToWorld(local), new Vector2(definition.DoorWidthMeters, _doorClearanceMeters),
                rotationDegrees);
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
            _grid.CellRange(footprint, out int minX, out int minZ, out int maxX, out int maxZ);
            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 cell = _grid.CellCenter(x, z);
                    if (footprint.Contains(cell) && _relief.SlopeDegreesAt(cell.x, cell.y) > _maxSlopeDegrees)
                        return true;
                }
            }
            return false;
        }

        /// <summary>Une empreinte ne recouvre ni une autre construction, ni le devant d'une porte.</summary>
        private PlacementResult CheckFootprintCells(RotatedRect footprint)
        {
            bool blocksADoor = false;
            _grid.CellRange(footprint, out int minX, out int minZ, out int maxX, out int maxZ);
            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (!footprint.Contains(_grid.CellCenter(x, z)))
                        continue;
                    if (_grid.OccupantAt(x, z) != 0)
                        return PlacementResult.Overlaps;
                    if (_grid.IsDoorway(x, z))
                        blocksADoor = true;
                }
            }
            return blocksADoor ? PlacementResult.BlocksADoor : PlacementResult.Ok;
        }

        private bool IsOccupied(RotatedRect rect)
        {
            _grid.CellRange(rect, out int minX, out int minZ, out int maxX, out int maxZ);
            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (rect.Contains(_grid.CellCenter(x, z)) && _grid.OccupantAt(x, z) != 0)
                        return true;
                }
            }
            return false;
        }
    }
}
