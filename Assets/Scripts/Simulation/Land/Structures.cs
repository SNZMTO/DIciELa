using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les constructions posées sur la carte (ARCHITECTURE, section 4) : la vérité, dont le maillage et les devants de
    /// porte sont déduits et tenus à jour ici. Une pose passe par les règles de pose, qu'elles créent et qui les lisent ;
    /// refusée, elle ne change rien et dit pourquoi. Une construction posée en fantôme devient bâtie quand son chantier
    /// est fini. Les événements d'un pas attendent la fin de leur couche, dans une seule file, dans l'ordre où ils sont
    /// arrivés (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class Structures
    {
        private readonly List<Structure> _all = new List<Structure>();
        private readonly List<Happening> _happened = new List<Happening>();
        private readonly NavGrid _grid;
        private readonly PlacementRules _rules;
        private int _nextId = 1;

        public Structures(NavGrid grid, Relief relief, SimulationSettings settings, Knowledge knowledge)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
            Doorways = new Doorways(grid);
            _rules = new PlacementRules(grid, relief, settings, this, knowledge);
        }

        /// <summary>Levé quand une construction est posée.</summary>
        public event Action<Structure> StructurePlaced;

        /// <summary>Levé quand une pose est refusée, avec sa fiche, sa place et la raison.</summary>
        public event Action<StructureDefinition, Placement, PlacementResult> PlacementRefused;

        /// <summary>Levé quand le chantier d'une construction est fini : elle est bâtie, elle sert.</summary>
        public event Action<Structure> StructureBuilt;

        public IReadOnlyList<Structure> All => _all;

        /// <summary>Les devants de porte des constructions posées, où rien ne se pose (sauf un sol).</summary>
        public Doorways Doorways { get; }

        /// <summary>Les règles de pose, que le monde montre au-dessus de lui (World.PlacementRules).</summary>
        internal PlacementRules Rules => _rules;

        /// <summary>
        /// Le logis de la Chimère : la construction bâtie que sa fiche dit être son logis (le campement, puis sa maison) ;
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

        /// <summary>La construction qui porte ce numéro ; null s'il n'y en a pas.</summary>
        public Structure Find(int id)
        {
            foreach (Structure structure in _all)
            {
                if (structure.Id == id)
                    return structure;
            }
            return null;
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
        /// Pose la construction si les règles le permettent et la rend ; sinon, la refuse, rend null, et
        /// <see cref="PlacementRefused"/> le dira avec les événements du pas. Seul le monde l'appelle, au début d'un pas ;
        /// au-dessus de la simulation, on vérifie avec les règles de pose.
        /// </summary>
        internal Structure Place(StructureDefinition definition, Placement placement)
        {
            PlacementResult result = _rules.Check(definition, placement);
            if (result != PlacementResult.Ok)
            {
                _happened.Add(Happening.Refused(definition, placement, result));
                return null;
            }

            var structure = new Structure(_nextId++, definition, placement);
            _all.Add(structure);
            _grid.SetOccupant(PlacementRules.Footprint(definition, placement), structure.Id, definition.IsWalkable);
            if (definition.HasDoor)
                Doorways.Add(_rules.Doorway(definition, placement));
            _happened.Add(Happening.Of(HappeningKind.Placed, structure));
            return structure;
        }

        /// <summary>
        /// Le chantier de cette construction est fini : elle devient bâtie. Seule l'économie l'appelle, quand le dernier
        /// travail est fait.
        /// </summary>
        internal void Finish(Structure structure)
        {
            if (ReferenceEquals(structure, null))
                throw new ArgumentNullException(nameof(structure));
            if (structure.State != StructureState.Planned)
                throw new InvalidOperationException("La construction n° " + structure.Id + " n'attend pas d'être bâtie.");

            structure.BecomeBuilt();
            _happened.Add(Happening.Of(HappeningKind.Built, structure));
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide la file. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _happened.Count; i++)
                {
                    Happening happening = _happened[i];
                    switch (happening.Kind)
                    {
                        case HappeningKind.Placed:
                            StructurePlaced?.Invoke(happening.Structure);
                            break;
                        case HappeningKind.Built:
                            StructureBuilt?.Invoke(happening.Structure);
                            break;
                        default:
                            PlacementRefused?.Invoke(happening.Definition, happening.Placement, happening.Result);
                            break;
                    }
                }
            }
            finally
            {
                _happened.Clear();
            }
        }

        private enum HappeningKind
        {
            Placed,
            Refused,
            Built
        }

        /// <summary>Ce qui est arrivé à une construction, en attendant d'être annoncé : posée, refusée ou bâtie.</summary>
        private readonly struct Happening
        {
            private Happening(HappeningKind kind, Structure structure, StructureDefinition definition,
                Placement placement, PlacementResult result)
            {
                Kind = kind;
                Structure = structure;
                Definition = definition;
                Placement = placement;
                Result = result;
            }

            public HappeningKind Kind { get; }
            public Structure Structure { get; }
            public StructureDefinition Definition { get; }
            public Placement Placement { get; }
            public PlacementResult Result { get; }

            public static Happening Of(HappeningKind kind, Structure structure)
            {
                return new Happening(kind, structure, structure.Definition, structure.Placement, PlacementResult.Ok);
            }

            public static Happening Refused(StructureDefinition definition, Placement placement, PlacementResult result)
            {
                return new Happening(HappeningKind.Refused, null, definition, placement, result);
            }
        }
    }
}
