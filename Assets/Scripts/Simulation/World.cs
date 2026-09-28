using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// Le monde : il assemble les systèmes et les appelle dans l'ordre du pas, rien d'autre (ARCHITECTURE, section 3).
    /// </summary>
    public sealed class World
    {
        private readonly Queue<Command> _pendingCommands = new Queue<Command>();

        public World(SimulationSettings settings, MapDescription map)
        {
            if (ReferenceEquals(settings, null))
                throw new ArgumentNullException(nameof(settings));
            if (ReferenceEquals(map, null))
                throw new ArgumentNullException(nameof(map));

            Clock = new Clock(settings);
            Map = map;
            Grid = new NavGrid(map.Relief, new GridMetrics(settings.CellsPerMeter));
            Islands = new Islands(Grid);
            PathFinder = new PathFinder(Grid, Islands);
            PlacementRules = new PlacementRules(Grid, map.Relief, settings);
            Structures = new Structures(Grid, PlacementRules);
        }

        public Clock Clock { get; }

        /// <summary>La carte reçue au lancement : elle ne change pas pendant la partie.</summary>
        public MapDescription Map { get; }

        /// <summary>Le maillage : ce qui occupe le sol, déduit des constructions.</summary>
        public NavGrid Grid { get; }

        /// <summary>Les îlots du maillage : qui peut rejoindre qui à pied, su sans rien fouiller.</summary>
        public Islands Islands { get; }

        /// <summary>La recherche des trajets sur le maillage : une seule, dont les tableaux servent à chaque recherche.</summary>
        public PathFinder PathFinder { get; }

        /// <summary>Les règles de pose : la seule réponse à « peut-on poser ici ? », pour l'aperçu et l'ordre.</summary>
        public PlacementRules PlacementRules { get; }

        public Structures Structures { get; }

        /// <summary>Envoie un ordre ; il s'applique au début du pas suivant. La seule porte du joueur.</summary>
        public void Submit(Command command)
        {
            if (ReferenceEquals(command, null))
                throw new ArgumentNullException(nameof(command));
            _pendingCommands.Enqueue(command);
        }

        /// <summary>
        /// Joue un pas. L'ordre du pas, écrit ici et nulle part ailleurs : les ordres en attente, puis le temps,
        /// le terrain, l'économie, la société, le monde. Seuls les ordres et le temps existent pour l'instant.
        /// </summary>
        public void Step()
        {
            ApplyPendingCommands();
            Clock.Advance();
        }

        /// <summary>Les ordres envoyés pendant l'application attendent le pas suivant : la boucle est bornée.</summary>
        private void ApplyPendingCommands()
        {
            int count = _pendingCommands.Count;
            for (int i = 0; i < count; i++)
                _pendingCommands.Dequeue().Execute(this);
        }
    }
}
