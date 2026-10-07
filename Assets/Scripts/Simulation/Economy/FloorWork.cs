using System;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Tracer un sol, ou effacer un sol bâti (GDD, section 9 ; décision 209) : un travail qui suit sa surface, que la
    /// gomme peut rétrécir, sans matériaux aujourd'hui (la terre est gratuite). Les ouvriers le bâtissent d'un coup,
    /// quand son travail est fait : tracé, il sert ; effacé, il n'y a plus de sol.
    /// </summary>
    public sealed class FloorWork : SiteWork
    {
        private readonly FloorPatch _patch;
        private readonly Floors _floors;
        private readonly double _hoursPerSquareMeter;

        internal FloorWork(FloorPatch patch, Floors floors, double hoursPerSquareMeter)
        {
            _patch = patch ?? throw new ArgumentNullException(nameof(patch));
            _floors = floors ?? throw new ArgumentNullException(nameof(floors));
            _hoursPerSquareMeter = hoursPerSquareMeter;
        }

        public override FloorPatch Floor => _patch;
        public override double WorkNeeded => _patch.AreaSquareMeters * _hoursPerSquareMeter;

        internal override void Finish()
        {
            _floors.Finish(_patch);
        }
    }
}
