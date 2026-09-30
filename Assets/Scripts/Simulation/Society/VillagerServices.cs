using System;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Ce que chaque habitant consulte de la ville pour vivre et travailler : le temps, le maillage, ses îlots et ses
    /// trajets, les abords des constructions, les tâches à prendre (dont les arbres à retirer), la table des postes et les
    /// statistiques. Un seul
    /// paquet, fait par les habitants, donné à chacun.
    /// </summary>
    internal sealed class VillagerServices
    {
        public VillagerServices(Clock clock, NavGrid grid, Islands islands, PathFinder finder, Surroundings surroundings,
            TaskOffers offers, ClearingOffers clearings, WorkSlots slots, StatisticTable statistics)
        {
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Islands = islands ?? throw new ArgumentNullException(nameof(islands));
            Finder = finder ?? throw new ArgumentNullException(nameof(finder));
            Surroundings = surroundings ?? throw new ArgumentNullException(nameof(surroundings));
            Offers = offers ?? throw new ArgumentNullException(nameof(offers));
            Clearings = clearings ?? throw new ArgumentNullException(nameof(clearings));
            Slots = slots ?? throw new ArgumentNullException(nameof(slots));
            Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        }

        public Clock Clock { get; }
        public NavGrid Grid { get; }
        public Islands Islands { get; }
        public PathFinder Finder { get; }
        public Surroundings Surroundings { get; }
        public TaskOffers Offers { get; }
        public ClearingOffers Clearings { get; }
        public WorkSlots Slots { get; }
        public StatisticTable Statistics { get; }
    }
}
