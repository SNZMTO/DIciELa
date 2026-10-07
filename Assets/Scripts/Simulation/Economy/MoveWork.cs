using System;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Déplacer un bâtiment bâti (GDD, section 9 ; décision 199) : un petit chantier, sans matériaux. Le premier travail
    /// le défait, il ne sert plus ; défait, il saute à la place qu'il retenait ; remonté, il sert de nouveau, sans
    /// nouvelle mise en service.
    /// </summary>
    public sealed class MoveWork : SiteWork
    {
        private readonly Structure _structure;
        private readonly StructureMoves _moves;
        private readonly double _workNeeded;

        internal MoveWork(Structure structure, StructureMoves moves, double takeDownHours, double putBackHours)
        {
            _structure = structure ?? throw new ArgumentNullException(nameof(structure));
            _moves = moves ?? throw new ArgumentNullException(nameof(moves));
            TakeDownHours = takeDownHours;
            _workNeeded = takeDownHours + putBackHours;
        }

        public override Structure Structure => _structure;
        public override double WorkNeeded => _workNeeded;

        /// <summary>Les heures de travail qui défont le bâtiment ; les suivantes le remontent.</summary>
        public double TakeDownHours { get; }

        /// <summary>Vrai une fois le bâtiment défait, puis sauté à sa nouvelle place.</summary>
        public bool HasLanded { get; private set; }

        internal override void Progress(double workDone)
        {
            if (_structure.State == StructureState.Built)
                _moves.TakeDown(_structure);
            if (HasLanded || workDone < TakeDownHours)
                return;
            _moves.Land(_structure);
            HasLanded = true;
        }

        internal override void Finish()
        {
            _moves.Finish(_structure);
        }
    }
}
