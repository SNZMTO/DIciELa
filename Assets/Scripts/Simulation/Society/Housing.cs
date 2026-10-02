using System;
using System.Collections.Generic;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Les logements (GDD, section 4 ; ARCHITECTURE, section 4) : qui a son lit dans quelle maison. Les lits d'une maison
    /// sont ceux que sa fiche offre, une fois bâtie ; ses lits libres s'en déduisent, jamais gardés. Un habitant reçoit
    /// le sien à son arrivée. La seule à changer qui dort où.
    /// </summary>
    public sealed class Housing
    {
        private readonly Structures _structures;
        private readonly PlacementRules _rules;
        private readonly List<Villager> _lodgers = new List<Villager>();
        private readonly List<Structure> _houses = new List<Structure>();

        /// <summary>Les logements de ces constructions ; leurs portes, d'après ces règles de pose.</summary>
        public Housing(Structures structures, PlacementRules rules)
        {
            _structures = structures ?? throw new ArgumentNullException(nameof(structures));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        /// <summary>Le point devant la porte de cette maison, au milieu du sol qu'elle garde libre : où l'on dort.</summary>
        public Vector2 DoorOf(Structure house)
        {
            return _rules.Doorway(house.Definition, house.Placement).Center;
        }

        /// <summary>La maison où cet habitant a son lit ; null s'il n'en a pas.</summary>
        public Structure HouseOf(Villager villager)
        {
            int index = _lodgers.IndexOf(villager);
            return index < 0 ? null : _houses[index];
        }

        /// <summary>Les lits libres de cette maison : ceux qu'elle offre, bâtie, moins ceux qu'on y a donnés.</summary>
        public int FreeBedsIn(Structure house)
        {
            if (house == null || house.State != StructureState.Built)
                return 0;
            int taken = 0;
            for (int i = 0; i < _houses.Count; i++)
            {
                if (ReferenceEquals(_houses[i], house))
                    taken++;
            }
            return Math.Max(0, house.Definition.Beds - taken);
        }

        /// <summary>
        /// La maison bâtie qui a un lit libre, la plus proche de ce point par le milieu de son empreinte ; à égalité, la
        /// première posée. Null s'il n'y a plus de lit libre en ville.
        /// </summary>
        public Structure NearestWithAFreeBed(Vector2 point)
        {
            Structure nearest = null;
            float best = float.MaxValue;
            IReadOnlyList<Structure> all = _structures.All;
            for (int i = 0; i < all.Count; i++)
            {
                Structure house = all[i];
                if (FreeBedsIn(house) == 0)
                    continue;
                float distance = (house.Placement.Center - point).sqrMagnitude;
                if (distance < best)
                {
                    best = distance;
                    nearest = house;
                }
            }
            return nearest;
        }

        /// <summary>
        /// Donne à cet habitant un lit dans cette maison, qui doit en avoir un de libre ; il n'en a pas encore. Seul le
        /// monde l'appelle, à son arrivée.
        /// </summary>
        internal void Lodge(Villager villager, Structure house)
        {
            if (villager == null)
                throw new ArgumentNullException(nameof(villager));
            if (HouseOf(villager) != null)
                throw new InvalidOperationException("L'habitant n° " + villager.Id + " a déjà son lit.");
            if (FreeBedsIn(house) == 0)
                throw new InvalidOperationException("Cette maison n'a pas de lit libre.");
            _lodgers.Add(villager);
            _houses.Add(house);
        }
    }
}
