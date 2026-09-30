using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Les tâches à prendre (ARCHITECTURE, section 7) : porter des matériaux d'un entrepôt à un chantier, bâtir un
    /// chantier livré. Prendre est un appel unique qui réserve ce que la tâche promet : deux habitants ne prennent jamais
    /// la même. On n'offre que ce que l'habitant peut rejoindre à pied (les abords sur son îlot). Les chantiers se
    /// servent dans l'ordre de la file ; un chantier qui attend ses matériaux laisse passer les suivants (GDD, section 9).
    /// </summary>
    public sealed class TaskOffers
    {
        private readonly Warehouses _warehouses;
        private readonly ConstructionSites _sites;
        private readonly Reach _reach;
        private readonly int _maxBuilders;

        public TaskOffers(Warehouses warehouses, ConstructionSites sites, Islands islands, Surroundings surroundings,
            SimulationSettings settings)
        {
            if (ReferenceEquals(warehouses, null))
                throw new ArgumentNullException(nameof(warehouses));
            if (ReferenceEquals(sites, null))
                throw new ArgumentNullException(nameof(sites));
            if (ReferenceEquals(islands, null))
                throw new ArgumentNullException(nameof(islands));
            if (ReferenceEquals(surroundings, null))
                throw new ArgumentNullException(nameof(surroundings));
            if (ReferenceEquals(settings, null))
                throw new ArgumentNullException(nameof(settings));

            _warehouses = warehouses;
            _sites = sites;
            _reach = new Reach(islands, surroundings);
            _maxBuilders = settings.MaxBuildersPerSite;
        }

        /// <summary>
        /// Prend une livraison pour un porteur sur cet îlot, près de ce point, qui porte au plus tant : le premier
        /// chantier de la file à qui il manque une marchandise, depuis l'entrepôt le plus proche qui en a. Une seule
        /// marchandise par voyage. Faux s'il n'y a rien à porter qu'il puisse rejoindre.
        /// </summary>
        internal bool TryTakeDelivery(int island, Vector2 from, int capacity, out Delivery delivery)
        {
            delivery = null;
            IReadOnlyList<ConstructionSite> queue = _sites.Queue;
            for (int i = 0; i < queue.Count; i++)
            {
                ConstructionSite site = queue[i];
                bool reachable = false;
                bool checkedReach = false;
                for (int line = 0; line < site.LineCount; line++)
                {
                    int missing = site.MissingAt(line);
                    if (missing < 1)
                        continue;
                    if (!checkedReach)
                    {
                        reachable = _reach.IsReachable(site.Structure, island);
                        checkedReach = true;
                    }
                    if (!reachable)
                        break;
                    GoodDefinition good = site.GoodAt(line);
                    Structure source = NearestSource(good, island, from);
                    if (source == null)
                        continue;
                    int amount = Math.Min(Math.Min(capacity, missing), _warehouses.StockOf(source).AvailableOf(good));
                    _warehouses.Reserve(source, good, amount);
                    site.Promise(good, amount);
                    delivery = new Delivery(source, site, good, amount);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Prend une place d'ouvrier sur le premier chantier de la file dont les matériaux sont livrés, qui n'a pas son
        /// plein d'ouvriers et que l'habitant peut rejoindre. Faux s'il n'y a rien à bâtir.
        /// </summary>
        internal bool TryTakeBuilding(int island, out BuildShift shift)
        {
            shift = null;
            IReadOnlyList<ConstructionSite> queue = _sites.Queue;
            for (int i = 0; i < queue.Count; i++)
            {
                ConstructionSite site = queue[i];
                if (!site.IsSupplied || site.Builders >= _maxBuilders || !_reach.IsReachable(site.Structure, island))
                    continue;
                site.AddBuilder();
                shift = new BuildShift(site);
                return true;
            }
            return false;
        }

        /// <summary>Le porteur est à l'entrepôt : il prend les marchandises promises dans sa charge.</summary>
        internal void PickUp(Delivery delivery, Inventory load)
        {
            if (delivery.IsOver || delivery.IsPickedUp)
                throw new InvalidOperationException("Cette livraison n'a plus rien à prendre.");
            _warehouses.Take(delivery.Source, delivery.Good, delivery.Amount, load);
            delivery.MarkPickedUp();
        }

        /// <summary>Le porteur est au chantier : il y dépose sa charge.</summary>
        internal void DropOff(Delivery delivery, Inventory load)
        {
            if (delivery.IsOver || !delivery.IsPickedUp)
                throw new InvalidOperationException("Cette livraison n'a rien à déposer.");
            load.Remove(delivery.Good, delivery.Amount, false);
            _sites.Receive(delivery.Site, delivery.Good, delivery.Amount);
            delivery.MarkOver();
        }

        /// <summary>
        /// La livraison ne se fera pas : ses promesses sont rendues, et ce que le porteur avait déjà pris retourne à la
        /// ville.
        /// </summary>
        internal void Cancel(Delivery delivery, Inventory load)
        {
            if (delivery.IsOver)
                return;
            delivery.Site.Unpromise(delivery.Good, delivery.Amount);
            if (delivery.IsPickedUp)
                _warehouses.ReturnToTown(load);
            else
                _warehouses.Release(delivery.Source, delivery.Good, delivery.Amount);
            delivery.MarkOver();
        }

        /// <summary>L'ouvrier apporte ce travail, en heures d'un débutant ; vrai si le chantier est fini.</summary>
        internal bool Work(BuildShift shift, double hours)
        {
            return _sites.Work(shift.Site, hours);
        }

        /// <summary>L'ouvrier s'en va : sa place est rendue.</summary>
        internal void Leave(BuildShift shift)
        {
            if (shift.IsOver)
                return;
            shift.Site.RemoveBuilder();
            shift.MarkOver();
        }

        /// <summary>L'entrepôt le plus proche de ce point, à vol d'oiseau, qui a cette marchandise à donner.</summary>
        private Structure NearestSource(GoodDefinition good, int island, Vector2 from)
        {
            Structure nearest = null;
            float best = float.MaxValue;
            for (int i = 0; i < _warehouses.Count; i++)
            {
                if (_warehouses.StockAt(i).AvailableOf(good) < 1)
                    continue;
                Structure warehouse = _warehouses.StructureAt(i);
                float distance = Vector2.Distance(warehouse.Placement.Center, from);
                if (distance < best && _reach.IsReachable(warehouse, island))
                {
                    best = distance;
                    nearest = warehouse;
                }
            }
            return nearest;
        }

    }
}
