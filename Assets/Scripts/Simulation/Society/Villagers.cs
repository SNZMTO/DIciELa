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
    /// Les habitants de la ville (ARCHITECTURE, section 4) : la vérité de qui vit ici et où. Pour l'instant, la Chimère,
    /// que le joueur crée au lancement d'une partie, et qu'il envoie dormir dans son logis ou réveille ; les arrivées
    /// viendront avec les logements. Chacun joue son pas dans l'ordre de son arrivée. Les événements d'un pas attendent
    /// la fin de leur couche, dans une seule file (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class Villagers
    {
        private readonly List<Villager> _all = new List<Villager>();
        private readonly List<VillagerHappening> _happened = new List<VillagerHappening>();
        private readonly VillagerServices _services;
        private readonly NavGrid _grid;
        private int _nextId = 1;

        /// <summary>
        /// Les habitants de ce monde : ils vivent sur ce maillage, à cette horloge, prennent leurs tâches ici, tiennent
        /// les postes de cette table, et leurs statistiques suivent celles-ci.
        /// </summary>
        public Villagers(NavGrid grid, Islands islands, PathFinder finder, Clock clock, Surroundings surroundings,
            TaskOffers offers, ClearingOffers clearings, WorkSlots slots, StatisticTable statistics)
        {
            _services = new VillagerServices(clock, grid, islands, finder, surroundings, offers, clearings, slots,
                statistics);
            _grid = grid;
        }

        /// <summary>Levé quand un habitant arrive en ville : pour l'instant, la Chimère, à sa création.</summary>
        public event Action<Villager> VillagerArrived;

        /// <summary>Levé quand un ordre « créer la Chimère » est refusé, avec la sorte refusée : elle existe déjà.</summary>
        public event Action<VillagerKindDefinition> ChimeraRefused;

        /// <summary>Levé quand un habitant part dormir : il marche vers son lit.</summary>
        public event Action<Villager> VillagerWentToBed;

        /// <summary>Levé quand un habitant arrive à son lit et s'endort.</summary>
        public event Action<Villager> VillagerFellAsleep;

        /// <summary>Levé quand un habitant se réveille, ou cesse d'aller dormir : il se remet à flâner.</summary>
        public event Action<Villager> VillagerWokeUp;

        /// <summary>Levé quand un ordre « dormir » ou « réveiller » est refusé, avec la raison.</summary>
        public event Action<SleepRefusal> SleepRefused;

        public IReadOnlyList<Villager> All => _all;

        /// <summary>La Chimère, une fois créée ; null avant.</summary>
        public Villager Chimera { get; private set; }

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
        /// Crée la Chimère au point d'arrivée, sur le milieu de sa maille, avec cette apparence. Une seule par monde : si
        /// elle existe déjà, faux, rien ne change, et <see cref="ChimeraRefused"/> le dira. Seul l'ordre « créer la
        /// Chimère » l'appelle.
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

            Vector2Int cell = _grid.CellOf(arrivalPoint);
            cell = new Vector2Int(Math.Clamp(cell.x, 0, _grid.Width - 1), Math.Clamp(cell.y, 0, _grid.Depth - 1));
            var chimera = new Villager(_nextId++, kind, appearance, _grid.CellCenter(cell), _services);
            _all.Add(chimera);
            Chimera = chimera;
            _happened.Add(VillagerHappening.Of(VillagerHappening.Kinds.Arrived, chimera));
            return true;
        }

        /// <summary>
        /// Envoie la Chimère dormir : elle marche jusqu'à ce point, devant la porte de son logis (null : elle n'a pas de
        /// logis). Refusé si elle n'existe pas, dort déjà ou y va, ou ne peut pas rejoindre ce point ; rien ne change
        /// alors, et <see cref="SleepRefused"/> le dira. Seul l'ordre « dormir » l'appelle.
        /// </summary>
        internal void SendChimeraToBed(Vector2? bed)
        {
            if (Chimera == null)
                Refuse(SleepRefusal.NoChimera);
            else if (!bed.HasValue)
                Refuse(SleepRefusal.NoHome);
            else if (Chimera.IsAsleep || Chimera.IsGoingToBed)
                Refuse(SleepRefusal.AlreadyInBed);
            else if (!Chimera.TryGoToBed(_grid.CellOf(bed.Value)))
                Refuse(SleepRefusal.Unreachable);
            else
                _happened.Add(VillagerHappening.Of(VillagerHappening.Kinds.WentToBed, Chimera));
        }

        /// <summary>
        /// Réveille la Chimère, ou l'arrête si elle allait dormir. Refusé si elle n'existe pas ou qu'elle est debout ; rien
        /// ne change alors, et <see cref="SleepRefused"/> le dira. Seul l'ordre « réveiller » l'appelle.
        /// </summary>
        internal void WakeChimera()
        {
            if (Chimera == null)
                Refuse(SleepRefusal.NoChimera);
            else if (!Chimera.IsAsleep && !Chimera.IsGoingToBed)
                Refuse(SleepRefusal.NotAsleep);
            else
            {
                Chimera.WakeUp();
                _happened.Add(VillagerHappening.Of(VillagerHappening.Kinds.WokeUp, Chimera));
            }
        }

        /// <summary>Joue un pas pour chaque habitant, dans l'ordre de leur arrivée. Seul le monde l'appelle.</summary>
        internal void Step()
        {
            for (int i = 0; i < _all.Count; i++)
            {
                Villager villager = _all[i];
                bool wasAsleep = villager.IsAsleep;
                villager.Step();
                if (!wasAsleep && villager.IsAsleep)
                    _happened.Add(VillagerHappening.Of(VillagerHappening.Kinds.FellAsleep, villager));
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

        private void Refuse(SleepRefusal reason)
        {
            _happened.Add(VillagerHappening.SleepRefusedFor(reason));
        }
    }
}
