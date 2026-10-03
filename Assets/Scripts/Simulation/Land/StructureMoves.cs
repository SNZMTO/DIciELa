using System;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Déplacer une construction (GDD, section 9 ; décision 199), une opération à part entière, jamais « retirer puis
    /// reposer » : les règles de pose disent si la place convient, la construction déplacée ne comptant pas ; acceptée,
    /// la construction rend son sol, change de place et prend le nouveau ; refusée, rien ne change et les constructions
    /// disent pourquoi. Seul le monde l'appelle, au début d'un pas.
    /// </summary>
    internal sealed class StructureMoves
    {
        private readonly Structures _structures;
        private readonly StructureGround _ground;
        private readonly PlacementRules _rules;

        public StructureMoves(Structures structures, StructureGround ground, PlacementRules rules)
        {
            _structures = structures ?? throw new ArgumentNullException(nameof(structures));
            _ground = ground ?? throw new ArgumentNullException(nameof(ground));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        /// <summary>
        /// Déplace la construction à cette place, tout de suite, si les règles le permettent ; sinon, rend faux, et les
        /// constructions diront pourquoi avec les événements du pas.
        /// </summary>
        public bool Move(Structure structure, Placement placement)
        {
            if (ReferenceEquals(structure, null))
                throw new ArgumentNullException(nameof(structure));

            PlacementResult result = _rules.CheckMove(structure, placement);
            if (result != PlacementResult.Ok)
            {
                _structures.Record(StructureHappening.MoveRefusedFor(structure, placement, result));
                return false;
            }
            _ground.Release(structure, structure.Placement);
            structure.MoveTo(placement);
            _ground.Take(structure, placement);
            _structures.Record(StructureHappening.Of(StructureHappening.Kinds.Moved, structure));
            return true;
        }
    }
}
