using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Conduire (GDD, section 11) : aller à l'entrepôt dont une place de charrette appelle la charrette, l'atteler, la
    /// tirer jusque devant sa place, au pas d'une charrette sur le sol où il marche (CartPace), par le trajet le plus
    /// rapide pour elle, et l'y garer. Il la gare même le travail fini : c'est la fin de
    /// sa marche ; hors des heures de travail, il ne l'attelle pas.
    /// </summary>
    internal sealed class DriveGesture : Gesture
    {
        private readonly CartOffers _carts;
        private readonly CartPace _pace;
        private readonly NavGrid _grid;
        private Drive _drive;

        public DriveGesture(CartOffers carts, CartPace pace, NavGrid grid)
        {
            _carts = carts;
            _pace = pace;
            _grid = grid;
        }

        public override Mover Mover => IsPulling ? Mover.Cart : Mover.OnFoot;

        public override float SpeedAt(Vector2 position)
        {
            return IsPulling ? _pace.SpeedAt(_grid.CellOf(position)) : 1f;
        }

        private bool IsPulling => _drive != null && _drive.IsHitched && !_drive.IsOver;

        public override bool TryTake(int island, Vector2 position)
        {
            if (!_carts.TryTakeDrive(island, out _drive))
                return false;
            Goal = ChoreGoal.Of(_drive.Cart.Home);
            return true;
        }

        public override GestureStep Arrive(Vector2 position, bool workTime)
        {
            if (_drive.IsHitched)
            {
                _carts.Park(_drive);
                return GestureStep.Done;
            }
            if (!workTime)
                return GestureStep.Done;
            _carts.Hitch(_drive, position);
            Goal = ChoreGoal.At(_drive.Stand);
            return GestureStep.Walk;
        }

        public override void Walked(Vector2 position)
        {
            if (IsPulling)
                _carts.Pull(_drive, position);
        }

        public override void Stop()
        {
            if (_drive == null)
                return;
            _carts.Cancel(_drive);
            _drive = null;
        }
    }
}
