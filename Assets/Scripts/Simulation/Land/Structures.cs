using System;
using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les constructions posées sur la carte (ARCHITECTURE, section 4) : la vérité, dont le maillage est déduit et tenu
    /// à jour ici. Une pose passe par les règles de pose ; refusée, elle ne change rien et dit pourquoi.
    /// </summary>
    public sealed class Structures
    {
        private readonly List<Structure> _all = new List<Structure>();
        private readonly NavGrid _grid;
        private readonly PlacementRules _rules;
        private int _nextId = 1;

        public Structures(NavGrid grid, PlacementRules rules)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (ReferenceEquals(rules, null))
                throw new ArgumentNullException(nameof(rules));

            _grid = grid;
            _rules = rules;
        }

        /// <summary>Levé quand une construction est posée.</summary>
        public event Action<Structure> StructurePlaced;

        /// <summary>Levé quand une pose est refusée, avec sa fiche, sa place et la raison.</summary>
        public event Action<StructureDefinition, Placement, PlacementResult> PlacementRefused;

        public IReadOnlyList<Structure> All => _all;

        /// <summary>
        /// Pose la construction si les règles le permettent ; sinon, lève <see cref="PlacementRefused"/>. Seul l'ordre
        /// « poser » l'appelle, au début d'un pas ; au-dessus de la simulation, on vérifie avec les règles de pose.
        /// </summary>
        internal PlacementResult Place(StructureDefinition definition, Placement placement)
        {
            PlacementResult result = _rules.Check(definition, placement);
            if (result != PlacementResult.Ok)
            {
                PlacementRefused?.Invoke(definition, placement, result);
                return result;
            }

            var structure = new Structure(_nextId++, definition, placement);
            _all.Add(structure);
            _grid.SetOccupant(PlacementRules.Footprint(definition, placement), structure.Id);
            if (definition.HasDoor)
                _grid.AddDoorway(_rules.Doorway(definition, placement));
            StructurePlaced?.Invoke(structure);
            return result;
        }
    }
}
