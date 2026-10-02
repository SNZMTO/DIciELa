using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Ce qui tient les marchandises de la ville (GDD, section 8) : le campement, puis les entrepôts ; toute
    /// construction bâtie dont la fiche dit qu'elle en tient. Le stock de la ville est ce qu'ils tiennent, un chiffre par
    /// marchandise. Seule fonction qui fait naître des marchandises (<c>Create</c>, dans un entrepôt ou dans la charge
    /// d'un habitant), et seule qui rend à la ville ce qu'un habitant porte (<see cref="Store"/>) : toujours à une place
    /// retenue pour lui, celle d'où sa charge vient ou celle qu'on lui a gardée (décision 178). Les événements d'un pas
    /// attendent la fin de leur couche (ARCHITECTURE, section 5).
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

        /// <summary>
        /// Levé une fois à la fin d'un pas où le stock de la ville a changé, après les StockChanged de ses entrepôts :
        /// pour qui montre la ville entière, une seule fois, quel que soit le nombre d'entrepôts qui ont bougé.
        /// </summary>
        public event Action CityStockChanged;

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
        /// Une construction est mise en service : si elle est bâtie et que sa fiche dit qu'elle tient des marchandises,
        /// elle sert d'entrepôt. Seule la mise en service du monde l'appelle (Commissioning).
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

        /// <summary>
        /// Fait naître ces marchandises dans la charge d'un habitant, pour cette cause : ce que rend un arbre abattu.
        /// </summary>
        internal void Create(Inventory load, GoodDefinition good, int amount, GoodsCause cause)
        {
            load.Add(good, amount);
            _ledger.Created(good, amount, cause);
        }

        /// <summary>Retient dans cet entrepôt la place de ce qu'un habitant y apportera.</summary>
        internal void ReserveSpace(Structure warehouse, int amount)
        {
            Required(warehouse).ReserveSpace(amount);
        }

        internal void ReleaseSpace(Structure warehouse, int amount)
        {
            Required(warehouse).ReleaseSpace(amount);
        }

        /// <summary>L'habitant range dans cet entrepôt ce qu'il porte, à la place qu'on lui avait retenue.</summary>
        internal void Store(Structure warehouse, Inventory load, GoodDefinition good, int amount)
        {
            Inventory stock = Required(warehouse);
            stock.ReleaseSpace(amount);
            load.Remove(good, amount, false);
            stock.Add(good, amount);
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

        /// <summary>
        /// Le porteur prend ce qu'on lui avait promis ; il le met dans sa charge. La place qu'il quitte reste retenue tant
        /// qu'il porte (décision 178) : livré, elle se rend ; rendu, sa charge y revient toujours (<see cref="Store"/>).
        /// </summary>
        internal void Take(Structure warehouse, GoodDefinition good, int amount, Inventory load)
        {
            Inventory stock = Required(warehouse);
            stock.Remove(good, amount, true);
            stock.ReserveSpace(amount);
            load.Add(good, amount);
            Changed(warehouse);
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide la file. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            if (_changed.Count == 0)
                return;
            try
            {
                for (int i = 0; i < _changed.Count; i++)
                    StockChanged?.Invoke(_changed[i]);
                CityStockChanged?.Invoke();
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
