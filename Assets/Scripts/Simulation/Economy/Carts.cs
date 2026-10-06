using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Les charrettes de l'étal et les places qui les appellent (GDD, section 11 ; décisions 194 et 204) : chaque
    /// construction bâtie dont la fiche le dit garde ses charrettes (un entrepôt, le campement : une) ; chaque marque que
    /// le joueur pose sur la Place est une place de charrette. Chaque charrette prend au hasard l'un des modèles de sa
    /// fiche (décision 198). Seule fonction qui change une charrette : l'appeler, l'atteler, la tirer, la garer, la
    /// renvoyer chez elle. Les événements d'un pas attendent la fin de leur couche (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class Carts
    {
        // Le second entier du tirage du modèle d'une charrette, le premier étant son numéro.
        private const int ModelDraw = 198;

        private readonly CartDefinition _definition;
        private readonly List<Cart> _all = new List<Cart>();
        private readonly List<CartSpot> _spots = new List<CartSpot>();
        private readonly List<Structure> _tracked = new List<Structure>();
        private readonly List<Cart> _added = new List<Cart>();
        private readonly List<Cart> _changed = new List<Cart>();
        private int _nextId = 1;

        /// <summary>
        /// Les charrettes de cette fiche ; sans fiche, aucune construction ne peut en garder. Les places de charrette
        /// naissent et s'en vont avec les marques de cette Place.
        /// </summary>
        public Carts(CartDefinition definition, PlaceZone place)
        {
            if (ReferenceEquals(place, null))
                throw new ArgumentNullException(nameof(place));

            _definition = definition;
            place.MarkPlaced += OnMarkPlaced;
            place.MarkRemoved += OnMarkRemoved;
        }

        /// <summary>Levé quand une charrette naît, rangée à son entrepôt.</summary>
        public event Action<Cart> CartAdded;

        /// <summary>Levé quand une charrette est attelée, garée, ou renvoyée chez elle ; une fois par pas au plus.</summary>
        public event Action<Cart> CartChanged;

        /// <summary>Les charrettes, dans l'ordre de leur naissance.</summary>
        public IReadOnlyList<Cart> All => _all;

        /// <summary>Les places de charrette, dans l'ordre où leurs marques ont été posées : celui où elles appellent.</summary>
        public IReadOnlyList<CartSpot> Spots => _spots;

        /// <summary>La place de charrette de ce numéro ; null s'il n'y en a pas.</summary>
        public CartSpot FindSpot(int id)
        {
            for (int i = 0; i < _spots.Count; i++)
            {
                if (_spots[i].Id == id)
                    return _spots[i];
            }
            return null;
        }

        /// <summary>La charrette qu'a appelée cette place ; null si elle n'en a appelé aucune.</summary>
        public Cart CartAt(CartSpot spot)
        {
            for (int i = 0; i < _all.Count; i++)
            {
                if (ReferenceEquals(_all[i].Spot, spot))
                    return _all[i];
            }
            return null;
        }

        /// <summary>
        /// Une construction est mise en service : bâtie, elle reçoit les charrettes que dit sa fiche. Seule la mise en
        /// service du monde l'appelle (Commissioning).
        /// </summary>
        internal void Track(Structure structure)
        {
            StructureDefinition definition = structure.Definition;
            if (structure.State != StructureState.Built || definition.Carts < 1 || _tracked.Contains(structure))
                return;
            if (ReferenceEquals(_definition, null))
                throw new InvalidOperationException("Le catalogue n'a pas de charrette, et " + definition.Id +
                                                    " en garde.");
            _tracked.Add(structure);
            for (int i = 0; i < definition.Carts; i++)
            {
                int id = _nextId++;
                int models = _definition.Models.Count;
                int model = models > 0 ? new HashRandom(id, ModelDraw).Range(models) : 0;
                var cart = new Cart(id, structure, _definition, model);
                _all.Add(cart);
                _added.Add(cart);
            }
        }

        /// <summary>Le début d'un pas : chaque charrette garde où elle en est, d'où la vue la fera glisser.</summary>
        internal void BeginStep()
        {
            for (int i = 0; i < _all.Count; i++)
                _all[i].BeginStep();
        }

        /// <summary>Cette place appelle cette charrette, rangée, qu'aucune place n'appelle.</summary>
        internal void Call(CartSpot spot, Cart cart)
        {
            if (cart.State != CartState.AtHome || cart.Spot != null || CartAt(spot) != null)
                throw new InvalidOperationException("La charrette n° " + cart.Id + " ne peut pas répondre à cette place.");
            cart.Answer(spot);
        }

        /// <summary>On attelle la charrette : elle sort ici, l'avant ainsi tourné.</summary>
        internal void Hitch(Cart cart, Vector2 position, float headingDegrees)
        {
            cart.Hitch(position, headingDegrees);
            Changed(cart);
        }

        /// <summary>
        /// Celui qui la tire est ici : elle le suit au bout de son timon, l'avant tourné vers lui ; plus près, elle ne
        /// bouge pas, et se tourne seulement.
        /// </summary>
        internal void Pull(Cart cart, Vector2 puller)
        {
            Vector2 offset = cart.Position - puller;
            float distance = offset.magnitude;
            if (distance < 1e-4f)
                return;
            float reach = cart.Definition.PullMeters;
            Vector2 position = distance > reach ? puller + offset * (reach / distance) : cart.Position;
            cart.MoveTo(position, Heading.Of(-offset));
        }

        /// <summary>La charrette se gare à la place qui l'a appelée : elle y sert d'étal.</summary>
        internal void Park(Cart cart)
        {
            if (cart.Spot == null)
                throw new InvalidOperationException("La charrette n° " + cart.Id + " n'a pas de place où se garer.");
            cart.Park();
            Changed(cart);
        }

        /// <summary>La charrette retourne à son entrepôt, et la place qui l'appelait en appellera une autre.</summary>
        internal void SendHome(Cart cart)
        {
            bool seen = cart.State != CartState.AtHome;
            cart.ReturnHome();
            if (seen)
                Changed(cart);
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide les files. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _added.Count; i++)
                    CartAdded?.Invoke(_added[i]);
                for (int i = 0; i < _changed.Count; i++)
                    CartChanged?.Invoke(_changed[i]);
            }
            finally
            {
                _added.Clear();
                _changed.Clear();
            }
        }

        // Une marque posée sur la Place : une place de charrette, qui appellera une charrette (CartOffers).
        private void OnMarkPlaced(PlaceMark mark)
        {
            _spots.Add(new CartSpot(mark));
        }

        // Une marque retirée : sa charrette, que le monde n'a laissé retirer que rangée, ou garée et vide, retourne chez
        // elle ; la place s'en va avec sa consigne.
        private void OnMarkRemoved(PlaceMark mark)
        {
            CartSpot spot = FindSpot(mark.Id);
            Cart cart = spot == null ? null : CartAt(spot);
            if (cart != null)
                SendHome(cart);
            _spots.Remove(spot);
        }

        private void Changed(Cart cart)
        {
            if (!_changed.Contains(cart))
                _changed.Add(cart);
        }
    }
}
