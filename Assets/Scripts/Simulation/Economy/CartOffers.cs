using System;
using System.Collections.Generic;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Les charrettes à conduire, comme tâches à prendre (GDD, section 11 ; ARCHITECTURE, section 7) : la première place
    /// de charrette libre, d'une Place que le charretier peut rejoindre, appelle la charrette rangée de l'entrepôt le plus
    /// proche de cette Place qu'il peut rejoindre. Prendre est un appel unique qui lie l'une à l'autre : deux charretiers
    /// ne prennent jamais la même. Il va la prendre à son entrepôt, l'attelle, la tire jusqu'à sa place et l'y gare.
    /// Rendue en chemin, elle retourne à son entrepôt, comme une charge à la place qu'elle gardait (décision 178). Une
    /// Place qu'on déplace n'appelle pas, la charrette d'un entrepôt qu'on déplace ne répond pas (décision 200).
    /// </summary>
    public sealed class CartOffers
    {
        private readonly Carts _carts;
        private readonly Reach _reach;
        private readonly NavGrid _grid;

        internal CartOffers(Carts carts, Reach reach, NavGrid grid)
        {
            if (ReferenceEquals(carts, null))
                throw new ArgumentNullException(nameof(carts));
            if (ReferenceEquals(reach, null))
                throw new ArgumentNullException(nameof(reach));
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _carts = carts;
            _reach = reach;
            _grid = grid;
        }

        /// <summary>
        /// Prend une charrette à conduire pour un charretier sur cet îlot ; faux si aucune place libre qu'il peut
        /// rejoindre n'a de charrette rangée qu'il peut rejoindre.
        /// </summary>
        internal bool TryTakeDrive(int island, out Drive drive)
        {
            drive = null;
            IReadOnlyList<CartSpot> spots = _carts.Spots;
            for (int i = 0; i < spots.Count; i++)
            {
                CartSpot spot = spots[i];
                if (_carts.CartAt(spot) != null || spot.Place.State != StructureState.Built ||
                    !_reach.IsReachable(spot.Place, island))
                    continue;
                Cart cart = NearestAtHome(spot.Place.Placement.Center, island);
                if (cart == null)
                    return false;
                _carts.Call(spot, cart);
                Vector2 stand = spot.Position + Heading.Direction(spot.HeadingDegrees) * cart.Definition.PullMeters;
                drive = new Drive(cart, spot, _grid.CellOf(stand));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Le charretier est à l'entrepôt, ici : il attelle la charrette, qui sort derrière lui, l'avant tourné vers la
        /// place où il va.
        /// </summary>
        internal void Hitch(Drive drive, Vector2 carter)
        {
            if (drive.IsOver || drive.IsHitched)
                throw new InvalidOperationException("Cette charrette n'est plus à atteler.");
            Vector2 toward = drive.Spot.Position - carter;
            Vector2 ahead = toward.sqrMagnitude > 1e-8f ? toward.normalized : Vector2.up;
            _carts.Hitch(drive.Cart, carter - ahead * drive.Cart.Definition.PullMeters, Heading.Of(ahead));
            drive.MarkHitched();
        }

        /// <summary>Le charretier a avancé jusqu'ici : la charrette le suit.</summary>
        internal void Pull(Drive drive, Vector2 carter)
        {
            if (drive.IsOver || !drive.IsHitched)
                throw new InvalidOperationException("Cette charrette n'est pas attelée.");
            _carts.Pull(drive.Cart, carter);
        }

        /// <summary>Le charretier est devant la place : la charrette s'y gare, et sert d'étal.</summary>
        internal void Park(Drive drive)
        {
            if (drive.IsOver || !drive.IsHitched)
                throw new InvalidOperationException("Cette charrette n'est pas attelée.");
            drive.MarkOver();
            _carts.Park(drive.Cart);
        }

        /// <summary>
        /// La conduite ne se fera pas : la charrette retourne à son entrepôt, et sa place en appellera une autre. Finie
        /// d'abord : rien ne se rend deux fois.
        /// </summary>
        internal void Cancel(Drive drive)
        {
            if (drive.IsOver)
                return;
            drive.MarkOver();
            _carts.SendHome(drive.Cart);
        }

        // La charrette rangée, qu'aucune place n'appelle, de l'entrepôt le plus proche de ce point, à vol d'oiseau, qu'on
        // peut rejoindre depuis cet îlot ; null s'il n'y en a pas.
        private Cart NearestAtHome(Vector2 from, int island)
        {
            Cart nearest = null;
            float best = float.MaxValue;
            IReadOnlyList<Cart> all = _carts.All;
            for (int i = 0; i < all.Count; i++)
            {
                Cart cart = all[i];
                if (cart.State != CartState.AtHome || cart.Spot != null || cart.Home.State != StructureState.Built)
                    continue;
                float distance = Vector2.Distance(cart.Home.Placement.Center, from);
                if (distance < best && _reach.IsReachable(cart.Home, island))
                {
                    best = distance;
                    nearest = cart;
                }
            }
            return nearest;
        }
    }
}
