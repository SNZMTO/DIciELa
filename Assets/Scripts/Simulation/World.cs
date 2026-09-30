using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;

namespace Diciela.Simulation
{
    /// <summary>
    /// Le monde : il assemble les systèmes et les appelle dans l'ordre du pas, rien d'autre (ARCHITECTURE, section 3).
    /// Il reçoit les réglages, la carte et le catalogue, d'où il tire le déroulé du départ et les statistiques.
    /// </summary>
    public sealed class World
    {
        private readonly Queue<Command> _pendingCommands = new Queue<Command>();

        public World(SimulationSettings settings, MapDescription map, Catalog catalog)
        {
            if (ReferenceEquals(settings, null))
                throw new ArgumentNullException(nameof(settings));
            if (ReferenceEquals(map, null))
                throw new ArgumentNullException(nameof(map));
            if (ReferenceEquals(catalog, null))
                throw new ArgumentNullException(nameof(catalog));

            Clock = new Clock(settings);
            Map = map;
            Grid = new NavGrid(map.Relief, new GridMetrics(settings.CellsPerMeter));
            Nature = new Nature(Grid, map.Nature);
            Islands = new Islands(Grid);
            PathFinder = new PathFinder(Grid, Islands);
            Structures = new Structures(Grid, map.Relief, settings);
            var surroundings = new Surroundings(Grid);
            Ledger = new GoodsLedger();
            Warehouses = new Warehouses(Ledger);
            ConstructionSites = new ConstructionSites(Structures, Warehouses, Ledger);
            var offers = new TaskOffers(Warehouses, ConstructionSites, Islands, surroundings, settings);
            Clearings = new ClearingOffers(Nature, Warehouses, Islands, surroundings);
            WorkSlots = new WorkSlots();
            Villagers = new Villagers(Grid, Islands, PathFinder, Clock, surroundings, offers, Clearings, WorkSlots,
                new StatisticTable(catalog.Statistics, settings));
            Placing = new Placing(this, new Opening(catalog.Opening));
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
        public PlacementRules PlacementRules => Structures.Rules;

        public Structures Structures { get; }

        /// <summary>Les arbres et les rochers de la carte : ceux qui restent, ceux que le joueur a marqués.</summary>
        public Nature Nature { get; }

        /// <summary>Les arbres et les rochers à retirer, comme tâches à prendre, et le travail déjà fait sur eux.</summary>
        public ClearingOffers Clearings { get; }

        /// <summary>Le bilan des marchandises : ce qui est né et ce qui a disparu, par cause.</summary>
        public GoodsLedger Ledger { get; }

        /// <summary>Ce qui tient les marchandises de la ville : le tas de départ, puis les entrepôts.</summary>
        public Warehouses Warehouses { get; }

        /// <summary>Les chantiers et leur file.</summary>
        public ConstructionSites ConstructionSites { get; }

        /// <summary>La table des postes : les postes ouverts et qui les tient.</summary>
        public WorkSlots WorkSlots { get; }

        /// <summary>Les habitants : la Chimère, une fois créée par son ordre.</summary>
        public Villagers Villagers { get; }

        /// <summary>La pose d'une construction, qui touche plusieurs couches ; seuls les ordres s'en servent.</summary>
        internal Placing Placing { get; }

        /// <summary>Envoie un ordre ; il s'applique au début du pas suivant. La seule porte du joueur.</summary>
        public void Submit(Command command)
        {
            if (ReferenceEquals(command, null))
                throw new ArgumentNullException(nameof(command));
            _pendingCommands.Enqueue(command);
        }

        /// <summary>
        /// Joue un pas. L'ordre du pas, écrit ici et nulle part ailleurs : les ordres en attente, puis le temps, le
        /// terrain, l'économie, la société, le monde. Pour l'instant, les ordres, le temps et les habitants, qui font le
        /// travail de l'économie et du terrain ; les événements partent à la fin de chaque couche.
        /// </summary>
        public void Step()
        {
            ApplyPendingCommands();
            Clock.Advance();
            Villagers.Step();
            DeliverEvents();
        }

        /// <summary>
        /// Les ordres envoyés pendant l'application attendent le pas suivant : la boucle est bornée. Leurs événements
        /// partent ensuite, tous les ordres joués (ARCHITECTURE, section 5).
        /// </summary>
        private void ApplyPendingCommands()
        {
            int count = _pendingCommands.Count;
            for (int i = 0; i < count; i++)
                _pendingCommands.Dequeue().Execute(this);
            DeliverEvents();
        }

        /// <summary>Les événements des couches, du bas vers le haut.</summary>
        private void DeliverEvents()
        {
            Structures.DeliverEvents();
            Nature.DeliverEvents();
            Warehouses.DeliverEvents();
            ConstructionSites.DeliverEvents();
            WorkSlots.DeliverEvents();
            Villagers.DeliverEvents();
        }
    }
}
