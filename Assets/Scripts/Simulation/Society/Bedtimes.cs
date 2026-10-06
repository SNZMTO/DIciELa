using System;
using System.Collections.Generic;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Aller dormir et se lever (GDD, sections 3 et 6). La Chimère, sur ordre du joueur, dans son logis. Les habitants
    /// dont la sorte rentre dormir, d'eux-mêmes : au début de la nuit, chacun marche jusqu'à sa porte et entre ; au matin,
    /// il se lève devant elle. Celui qui ne peut pas rejoindre sa porte ne part pas : il flâne la nuit. Ce qui arrive
    /// attend la fin de la couche, dans la file des habitants.
    /// </summary>
    internal sealed class Bedtimes
    {
        private readonly NavGrid _grid;
        private readonly List<VillagerHappening> _happened;
        private readonly Func<Villager, Vector2?> _doorOf;
        private DayPhase _phase;

        /// <summary>
        /// Le coucher et le lever de ces habitants, sur ce maillage, à partir de cette phase de la journée ; doorOf dit le
        /// point devant la porte de chez soi (null : pas de logis). Ce qui arrive va dans cette file.
        /// </summary>
        public Bedtimes(NavGrid grid, DayPhase phase, List<VillagerHappening> happened, Func<Villager, Vector2?> doorOf)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _happened = happened ?? throw new ArgumentNullException(nameof(happened));
            _doorOf = doorOf ?? throw new ArgumentNullException(nameof(doorOf));
            _phase = phase;
        }

        /// <summary>
        /// Envoie la Chimère dormir : elle marche jusqu'au point devant la porte de son logis. Refusé si elle n'existe pas,
        /// n'a pas de logis, dort déjà ou y va, ou ne peut pas rejoindre sa porte ; rien ne change alors, et les habitants
        /// le diront (SleepRefused). Seul l'ordre « dormir » l'appelle.
        /// </summary>
        public void SendChimeraToBed(Villager chimera)
        {
            if (chimera == null)
            {
                Refuse(SleepRefusal.NoChimera);
                return;
            }
            Vector2? door = _doorOf(chimera);
            if (!door.HasValue)
                Refuse(SleepRefusal.NoHome);
            else if (chimera.IsAsleep || chimera.IsGoingToBed)
                Refuse(SleepRefusal.AlreadyInBed);
            else if (!chimera.TryGoToBed(_grid.CellOf(door.Value)))
                Refuse(SleepRefusal.Unreachable);
            else
                _happened.Add(VillagerHappening.Of(VillagerHappening.Kinds.WentToBed, chimera));
        }

        /// <summary>
        /// Réveille la Chimère, ou l'arrête si elle allait dormir. Refusé si elle n'existe pas ou qu'elle est debout ; rien
        /// ne change alors. Seul l'ordre « réveiller » l'appelle.
        /// </summary>
        public void WakeChimera(Villager chimera)
        {
            if (chimera == null)
                Refuse(SleepRefusal.NoChimera);
            else if (!chimera.IsAsleep && !chimera.IsGoingToBed)
                Refuse(SleepRefusal.NotAsleep);
            else
            {
                chimera.WakeUp();
                _happened.Add(VillagerHappening.Of(VillagerHappening.Kinds.WokeUp, chimera));
            }
        }

        /// <summary>
        /// Au passage à la nuit, envoie dormir ceux qui rentrent chez eux, chacun vers sa porte (sans logis, il flâne) ;
        /// au passage au matin, les lève. Appelé à chaque pas, avant que les habitants jouent le leur.
        /// </summary>
        public void Step(IReadOnlyList<Villager> all, DayPhase phase)
        {
            if (phase == _phase)
                return;
            _phase = phase;
            if (phase != DayPhase.Night && phase != DayPhase.Work)
                return;
            for (int i = 0; i < all.Count; i++)
            {
                Villager villager = all[i];
                if (!villager.Kind.SleepsAtNight)
                    continue;
                if (phase == DayPhase.Night)
                    GoHome(villager, _doorOf(villager));
                else if (villager.IsAsleep || villager.IsGoingToBed)
                {
                    villager.WakeUp();
                    _happened.Add(VillagerHappening.Of(VillagerHappening.Kinds.WokeUp, villager));
                }
            }
        }

        private void GoHome(Villager villager, Vector2? door)
        {
            if (!door.HasValue || villager.IsAsleep || villager.IsGoingToBed)
                return;
            if (villager.TryGoToBed(_grid.CellOf(door.Value)))
                _happened.Add(VillagerHappening.Of(VillagerHappening.Kinds.WentToBed, villager));
        }

        private void Refuse(SleepRefusal reason)
        {
            _happened.Add(VillagerHappening.SleepRefusedFor(reason));
        }
    }
}
