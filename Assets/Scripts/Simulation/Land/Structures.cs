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
    /// est fini ; elle se déplace par une opération à part entière (StructureMoves). Les événements d'un pas attendent la
    /// fin de leur couche, dans une seule file, dans l'ordre où ils sont arrivés (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class Structures
    {
        private readonly List<Structure> _all = new List<Structure>();
        private readonly List<StructureHappening> _happened = new List<StructureHappening>();
        private readonly PlacementRules _rules;
        private readonly StructureGround _ground;
        private int _nextId = 1;

        public Structures(NavGrid grid, Relief relief, SimulationSettings settings, Knowledge knowledge)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            Doorways = new Doorways(grid);
            _rules = new PlacementRules(grid, relief, settings, this, knowledge);
            _ground = new StructureGround(grid, Doorways, _rules);
            Moves = new StructureMoves(this, _ground, _rules);
        }

        /// <summary>Levé quand une construction est posée.</summary>
        public event Action<Structure> StructurePlaced;

        /// <summary>Levé quand une pose est refusée, avec sa fiche, sa place et la raison.</summary>
        public event Action<StructureDefinition, Placement, PlacementResult> PlacementRefused;

        /// <summary>Levé quand le chantier d'une construction est fini : elle est bâtie, elle sert.</summary>
        public event Action<Structure> StructureBuilt;

        /// <summary>Levé quand une construction change de place.</summary>
        public event Action<Structure> StructureMoved;

        /// <summary>Levé quand un déplacement est refusé, avec la construction, la place demandée et la raison.</summary>
        public event Action<Structure, Placement, PlacementResult> MoveRefused;

        public IReadOnlyList<Structure> All => _all;

        /// <summary>Les devants de porte des constructions posées, où rien ne se pose (sauf un sol).</summary>
        public Doorways Doorways { get; }

        /// <summary>Les règles de pose, que le monde montre au-dessus de lui (World.PlacementRules).</summary>
        internal PlacementRules Rules => _rules;

        /// <summary>Le déplacement des constructions : seuls les ordres s'en servent.</summary>
        internal StructureMoves Moves { get; }

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
                _happened.Add(StructureHappening.Refused(definition, placement, result));
                return null;
            }

            var structure = new Structure(_nextId++, definition, placement);
            _all.Add(structure);
            _ground.Take(structure, placement);
            _happened.Add(StructureHappening.Of(StructureHappening.Kinds.Placed, structure));
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
            _happened.Add(StructureHappening.Of(StructureHappening.Kinds.Built, structure));
        }

        /// <summary>Met dans la file ce qui est arrivé à une construction : le déplacement l'y met aussi.</summary>
        internal void Record(StructureHappening happening)
        {
            _happened.Add(happening);
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide la file. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _happened.Count; i++)
                    Deliver(_happened[i]);
            }
            finally
            {
                _happened.Clear();
            }
        }

        private void Deliver(StructureHappening happening)
        {
            switch (happening.Kind)
            {
                case StructureHappening.Kinds.Placed:
                    StructurePlaced?.Invoke(happening.Structure);
                    break;
                case StructureHappening.Kinds.Built:
                    StructureBuilt?.Invoke(happening.Structure);
                    break;
                case StructureHappening.Kinds.Moved:
                    StructureMoved?.Invoke(happening.Structure);
                    break;
                case StructureHappening.Kinds.MoveRefused:
                    MoveRefused?.Invoke(happening.Structure, happening.Placement, happening.Result);
                    break;
                default:
                    PlacementRefused?.Invoke(happening.Definition, happening.Placement, happening.Result);
                    break;
            }
        }
    }
}
