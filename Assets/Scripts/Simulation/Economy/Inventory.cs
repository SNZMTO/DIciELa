using System;
using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Un stock (ARCHITECTURE, section 3) : ce que tient un entrepôt, un tas au sol, la charge d'un porteur, une charrette.
    /// Son contenu, une ligne par marchandise dans l'ordre où elle est entrée, et sa place, qui se compte en places :
    /// chaque marchandise en prend autant que dit son encombrement (GDD, section 8 ; décision 194). Il garde aussi ses
    /// réservations, jamais sauvegardées : ce qui est promis à un porteur n'est plus disponible pour un autre, et la place
    /// retenue pour ce qu'on lui apporte n'est plus libre pour autre chose (ARCHITECTURE, section 7).
    /// </summary>
    public sealed class Inventory
    {
        private readonly List<GoodDefinition> _goods = new List<GoodDefinition>();
        private readonly List<int> _counts = new List<int>();
        private readonly List<int> _reserved = new List<int>();

        public Inventory(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Un stock a au moins une place.");

            Capacity = capacity;
        }

        /// <summary>Le nombre de places qu'il a.</summary>
        public int Capacity { get; }

        /// <summary>Le nombre de marchandises qu'il tient.</summary>
        public int Total { get; private set; }

        /// <summary>Les places que prennent les marchandises qu'il tient.</summary>
        public int Used { get; private set; }

        /// <summary>Les places retenues pour ce qu'on lui apporte : elles ne sont plus libres.</summary>
        public int SpaceReserved { get; private set; }

        /// <summary>Les places qui restent, sans celles qui sont retenues.</summary>
        public int FreeSpace => Capacity - Used - SpaceReserved;

        /// <summary>Le nombre de lignes : les marchandises qu'il a tenues, dans l'ordre où elles sont entrées.</summary>
        public int LineCount => _goods.Count;

        public GoodDefinition GoodAt(int line)
        {
            return _goods[line];
        }

        public int CountAt(int line)
        {
            return _counts[line];
        }

        /// <summary>Combien il tient de cette marchandise.</summary>
        public int CountOf(GoodDefinition good)
        {
            int line = LineOf(good);
            return line < 0 ? 0 : _counts[line];
        }

        /// <summary>Combien il en tient qui n'est promis à personne.</summary>
        public int AvailableOf(GoodDefinition good)
        {
            int line = LineOf(good);
            return line < 0 ? 0 : _counts[line] - _reserved[line];
        }

        /// <summary>Combien de cette marchandise tiendraient encore dans les places libres.</summary>
        public int RoomFor(GoodDefinition good)
        {
            if (ReferenceEquals(good, null))
                throw new ArgumentNullException(nameof(good));
            return FreeSpace / good.Space;
        }

        internal void Add(GoodDefinition good, int amount)
        {
            CheckAmount(amount);
            if (amount > RoomFor(good))
                throw new InvalidOperationException("Le stock n'a pas la place de " + amount + " de plus.");
            int line = LineOf(good);
            if (line < 0)
            {
                _goods.Add(good);
                _counts.Add(0);
                _reserved.Add(0);
                line = _goods.Count - 1;
            }
            _counts[line] += amount;
            Total += amount;
            Used += amount * good.Space;
        }

        /// <summary>Retire des marchandises, prises d'abord sur celles qu'on a promises si <paramref name="reserved"/>.</summary>
        internal void Remove(GoodDefinition good, int amount, bool reserved)
        {
            CheckAmount(amount);
            int line = LineOf(good);
            if (line < 0 || _counts[line] < amount || (reserved && _reserved[line] < amount))
                throw new InvalidOperationException("Le stock ne tient pas " + amount + " de cette marchandise.");
            _counts[line] -= amount;
            if (reserved)
                _reserved[line] -= amount;
            Total -= amount;
            Used -= amount * good.Space;
        }

        /// <summary>Promet des marchandises disponibles : elles restent là, mais personne d'autre ne les prend.</summary>
        internal void Reserve(GoodDefinition good, int amount)
        {
            CheckAmount(amount);
            if (AvailableOf(good) < amount)
                throw new InvalidOperationException("Le stock n'a pas " + amount + " de cette marchandise à promettre.");
            _reserved[LineOf(good)] += amount;
        }

        /// <summary>Retient la place de tant de cette marchandise, qu'on lui apportera : personne d'autre ne la prend.</summary>
        internal void ReserveSpace(GoodDefinition good, int amount)
        {
            CheckAmount(amount);
            if (amount > RoomFor(good))
                throw new InvalidOperationException("Le stock n'a pas la place de retenir " + amount + " de plus.");
            SpaceReserved += amount * good.Space;
        }

        /// <summary>Rend la place retenue pour tant de cette marchandise : elle redevient libre.</summary>
        internal void ReleaseSpace(GoodDefinition good, int amount)
        {
            CheckAmount(amount);
            int places = amount * good.Space;
            if (places > SpaceReserved)
                throw new InvalidOperationException("Le stock n'a pas retenu la place de " + amount + ".");
            SpaceReserved -= places;
        }

        /// <summary>Rend une promesse : les marchandises redeviennent disponibles.</summary>
        internal void Release(GoodDefinition good, int amount)
        {
            CheckAmount(amount);
            int line = LineOf(good);
            if (line < 0 || _reserved[line] < amount)
                throw new InvalidOperationException("Le stock n'a pas promis " + amount + " de cette marchandise.");
            _reserved[line] -= amount;
        }

        private int LineOf(GoodDefinition good)
        {
            if (ReferenceEquals(good, null))
                throw new ArgumentNullException(nameof(good));
            for (int i = 0; i < _goods.Count; i++)
            {
                if (ReferenceEquals(_goods[i], good))
                    return i;
            }
            return -1;
        }

        private static void CheckAmount(int amount)
        {
            if (amount < 1)
                throw new ArgumentOutOfRangeException(nameof(amount), "Une quantité compte au moins une marchandise.");
        }
    }
}
