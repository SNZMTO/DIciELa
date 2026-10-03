using Diciela.Simulation.Economy;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Porter (GDD, section 9) : aller à l'entrepôt, prendre ce qui est promis, aller au chantier, déposer. Il dépose ce
    /// qu'il porte même le travail fini : c'est la fin de sa marche.
    /// </summary>
    internal sealed class CarryGesture : Gesture
    {
        private readonly TaskOffers _offers;
        private readonly Inventory _load;
        private Delivery _delivery;

        public CarryGesture(TaskOffers offers, Inventory load)
        {
            _offers = offers;
            _load = load;
        }

        public override bool TryTake(int island, Vector2 position)
        {
            if (!_offers.TryTakeDelivery(island, position, _load, out _delivery))
                return false;
            Goal = ChoreGoal.Of(_delivery.Source);
            return true;
        }

        public override GestureStep Arrive(Vector2 position, bool workTime)
        {
            if (_delivery.IsPickedUp)
            {
                _offers.DropOff(_delivery, _load);
                return GestureStep.Done;
            }
            if (!workTime)
                return GestureStep.Done;
            _offers.PickUp(_delivery, _load);
            Goal = ChoreGoal.Of(_delivery.Site.Structure);
            return GestureStep.Walk;
        }

        public override void Stop()
        {
            if (_delivery == null)
                return;
            _offers.Cancel(_delivery, _load);
            _delivery = null;
        }
    }
}
