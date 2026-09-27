using System;
using Diciela.Data;
using Diciela.Simulation.Core;

namespace Diciela.Simulation
{
    /// <summary>
    /// Le monde : il assemble les systèmes et les appelle dans l'ordre du pas, rien d'autre (ARCHITECTURE, section 3).
    /// </summary>
    public sealed class World
    {
        public World(SimulationSettings settings)
        {
            if (ReferenceEquals(settings, null))
                throw new ArgumentNullException(nameof(settings));

            Clock = new Clock(settings);
        }

        public Clock Clock { get; }

        /// <summary>
        /// Joue un pas. L'ordre du pas, écrit ici et nulle part ailleurs : les ordres en attente, puis le temps,
        /// le terrain, l'économie, la société, le monde. Seul le temps existe pour l'instant.
        /// </summary>
        public void Step()
        {
            Clock.Advance();
        }
    }
}
