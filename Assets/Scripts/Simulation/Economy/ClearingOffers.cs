using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Les arbres et les rochers à retirer, comme tâches à prendre (GDD, section 2 ; ARCHITECTURE, section 7) : le premier
    /// marqué, dans l'ordre où le joueur les a marqués, que personne n'a pris, que l'habitant peut rejoindre, et dont un
    /// entrepôt qu'il peut rejoindre a la place de ce qu'il rend. Le prendre le réserve et retient cette place. Le travail
    /// avance en heures d'un débutant et se garde si le défricheur s'en va ; fini, l'arbre quitte la carte et ce qu'il
    /// rend naît dans la charge du défricheur, qui le range à l'entrepôt.
    /// </summary>
    public sealed class ClearingOffers
    {
        private readonly Nature _nature;
        private readonly Warehouses _warehouses;
        private readonly Reach _reach;
        private readonly HashSet<NatureItem> _taken = new HashSet<NatureItem>();
        private readonly Dictionary<NatureItem, double> _progress = new Dictionary<NatureItem, double>();

        public ClearingOffers(Nature nature, Warehouses warehouses, Islands islands, Surroundings surroundings)
        {
            if (ReferenceEquals(nature, null))
                throw new ArgumentNullException(nameof(nature));
            if (ReferenceEquals(warehouses, null))
                throw new ArgumentNullException(nameof(warehouses));

            _nature = nature;
            _warehouses = warehouses;
            _reach = new Reach(islands, surroundings);
        }

        /// <summary>Les heures d'un débutant déjà faites sur cet arbre ou ce rocher ; 0 s'il n'est pas entamé.</summary>
        public double ProgressOf(NatureItem item)
        {
            return _progress.TryGetValue(item, out double hours) ? hours : 0d;
        }

        /// <summary>
        /// Prend un arbre ou un rocher pour un défricheur sur cet îlot, qui a encore cette place dans sa charge. Faux s'il
        /// n'y a rien à retirer qu'il puisse rejoindre, ou pas de place où ranger ce que cela rend.
        /// </summary>
        internal bool TryTakeClearing(int island, int freeSpace, out Clearing clearing)
        {
            clearing = null;
            IReadOnlyList<NatureItem> marked = _nature.Marked;
            for (int i = 0; i < marked.Count; i++)
            {
                NatureItem item = marked[i];
                GoodAmount yield = item.Definition.Yield;
                bool yields = !ReferenceEquals(yield.Good, null) && yield.Amount > 0;
                if (_taken.Contains(item) || (yields && yield.Amount > freeSpace) ||
                    !_reach.IsReachable(item.Footprint, item.Occupant, island))
                    continue;
                Structure warehouse = null;
                if (yields)
                {
                    warehouse = NearestWithRoom(yield.Amount, island, item.Placement.Center);
                    if (warehouse == null)
                        continue;
                    _warehouses.ReserveSpace(warehouse, yield.Amount);
                }
                _taken.Add(item);
                clearing = yields
                    ? new Clearing(item, warehouse, yield.Good, yield.Amount)
                    : new Clearing(item, null, null, 0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Le défricheur apporte ce travail, en heures d'un débutant ; vrai s'il a fini : l'arbre ou le rocher quitte la
        /// carte, et ce qu'il rend naît dans sa charge.
        /// </summary>
        internal bool Work(Clearing clearing, double hours, Inventory load)
        {
            if (clearing.IsOver || clearing.IsFelled)
                throw new InvalidOperationException("Cet arbre ou ce rocher n'attend plus de travail.");
            NatureItem item = clearing.Item;
            double done = ProgressOf(item) + hours;
            if (done < item.Definition.WorkHours)
            {
                _progress[item] = done;
                return false;
            }
            _progress.Remove(item);
            _taken.Remove(item);
            _nature.Remove(item);
            if (clearing.Amount > 0)
                _warehouses.Create(load, clearing.Good, clearing.Amount, GoodsCause.Clearing);
            clearing.MarkFelled();
            return true;
        }

        /// <summary>Le défricheur est à l'entrepôt : il y range ce qu'il porte, à la place retenue.</summary>
        internal void Store(Clearing clearing, Inventory load)
        {
            if (clearing.IsOver || !clearing.IsFelled)
                throw new InvalidOperationException("Ce défrichage n'a rien à ranger.");
            if (clearing.Amount > 0)
                _warehouses.Store(clearing.Warehouse, load, clearing.Good, clearing.Amount);
            clearing.MarkOver();
        }

        /// <summary>
        /// La tâche s'arrête là : l'arbre, pas encore abattu, redevient libre pour un autre, avec le travail déjà fait, et
        /// la place retenue est rendue ; abattu, ce que porte le défricheur va à la place qu'on lui avait retenue.
        /// </summary>
        internal void Cancel(Clearing clearing, Inventory load)
        {
            if (clearing.IsOver)
                return;
            clearing.MarkOver();
            if (clearing.IsFelled)
            {
                if (clearing.Amount > 0)
                    _warehouses.Store(clearing.Warehouse, load, clearing.Good, clearing.Amount);
                return;
            }
            if (clearing.Amount > 0)
                _warehouses.ReleaseSpace(clearing.Warehouse, clearing.Amount);
            _taken.Remove(clearing.Item);
        }

        /// <summary>L'entrepôt le plus proche de ce point, à vol d'oiseau, qui a la place de tant de marchandises.</summary>
        private Structure NearestWithRoom(int amount, int island, Vector2 from)
        {
            Structure nearest = null;
            float best = float.MaxValue;
            for (int i = 0; i < _warehouses.Count; i++)
            {
                if (_warehouses.StockAt(i).FreeSpace < amount)
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
