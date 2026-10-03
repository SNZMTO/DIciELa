using Diciela.Simulation.Economy;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Garnir (GDD, section 11 ; décision 202) : aller à l'entrepôt prendre ce que demande la consigne d'une charrette
    /// garée, et le déposer devant elle ; ou y prendre son surplus et le rapporter à l'entrepôt. Comme porter, il dépose
    /// ce qu'il porte même le travail fini : c'est la fin de sa marche ; hors des heures de travail, il ne prend rien.
    /// </summary>
    internal sealed class StockGesture : Gesture
    {
        private readonly StallOffers _stalls;
        private readonly Inventory _load;
        private StallDelivery _delivery;

        public StockGesture(StallOffers stalls, Inventory load)
        {
            _stalls = stalls;
            _load = load;
        }

        public override bool TryTake(int island, Vector2 position)
        {
            if (!_stalls.TryTakeStock(island, position, _load, out _delivery))
                return false;
            Goal = _delivery.ToCart ? ChoreGoal.Of(_delivery.Warehouse) : ChoreGoal.At(_delivery.CartCell);
            return true;
        }

        public override GestureStep Arrive(Vector2 position, bool workTime)
        {
            if (_delivery.IsPickedUp)
            {
                _stalls.DropOff(_delivery, _load);
                return GestureStep.Done;
            }
            if (!workTime)
                return GestureStep.Done;
            _stalls.PickUp(_delivery, _load);
            Goal = _delivery.ToCart ? ChoreGoal.At(_delivery.CartCell) : ChoreGoal.Of(_delivery.Warehouse);
            return GestureStep.Walk;
        }

        public override void Stop()
        {
            if (_delivery == null)
                return;
            _stalls.Cancel(_delivery, _load);
            _delivery = null;
        }
    }
}
