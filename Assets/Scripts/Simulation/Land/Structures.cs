using System;
using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les constructions posées sur la carte (ARCHITECTURE, section 4) : la vérité, dont le maillage est déduit et tenu
    /// à jour ici. Une pose passe par les règles de pose, qu'elles créent et qui les lisent ; refusée, elle ne change
    /// rien et dit pourquoi. Les événements d'un pas attendent la fin de leur couche, dans une seule file, dans l'ordre
    /// des poses (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class Structures
    {
        private readonly List<Structure> _all = new List<Structure>();
        private readonly List<Happening> _happened = new List<Happening>();
        private readonly NavGrid _grid;
        private readonly PlacementRules _rules;
        private int _nextId = 1;

        public Structures(NavGrid grid, Relief relief, SimulationSettings settings)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
            _rules = new PlacementRules(grid, relief, settings, this);
        }

        /// <summary>Levé quand une construction est posée.</summary>
        public event Action<Structure> StructurePlaced;

        /// <summary>Levé quand une pose est refusée, avec sa fiche, sa place et la raison.</summary>
        public event Action<StructureDefinition, Placement, PlacementResult> PlacementRefused;

        public IReadOnlyList<Structure> All => _all;

        /// <summary>Les règles de pose, que le monde montre au-dessus de lui (World.PlacementRules).</summary>
        internal PlacementRules Rules => _rules;

        /// <summary>
        /// Le logis de la Chimère : la construction bâtie que sa fiche dit être son logis (la tente, puis sa maison) ;
        /// null tant qu'elle n'en a pas (GDD, section 6).
        /// </summary>
        public Structure ChimeraHome
        {
            get
            {
                foreach (Structure structure in _all)
                {
                    if (structure.Definition.IsChimeraHome && structure.State == StructureState.Built)
                        return structure;
                }
                return null;
            }
        }

        /// <summary>Vrai si une construction de cette fiche est posée.</summary>
        public bool IsPlaced(StructureDefinition definition)
        {
            foreach (Structure structure in _all)
            {
                if (ReferenceEquals(structure.Definition, definition))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Pose la construction si les règles le permettent ; sinon, la refuse, et <see cref="PlacementRefused"/> le dira
        /// avec les événements du pas. Seul l'ordre « poser » l'appelle, au début d'un pas ; au-dessus de la simulation,
        /// on vérifie avec les règles de pose.
        /// </summary>
        internal PlacementResult Place(StructureDefinition definition, Placement placement)
        {
            PlacementResult result = _rules.Check(definition, placement);
            if (result != PlacementResult.Ok)
            {
                _happened.Add(new Happening(null, definition, placement, result));
                return result;
            }

            var structure = new Structure(_nextId++, definition, placement);
            _all.Add(structure);
            _grid.SetOccupant(PlacementRules.Footprint(definition, placement), structure.Id);
            if (definition.HasDoor)
                _grid.AddDoorway(_rules.Doorway(definition, placement));
            _happened.Add(new Happening(structure, definition, placement, result));
            return result;
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide la file. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _happened.Count; i++)
                {
                    Happening happening = _happened[i];
                    if (happening.Result == PlacementResult.Ok)
                        StructurePlaced?.Invoke(happening.Placed);
                    else
                        PlacementRefused?.Invoke(happening.Definition, happening.Placement, happening.Result);
                }
            }
            finally
            {
                _happened.Clear();
            }
        }

        /// <summary>Une pose jouée, en attendant d'être annoncée : la construction posée, ou la raison du refus.</summary>
        private readonly struct Happening
        {
            public Happening(Structure placed, StructureDefinition definition, Placement placement,
                PlacementResult result)
            {
                Placed = placed;
                Definition = definition;
                Placement = placement;
                Result = result;
            }

            public Structure Placed { get; }
            public StructureDefinition Definition { get; }
            public Placement Placement { get; }
            public PlacementResult Result { get; }
        }
    }
}
