using System;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Déplacer une construction (GDD, section 9 ; décisions 199 et 200), une opération à part entière, jamais « retirer
    /// puis reposer » : les règles de pose disent si la place convient, la construction déplacée ne comptant pas ; refusée,
    /// rien ne change et les constructions disent pourquoi. Un chantier saute tout de suite : il rend son sol et prend le
    /// nouveau. Un bâtiment bâti retient sa nouvelle place dès l'ordre, et sert encore ; un ouvrier le défait (il ne sert
    /// plus), il saute à sa nouvelle place, qu'il tenait déjà, puis on le remonte. Le monde ordonne, l'économie fait
    /// avancer le petit chantier.
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
        /// Déplace la construction à cette place si les règles le permettent : un chantier tout de suite, un bâtiment en
        /// retenant sa nouvelle place. Sinon rend faux, et les constructions diront pourquoi avec les événements du pas.
        /// Une construction qu'on déplace déjà attend d'être remontée. Seul le monde l'appelle, au début d'un pas.
        /// </summary>
        public bool Move(Structure structure, Placement placement)
        {
            if (ReferenceEquals(structure, null))
                throw new ArgumentNullException(nameof(structure));

            PlacementResult result = structure.MovingTo.HasValue
                ? PlacementResult.Moving
                : _rules.CheckMove(structure, placement);
            if (result != PlacementResult.Ok)
            {
                _structures.Record(StructureHappening.MoveRefusedFor(structure, placement, result));
                return false;
            }
            if (structure.State == StructureState.Planned)
            {
                _ground.Release(structure, structure.Placement);
                structure.MoveTo(placement);
                _ground.Take(structure, placement);
                _structures.Record(StructureHappening.Of(StructureHappening.Kinds.Moved, structure));
                return true;
            }
            // La nouvelle place est retenue tout de suite, devant de porte compris : rien ne s'y pose plus.
            structure.StartMove(placement);
            _ground.Take(structure, placement);
            _structures.Record(StructureHappening.Of(StructureHappening.Kinds.MoveStarted, structure));
            return true;
        }

        /// <summary>Un ouvrier commence à défaire le bâtiment qu'on déplace : il ne sert plus. Seule l'économie l'appelle.</summary>
        public void TakeDown(Structure structure)
        {
            if (structure.State != StructureState.Built || !structure.MovingTo.HasValue)
                throw new InvalidOperationException("La construction n° " + structure.Id + " n'attend pas d'être défaite.");
            structure.TakeDown();
            _structures.Record(StructureHappening.Of(StructureHappening.Kinds.TakenDown, structure));
        }

        /// <summary>
        /// Défait, le bâtiment saute à sa nouvelle place, qu'il tenait déjà : il rend l'ancienne, puis reprend les mailles
        /// que les deux partageaient. Seule l'économie l'appelle.
        /// </summary>
        public void Land(Structure structure)
        {
            if (structure.State != StructureState.Moving || !structure.MovingTo.HasValue)
                throw new InvalidOperationException("La construction n° " + structure.Id + " n'est pas défaite.");
            Placement target = structure.MovingTo.Value;
            _ground.Release(structure, structure.Placement);
            structure.MoveTo(target);
            _ground.Cover(structure, target);
            _structures.Record(StructureHappening.Of(StructureHappening.Kinds.Moved, structure));
        }

        /// <summary>Remonté à sa nouvelle place, le bâtiment sert de nouveau. Seule l'économie l'appelle.</summary>
        public void Finish(Structure structure)
        {
            if (structure.State != StructureState.Moving)
                throw new InvalidOperationException("La construction n° " + structure.Id + " n'est pas en déplacement.");
            structure.EndMove();
            _structures.Record(StructureHappening.Of(StructureHappening.Kinds.Built, structure));
        }
    }
}
