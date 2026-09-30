using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Défricher (GDD, section 2) : aller à l'arbre ou au rocher marqué, y travailler jusqu'à l'abattre, puis porter ce
    /// qu'il rend à l'entrepôt où sa place est retenue. Démarqué avant d'être abattu, il le laisse.
    /// </summary>
    internal sealed class ClearGesture : Gesture
    {
        private readonly ClearingOffers _clearings;
        private readonly Inventory _load;
        private Clearing _clearing;

        public ClearGesture(ClearingOffers clearings, Inventory load)
        {
            _clearings = clearings;
            _load = load;
        }

        public override bool TryTake(int island, Vector2 position)
        {
            if (!_clearings.TryTakeClearing(island, _load.FreeSpace, out _clearing))
                return false;
            Goal = ChoreGoal.Of(_clearing.Item);
            return true;
        }

        public override GestureStep Arrive(bool workTime)
        {
            if (_clearing.IsFelled)
            {
                _clearings.Store(_clearing, _load);
                return GestureStep.Done;
            }
            return workTime && _clearing.Item.IsMarked ? GestureStep.Work : GestureStep.Done;
        }

        public override GestureStep Work(double hours, out Structure built)
        {
            built = null;
            if (!_clearing.Item.IsMarked)
                return GestureStep.Done;
            if (!_clearings.Work(_clearing, hours, _load))
                return GestureStep.Work;
            if (_clearing.Warehouse == null)
            {
                _clearings.Store(_clearing, _load);
                return GestureStep.Done;
            }
            Goal = ChoreGoal.Of(_clearing.Warehouse);
            return GestureStep.Walk;
        }

        public override void Stop()
        {
            if (_clearing == null)
                return;
            _clearings.Cancel(_clearing, _load);
            _clearing = null;
        }
    }
}
