using System;
using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// L'étal (GDD, section 11 ; décisions 194 et 202) : la consigne de chaque place de charrette, que le joueur règle dans
    /// la fiche de la Place, et ce qu'elle demande à la charrette garée. Ligne après ligne, ce qui lui manque, dans la
    /// limite de ses places libres : une quantité, ou tout ce qui dépasse « garder au moins N » de ce que la ville peut
    /// donner. Et ce qu'elle a en trop, quand le joueur a baissé une quantité ou retiré une ligne : rien d'autre ne fait
    /// rendre une charrette, et une ligne « garder au moins N » ne fait que remplir, sans quoi les marchandises iraient et
    /// viendraient au gré du stock. Seule fonction qui change une consigne ; elle tient aussi ce qu'on apporte déjà à
    /// chaque charrette, des réservations, jamais sauvegardées. Les événements d'un pas attendent la fin de leur couche.
    /// </summary>
    public sealed class Stalls
    {
        private readonly Carts _carts;
        private readonly Warehouses _warehouses;
        private readonly Dictionary<(Cart, GoodDefinition), int> _incoming = new Dictionary<(Cart, GoodDefinition), int>();
        private readonly List<CartSpot> _changed = new List<CartSpot>();

        public Stalls(Carts carts, Warehouses warehouses)
        {
            _carts = carts ?? throw new ArgumentNullException(nameof(carts));
            _warehouses = warehouses ?? throw new ArgumentNullException(nameof(warehouses));
        }

        /// <summary>Levé quand la consigne d'une place de charrette change ; une fois par pas au plus.</summary>
        public event Action<CartSpot> ConsigneChanged;

        /// <summary>La place de charrette de ce rang sur la Place de ce numéro ; null s'il n'y en a pas.</summary>
        public CartSpot SpotOf(int placeId, int index)
        {
            IReadOnlyList<CartSpot> spots = _carts.Spots;
            for (int i = 0; i < spots.Count; i++)
            {
                if (spots[i].Place.Id == placeId && spots[i].Index == index)
                    return spots[i];
            }
            return null;
        }

        /// <summary>Ce qu'on apporte déjà de cette marchandise à cette charrette.</summary>
        public int IncomingOf(Cart cart, GoodDefinition good)
        {
            return _incoming.TryGetValue((cart, good), out int amount) ? amount : 0;
        }

        /// <summary>
        /// Ce qui manque encore à cette charrette pour cette ligne de sa consigne, sans ce qu'on lui apporte déjà ; 0 si
        /// rien. Ses places libres le bornent, mais pas ici : c'est le porteur qui les compte.
        /// </summary>
        public int MissingFor(Cart cart, ConsigneLine line)
        {
            if (line.Mode == ConsigneMode.KeepAtLeast)
                return Math.Max(0, _warehouses.CityAvailableOf(line.Good) - line.Number);
            return Math.Max(0, line.Number - cart.Stock.CountOf(line.Good) - IncomingOf(cart, line.Good));
        }

        /// <summary>
        /// Ce que cette charrette, garée à cette place, a en trop de cette marchandise, sans ce qu'on en emporte déjà : ce
        /// que sa consigne ne demande plus. Une ligne « garder au moins N » ne rend jamais rien.
        /// </summary>
        public int SurplusOf(Cart cart, CartSpot spot, GoodDefinition good)
        {
            int kept = cart.Stock.AvailableOf(good);
            if (!spot.TryGetLine(good, out ConsigneLine line))
                return kept;
            return line.Mode == ConsigneMode.Amount ? Math.Max(0, kept - line.Number) : 0;
        }

        /// <summary>
        /// Met cette ligne dans la consigne de cette place, à la place de celle de la même marchandise, ou au bout ; une
        /// ligne sans marchandise ou d'un nombre négatif ne change rien. Seul l'ordre « garnir » l'appelle.
        /// </summary>
        internal void SetLine(CartSpot spot, ConsigneLine line)
        {
            if (ReferenceEquals(line.Good, null) || line.Number < 0)
                return;
            spot.SetLine(line);
            Changed(spot);
        }

        /// <summary>Retire de la consigne de cette place la ligne de cette marchandise. Seul un ordre l'appelle.</summary>
        internal void RemoveLine(CartSpot spot, GoodDefinition good)
        {
            if (spot.RemoveLine(good))
                Changed(spot);
        }

        /// <summary>Un porteur apporte tant de cette marchandise à cette charrette.</summary>
        internal void Promise(Cart cart, GoodDefinition good, int amount)
        {
            _incoming[(cart, good)] = IncomingOf(cart, good) + amount;
        }

        /// <summary>Ce que le porteur apportait est arrivé, ou ne viendra pas.</summary>
        internal void Unpromise(Cart cart, GoodDefinition good, int amount)
        {
            int left = IncomingOf(cart, good) - amount;
            if (left < 0)
                throw new InvalidOperationException("On n'apportait pas " + amount + " de " + good.Id + " à cette charrette.");
            if (left == 0)
                _incoming.Remove((cart, good));
            else
                _incoming[(cart, good)] = left;
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide la file. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _changed.Count; i++)
                    ConsigneChanged?.Invoke(_changed[i]);
            }
            finally
            {
                _changed.Clear();
            }
        }

        private void Changed(CartSpot spot)
        {
            if (!_changed.Contains(spot))
                _changed.Add(spot);
        }
    }
}
