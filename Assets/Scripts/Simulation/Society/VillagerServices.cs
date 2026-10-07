using System;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Ce que chaque habitant consulte de la ville pour vivre et travailler : le temps, le maillage, ses îlots et ses
    /// trajets, le pas des charrettes sur les sols, les abords des constructions, les tâches à prendre (dont les arbres
    /// à retirer, les charrettes à conduire et à garnir), les statistiques et les réglages de la simulation. Un seul
    /// paquet, fait par les habitants, donné à chacun.
    /// </summary>
    internal sealed class VillagerServices
    {
        public VillagerServices(Clock clock, NavGrid grid, Islands islands, PathFinder finder, CartPace pace,
            Surroundings surroundings,
            TaskOffers offers, ClearingOffers clearings, CartOffers carts, StallOffers stalls,
            StatisticTable statistics, SimulationSettings settings)
        {
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Islands = islands ?? throw new ArgumentNullException(nameof(islands));
            Finder = finder ?? throw new ArgumentNullException(nameof(finder));
            Pace = pace ?? throw new ArgumentNullException(nameof(pace));
            Surroundings = surroundings ?? throw new ArgumentNullException(nameof(surroundings));
            Offers = offers ?? throw new ArgumentNullException(nameof(offers));
            Clearings = clearings ?? throw new ArgumentNullException(nameof(clearings));
            Carts = carts ?? throw new ArgumentNullException(nameof(carts));
            Stalls = stalls ?? throw new ArgumentNullException(nameof(stalls));
            Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
            Settings = ReferenceEquals(settings, null) ? throw new ArgumentNullException(nameof(settings)) : settings;
        }

        public Clock Clock { get; }
        public NavGrid Grid { get; }
        public Islands Islands { get; }
        public PathFinder Finder { get; }
        public CartPace Pace { get; }
        public Surroundings Surroundings { get; }
        public TaskOffers Offers { get; }
        public ClearingOffers Clearings { get; }
        public CartOffers Carts { get; }
        public StallOffers Stalls { get; }
        public StatisticTable Statistics { get; }
        public SimulationSettings Settings { get; }
    }
}
