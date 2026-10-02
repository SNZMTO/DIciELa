using Diciela.Simulation.Economy;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>Bâtir (GDD, section 9) : aller au chantier livré, y travailler jusqu'à ce qu'il soit fini.</summary>
    internal sealed class BuildGesture : Gesture
    {
        private readonly TaskOffers _offers;
        private BuildShift _shift;

        public BuildGesture(TaskOffers offers)
        {
            _offers = offers;
        }

        public override bool TryTake(int island, Vector2 position)
        {
            if (!_offers.TryTakeBuilding(island, out _shift))
                return false;
            Goal = ChoreGoal.Of(_shift.Site.Structure);
            return true;
        }

        public override GestureStep Arrive(bool workTime)
        {
            return workTime ? GestureStep.Work : GestureStep.Done;
        }

        public override GestureStep Work(double hours)
        {
            if (!_offers.Work(_shift, hours))
                return GestureStep.Work;
            return GestureStep.Done;
        }

        public override void Stop()
        {
            if (_shift == null)
                return;
            _offers.Leave(_shift);
            _shift = null;
        }
    }
}
