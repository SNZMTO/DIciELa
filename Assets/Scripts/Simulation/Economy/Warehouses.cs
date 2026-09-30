using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Ce qui tient les marchandises de la ville (GDD, section 8) : le tas de départ, puis les entrepôts ; toute
    /// construction bâtie dont la fiche dit qu'elle en tient. Le stock de la ville est ce qu'ils tiennent, un chiffre par
    /// marchandise. Seule fonction qui fait naître des marchandises (<see cref="Create"/>), et seule qui rend à la ville
    /// ce qu'un porteur n'a pas livré (<see cref="ReturnToTown"/>). Les événements d'un pas attendent la fin de leur
    /// couche (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class Warehouses
    {
        private readonly List<Structure> _structures = new List<Structure>();
        private readonly List<Inventory> _stocks = new List<Inventory>();
        private readonly List<Structure> _changed = new List<Structure>();
        private readonly GoodsLedger _ledger;

        public Warehouses(GoodsLedger ledger)
        {
            if (ReferenceEquals(ledger, null))
                throw new ArgumentNullException(nameof(ledger));

            _ledger = ledger;
        }

        /// <summary>Levé quand un entrepôt s'ouvre, ou que ce qu'il tient change ; une fois par pas au plus.</summary>
        public event Action<Structure> StockChanged;

        /// <summary>Le nombre d'entrepôts, dans l'ordre où ils ont servi.</summary>
        public int Count => _structures.Count;

        public Structure StructureAt(int index)
        {
            return _structures[index];
        }

        public Inventory StockAt(int index)
        {
            return _stocks[index];
        }

        /// <summary>Le stock de cette construction ; null si elle ne tient pas les marchandises de la ville.</summary>
        public Inventory StockOf(Structure structure)
        {
            int index = _structures.IndexOf(structure);
            return index < 0 ? null : _stocks[index];
        }

        /// <summary>Le stock de la ville pour cette marchandise : ce que tiennent tous ses entrepôts.</summary>
        public int CityStockOf(GoodDefinition good)
        {
            int sum = 0;
            for (int i = 0; i < _stocks.Count; i++)
                sum += _stocks[i].CountOf(good);
            return sum;
        }

        /// <summary>
        /// Une construction vient d'être posée ou bâtie : si elle est bâtie et que sa fiche dit qu'elle tient des
        /// marchandises, elle sert d'entrepôt. Seuls le monde et les chantiers l'appellent.
        /// </summary>
        internal void Track(Structure structure)
        {
            if (structure.State != StructureState.Built || structure.Definition.StorageCapacity < 1 ||
                _structures.Contains(structure))
                return;
            _structures.Add(structure);
            _stocks.Add(new Inventory(structure.Definition.StorageCapacity));
            Changed(structure);
        }

        /// <summary>Fait naître ces marchandises dans cet entrepôt, pour cette cause.</summary>
        internal void Create(Structure warehouse, GoodDefinition good, int amount, GoodsCause cause)
        {
            Inventory stock = Required(warehouse);
            stock.Add(good, amount);
            _ledger.Created(good, amount, cause);
            Changed(warehouse);
        }

        /// <summary>Promet ces marchandises à un porteur : elles restent là, personne d'autre ne les prend.</summary>
        internal void Reserve(Structure warehouse, GoodDefinition good, int amount)
        {
            Required(warehouse).Reserve(good, amount);
        }

        internal void Release(Structure warehouse, GoodDefinition good, int amount)
        {
            Required(warehouse).Release(good, amount);
        }

        /// <summary>Le porteur prend ce qu'on lui avait promis ; il le met dans sa charge.</summary>
        internal void Take(Structure warehouse, GoodDefinition good, int amount, Inventory load)
        {
            Required(warehouse).Remove(good, amount, true);
            load.Add(good, amount);
            Changed(warehouse);
        }

        /// <summary>
        /// Rend à la ville tout ce que tient cette charge : chaque marchandise va dans les entrepôts qui ont la place,
        /// dans l'ordre où ils ont servi. Tant que seul le tas de départ fait naître des marchandises, la place où elles
        /// étaient reste libre ; une ville sans place pour elles est une erreur.
        /// </summary>
        internal void ReturnToTown(Inventory load)
        {
            for (int line = 0; line < load.LineCount; line++)
            {
                GoodDefinition good = load.GoodAt(line);
                int left = load.CountAt(line);
                for (int i = 0; i < _stocks.Count && left > 0; i++)
                {
                    int amount = Math.Min(left, _stocks[i].FreeSpace);
                    if (amount < 1)
                        continue;
                    load.Remove(good, amount, false);
                    _stocks[i].Add(good, amount);
                    Changed(_structures[i]);
                    left -= amount;
                }
                if (left > 0)
                    throw new InvalidOperationException("La ville n'a pas la place de reprendre " + left + " " +
                                                        good.Id + ".");
            }
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide la file. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _changed.Count; i++)
                    StockChanged?.Invoke(_changed[i]);
            }
            finally
            {
                _changed.Clear();
            }
        }

        private Inventory Required(Structure warehouse)
        {
            Inventory stock = StockOf(warehouse);
            if (stock == null)
                throw new InvalidOperationException("La construction n° " + warehouse.Id + " n'est pas un entrepôt.");
            return stock;
        }

        private void Changed(Structure warehouse)
        {
            if (!_changed.Contains(warehouse))
                _changed.Add(warehouse);
        }
    }
}
