using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Garnir les charrettes de l'étal, comme tâches à prendre (GDD, section 11 ; ARCHITECTURE, section 7 ; décision 202) :
    /// pour une charrette garée sur une Place bâtie que le porteur peut rejoindre, son surplus d'abord, vers l'entrepôt le
    /// plus proche qui a la place, puis ce qui manque à sa consigne, ligne après ligne, depuis l'entrepôt le plus proche qui
    /// en a, dans la limite de ses places libres et de sa charge. Prendre réserve des deux côtés ; comme une livraison
    /// (décision 178), la place qu'une charge quitte reste retenue tant qu'on la porte, et une charge rendue y revient.
    /// Seule fonction qui change le contenu d'une charrette.
    /// </summary>
    public sealed class StallOffers
    {
        private readonly Carts _carts;
        private readonly Stalls _stalls;
        private readonly Warehouses _warehouses;
        private readonly Reach _reach;
        private readonly NavGrid _grid;

        internal StallOffers(Carts carts, Stalls stalls, Warehouses warehouses, Reach reach, NavGrid grid)
        {
            _carts = carts ?? throw new ArgumentNullException(nameof(carts));
            _stalls = stalls ?? throw new ArgumentNullException(nameof(stalls));
            _warehouses = warehouses ?? throw new ArgumentNullException(nameof(warehouses));
            _reach = reach ?? throw new ArgumentNullException(nameof(reach));
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        }

        /// <summary>
        /// Prend de quoi garnir une charrette, ou en rapporter le surplus, pour un porteur sur cet îlot, près de ce point,
        /// avec cette charge ; faux s'il n'y a rien à porter pour l'étal qu'il puisse rejoindre.
        /// </summary>
        internal bool TryTakeStock(int island, Vector2 from, Inventory load, out StallDelivery delivery)
        {
            delivery = null;
            IReadOnlyList<CartSpot> spots = _carts.Spots;
            // Le surplus d'abord : il libère des places.
            for (int i = 0; i < spots.Count; i++)
            {
                Cart cart = ParkedAt(spots[i], island);
                if (cart != null && TryReturn(spots[i], cart, island, load, out delivery))
                    return true;
            }
            for (int i = 0; i < spots.Count; i++)
            {
                Cart cart = ParkedAt(spots[i], island);
                if (cart != null && TryFill(spots[i], cart, island, from, load, out delivery))
                    return true;
            }
            return false;
        }

        /// <summary>Le porteur est à la source : il prend ce qui lui était promis dans sa charge.</summary>
        internal void PickUp(StallDelivery delivery, Inventory load)
        {
            if (delivery.IsOver || delivery.IsPickedUp)
                throw new InvalidOperationException("Cette part de l'étal n'a plus rien à prendre.");
            if (delivery.ToCart)
            {
                _warehouses.Take(delivery.Warehouse, delivery.Good, delivery.Amount, load);
            }
            else
            {
                Inventory stock = delivery.Cart.Stock;
                stock.Remove(delivery.Good, delivery.Amount, true);
                stock.ReserveSpace(delivery.Good, delivery.Amount);
                load.Add(delivery.Good, delivery.Amount);
            }
            delivery.MarkPickedUp();
        }

        /// <summary>Le porteur est arrivé : il dépose sa charge, et la place qu'elle gardait à sa source se rend.</summary>
        internal void DropOff(StallDelivery delivery, Inventory load)
        {
            if (delivery.IsOver || !delivery.IsPickedUp)
                throw new InvalidOperationException("Cette part de l'étal n'a rien à déposer.");
            delivery.MarkOver();
            if (delivery.ToCart)
            {
                _stalls.Unpromise(delivery.Cart, delivery.Good, delivery.Amount);
                IntoCart(delivery, load);
                _warehouses.ReleaseSpace(delivery.Warehouse, delivery.Good, delivery.Amount);
            }
            else
            {
                _warehouses.Store(delivery.Warehouse, load, delivery.Good, delivery.Amount);
                delivery.Cart.Stock.ReleaseSpace(delivery.Good, delivery.Amount);
            }
        }

        /// <summary>
        /// Cette part de l'étal ne se fera pas : les promesses sont rendues, et ce que le porteur avait pris retourne à sa
        /// source, à la place qu'elle lui gardait. Finie d'abord : rien ne se rend deux fois.
        /// </summary>
        internal void Cancel(StallDelivery delivery, Inventory load)
        {
            if (delivery.IsOver)
                return;
            delivery.MarkOver();
            if (delivery.ToCart)
            {
                _stalls.Unpromise(delivery.Cart, delivery.Good, delivery.Amount);
                delivery.Cart.Stock.ReleaseSpace(delivery.Good, delivery.Amount);
                if (delivery.IsPickedUp)
                    _warehouses.Store(delivery.Warehouse, load, delivery.Good, delivery.Amount);
                else
                    _warehouses.Release(delivery.Warehouse, delivery.Good, delivery.Amount);
                return;
            }
            _warehouses.ReleaseSpace(delivery.Warehouse, delivery.Good, delivery.Amount);
            if (delivery.IsPickedUp)
                IntoCart(delivery, load);
            else
                delivery.Cart.Stock.Release(delivery.Good, delivery.Amount);
        }

        // La charge entre dans la charrette, à la place qu'elle y avait retenue.
        private static void IntoCart(StallDelivery delivery, Inventory load)
        {
            Inventory stock = delivery.Cart.Stock;
            stock.ReleaseSpace(delivery.Good, delivery.Amount);
            load.Remove(delivery.Good, delivery.Amount, false);
            stock.Add(delivery.Good, delivery.Amount);
        }

        // La charrette garée à cette place, sur une Place bâtie qu'on peut rejoindre depuis cet îlot ; null sinon.
        private Cart ParkedAt(CartSpot spot, int island)
        {
            Cart cart = _carts.CartAt(spot);
            if (cart == null || cart.State != CartState.Parked || spot.Place.State != StructureState.Built ||
                !_reach.IsReachable(spot.Place, island))
                return null;
            return cart;
        }

        private bool TryReturn(CartSpot spot, Cart cart, int island, Inventory load, out StallDelivery delivery)
        {
            delivery = null;
            Inventory stock = cart.Stock;
            for (int line = 0; line < stock.LineCount; line++)
            {
                GoodDefinition good = stock.GoodAt(line);
                int amount = Math.Min(_stalls.SurplusOf(cart, spot, good), load.RoomFor(good));
                if (amount < 1)
                    continue;
                Structure warehouse = _reach.NearestWithRoom(_warehouses, good, amount, island, spot.Position);
                if (warehouse == null)
                    continue;
                stock.Reserve(good, amount);
                _warehouses.ReserveSpace(warehouse, good, amount);
                delivery = new StallDelivery(cart, warehouse, good, amount, false, CellOf(spot, cart));
                return true;
            }
            return false;
        }

        private bool TryFill(CartSpot spot, Cart cart, int island, Vector2 from, Inventory load,
            out StallDelivery delivery)
        {
            delivery = null;
            IReadOnlyList<ConsigneLine> consigne = spot.Consigne;
            for (int i = 0; i < consigne.Count; i++)
            {
                GoodDefinition good = consigne[i].Good;
                int wanted = Math.Min(Math.Min(_stalls.MissingFor(cart, consigne[i]), cart.Stock.RoomFor(good)),
                    load.RoomFor(good));
                if (wanted < 1)
                    continue;
                Structure source = _reach.NearestWithGood(_warehouses, good, island, from);
                if (source == null)
                    continue;
                int amount = Math.Min(wanted, _warehouses.StockOf(source).AvailableOf(good));
                _warehouses.Reserve(source, good, amount);
                cart.Stock.ReserveSpace(good, amount);
                _stalls.Promise(cart, good, amount);
                delivery = new StallDelivery(cart, source, good, amount, true, CellOf(spot, cart));
                return true;
            }
            return false;
        }

        // La maille devant la charrette garée, au bout de son timon.
        private Vector2Int CellOf(CartSpot spot, Cart cart)
        {
            return _grid.CellOf(spot.StandAt(cart.Definition.PullMeters));
        }
    }
}
