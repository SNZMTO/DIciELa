using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Les habitants de la ville (ARCHITECTURE, section 4) : la vérité de qui vit ici et où. La Chimère, que le joueur
    /// crée au lancement d'une partie, et qu'il envoie dormir dans son logis ou réveille ; les habitants qui arrivent, et
    /// rentrent dormir chez eux la nuit (l'un et l'autre dans Bedtimes). Une seule arrivée pour tous. Chacun joue son pas
    /// dans l'ordre de son arrivée. Les événements d'un pas attendent la fin de leur couche, dans une seule file
    /// (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class Villagers
    {
        private readonly List<Villager> _all = new List<Villager>();
        private readonly List<VillagerHappening> _happened = new List<VillagerHappening>();
        private readonly VillagerServices _services;
        private readonly NavGrid _grid;
        private readonly Structures _structures;
        private readonly Housing _housing;
        private int _nextId = 1;

        /// <summary>
        /// Les habitants de ce monde : ils vivent sur ce maillage, à cette horloge, prennent leurs tâches ici, leurs
        /// statistiques suivent celles-ci, et leurs bornes, ces réglages ; ils dorment dans les constructions de la ville,
        /// la Chimère dans son logis, les autres là où ils ont leur lit.
        /// </summary>
        public Villagers(NavGrid grid, Islands islands, PathFinder finder, Clock clock, Surroundings surroundings,
            TaskOffers offers, ClearingOffers clearings, CartOffers carts, StallOffers stalls,
            StatisticTable statistics, SimulationSettings settings, Structures structures, Housing housing)
        {
            _services = new VillagerServices(clock, grid, islands, finder, surroundings, offers, clearings, carts,
                stalls, statistics, settings);
            _grid = grid;
            _structures = structures ?? throw new ArgumentNullException(nameof(structures));
            _housing = housing ?? throw new ArgumentNullException(nameof(housing));
            Bedtimes = new Bedtimes(grid, clock.Phase, _happened, DoorOf);
        }

        /// <summary>Levé quand un habitant arrive en ville : la Chimère à sa création, puis chaque arrivant.</summary>
        public event Action<Villager> VillagerArrived;

        /// <summary>Levé quand un ordre « créer la Chimère » est refusé, avec la sorte refusée : elle existe déjà.</summary>
        public event Action<VillagerKindDefinition> ChimeraRefused;

        /// <summary>Levé quand un habitant part dormir : il marche vers sa porte.</summary>
        public event Action<Villager> VillagerWentToBed;

        /// <summary>Levé quand un habitant arrive à sa porte et s'endort.</summary>
        public event Action<Villager> VillagerFellAsleep;

        /// <summary>
        /// Levé quand un habitant se réveille, ou cesse d'aller dormir (réveillé en chemin, ou parce que sa porte n'est
        /// plus joignable) : il se remet à flâner.
        /// </summary>
        public event Action<Villager> VillagerWokeUp;

        /// <summary>Levé quand un ordre « dormir » ou « réveiller » est refusé, avec la raison.</summary>
        public event Action<SleepRefusal> SleepRefused;

        public IReadOnlyList<Villager> All => _all;

        /// <summary>La Chimère, une fois créée ; null avant.</summary>
        public Villager Chimera { get; private set; }

        /// <summary>Le coucher et le lever : les ordres « dormir » et « réveiller » de la Chimère y passent.</summary>
        internal Bedtimes Bedtimes { get; }

        /// <summary>L'habitant qui porte ce numéro ; null s'il n'y en a pas.</summary>
        public Villager Find(int id)
        {
            foreach (Villager villager in _all)
            {
                if (villager.Id == id)
                    return villager;
            }
            return null;
        }

        /// <summary>
        /// Là où dort cet habitant : la Chimère, dans son logis (GDD, section 6) ; un autre, dans la maison où il a son
        /// lit (GDD, section 4). Null s'il n'en a pas. La seule réponse, pour la nuit comme pour la vue.
        /// </summary>
        public Structure HomeOf(Villager villager)
        {
            return ReferenceEquals(villager, Chimera) ? _structures.ChimeraHome : _housing.HouseOf(villager);
        }

        /// <summary>
        /// Un habitant arrive en ville, de cette sorte, avec cette apparence, ce nom et ce point fort, sur le milieu de la
        /// maille de ce point (ramené dans la carte) : la seule arrivée. Il se met à flâner là. Seuls le monde (le déroulé
        /// du départ) et l'ordre « créer la Chimère » l'appellent.
        /// </summary>
        internal Villager Arrive(VillagerKindDefinition kind, Appearance appearance, string nameKey, Vector2 at,
            Strength strength)
        {
            if (ReferenceEquals(kind, null))
                throw new ArgumentNullException(nameof(kind));

            Vector2Int cell = _grid.CellOf(at);
            cell = new Vector2Int(Math.Clamp(cell.x, 0, _grid.Width - 1), Math.Clamp(cell.y, 0, _grid.Depth - 1));
            var villager = new Villager(_nextId++, kind, appearance, nameKey, _grid.CellCenter(cell), strength, _services);
            _all.Add(villager);
            _happened.Add(VillagerHappening.Of(VillagerHappening.Kinds.Arrived, villager));
            return villager;
        }

        /// <summary>
        /// Crée la Chimère au point d'arrivée, avec cette apparence. Une seule par monde : si elle existe déjà, faux, rien
        /// ne change, et <see cref="ChimeraRefused"/> le dira. Seul l'ordre « créer la Chimère » l'appelle.
        /// </summary>
        internal bool CreateChimera(VillagerKindDefinition kind, Appearance appearance, Vector2 arrivalPoint)
        {
            if (ReferenceEquals(kind, null))
                throw new ArgumentNullException(nameof(kind));
            if (Chimera != null)
            {
                _happened.Add(VillagerHappening.ChimeraRefusedWith(kind));
                return false;
            }
            Chimera = Arrive(kind, appearance, kind.NameKey, arrivalPoint, Strength.None);
            return true;
        }

        /// <summary>
        /// Couche ou lève ceux qui rentrent dormir, puis joue un pas pour chaque habitant, dans l'ordre de leur arrivée ;
        /// celui qui s'endort, ou qui renonce en chemin à son lit, le dit. Seul le monde l'appelle.
        /// </summary>
        internal void Step()
        {
            Bedtimes.Step(_all, _services.Clock.Phase);
            for (int i = 0; i < _all.Count; i++)
            {
                Villager villager = _all[i];
                bool wasAsleep = villager.IsAsleep;
                bool wasGoingToBed = villager.IsGoingToBed;
                villager.Step();
                if (!wasAsleep && villager.IsAsleep)
                    _happened.Add(VillagerHappening.Of(VillagerHappening.Kinds.FellAsleep, villager));
                else if (wasGoingToBed && !villager.IsGoingToBed)
                    _happened.Add(VillagerHappening.Of(VillagerHappening.Kinds.WokeUp, villager));
            }
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide la file. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _happened.Count; i++)
                    Deliver(_happened[i]);
            }
            finally
            {
                _happened.Clear();
            }
        }

        // Le point devant la porte de chez lui, au milieu du sol qu'elle garde libre ; null s'il n'a pas de logis.
        private Vector2? DoorOf(Villager villager)
        {
            Structure home = HomeOf(villager);
            return home == null ? (Vector2?)null : _housing.DoorOf(home);
        }

        private void Deliver(VillagerHappening happening)
        {
            switch (happening.Kind)
            {
                case VillagerHappening.Kinds.Arrived:
                    VillagerArrived?.Invoke(happening.Villager);
                    break;
                case VillagerHappening.Kinds.ChimeraRefused:
                    ChimeraRefused?.Invoke(happening.RefusedKind);
                    break;
                case VillagerHappening.Kinds.WentToBed:
                    VillagerWentToBed?.Invoke(happening.Villager);
                    break;
                case VillagerHappening.Kinds.FellAsleep:
                    VillagerFellAsleep?.Invoke(happening.Villager);
                    break;
                case VillagerHappening.Kinds.WokeUp:
                    VillagerWokeUp?.Invoke(happening.Villager);
                    break;
                default:
                    SleepRefused?.Invoke(happening.SleepRefusal);
                    break;
            }
        }
    }
}
