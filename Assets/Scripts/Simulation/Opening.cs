using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using UnityEngine;

namespace Diciela.Simulation
{
    /// <summary>
    /// Le déroulé du départ (GDD, section 6 ; décisions 172, 187, 189 et 190) : ce qui se passe de soi-même au début
    /// d'une partie.
    /// Le premier logis de la Chimère qui tient les marchandises de la ville, le campement, reçoit le stock de départ à
    /// sa mise en service, une seule fois par partie : le bilan des marchandises le dit, rien d'autre ne le retient. Puis
    /// les premiers habitants arrivent, dans l'ordre du déroulé, un par un : chacun quand une maison bâtie a un lit
    /// libre, jamais la nuit (GDD, section 3), au moins tant d'heures après le précédent ; il arrive au point d'arrivée de
    /// la carte, fort dans son métier, et prend le poste « à tout faire » ouvert pour lui au logis de la Chimère (GDD,
    /// section 7), et le lit libre le plus proche de ce poste (GDD, section 4) ; sans logis de la Chimère, pas de poste,
    /// et le lit le plus proche de là où il arrive. Il apporte l'idée de son lieu de travail : la ville sait aussitôt le
    /// bâtir (GDD, section 13). Où en est le déroulé appartient au monde (ARCHITECTURE, section 4).
    /// Quand son lieu de travail est bâti, son poste « à tout faire » se ferme et il prend un poste de son lieu (GDD,
    /// section 7 ; décision 191). Un catalogue sans déroulé du départ, ou sans sorte d'habitant, n'en fait rien.
    /// </summary>
    internal sealed class Opening
    {
        private readonly OpeningDefinition _definition;
        private readonly VillagerKindDefinition _villagerKind;
        private readonly List<Settler> _settlers = new List<Settler>();
        private int _nextFirstVillager;
        private long _nextArrivalStep;

        public Opening(OpeningDefinition definition, VillagerKindDefinition villagerKind)
        {
            _definition = definition;
            _villagerKind = villagerKind;
        }

        /// <summary>
        /// Cette construction vient d'être mise en service (Commissioning), son stock déjà ouvert : si c'est un logis de la
        /// Chimère qui tient des marchandises, et que le stock de départ n'a pas encore été donné, elle le reçoit.
        /// </summary>
        public void Welcome(Structure built, Warehouses warehouses, GoodsLedger ledger)
        {
            if (ReferenceEquals(_definition, null) || !built.Definition.IsChimeraHome ||
                warehouses.StockOf(built) == null || ledger.CreatedBy(GoodsCause.StartingStock) > 0)
                return;
            foreach (GoodAmount goods in _definition.StartingGoods)
                warehouses.Create(built, goods.Good, goods.Amount, GoodsCause.StartingStock);
        }

        /// <summary>
        /// Le déroulé, à la fin du pas du monde, une fois mis en service ce qui vient d'être bâti : le premier habitant
        /// suivant arrive, s'il le peut. Seul le monde l'appelle.
        /// </summary>
        public void Step(World world)
        {
            if (ReferenceEquals(_definition, null) || ReferenceEquals(_villagerKind, null))
                return;
            IReadOnlyList<FirstVillagerDefinition> firsts = _definition.FirstVillagers;
            if (_nextFirstVillager >= firsts.Count || world.Clock.Phase == DayPhase.Night ||
                world.Clock.StepCount < _nextArrivalStep)
                return;
            Vector2 at = world.Map.ArrivalPoint;
            Structure post = ReferenceEquals(_definition.FirstVillagersSlot, null) ? null : world.Structures.ChimeraHome;
            Structure house = world.Housing.NearestWithAFreeBed(post == null ? at : post.Placement.Center);
            if (house == null)
                return;

            FirstVillagerDefinition first = firsts[_nextFirstVillager++];
            Villager villager = world.Villagers.Arrive(_villagerKind, new Appearance(first.Character), first.NameKey, at,
                new Strength(first.Strength, _definition.FirstVillagerStrength));
            world.Housing.Lodge(villager, house);
            if (post != null)
                world.WorkSlots.OpenFor(villager, post, _definition.FirstVillagersSlot);
            if (!ReferenceEquals(first.Workplace, null))
                world.Knowledge.Learn(first.Workplace);
            _settlers.Add(new Settler { Workplace = first.Workplace, Villager = villager, Opened = villager.Slot });
            _nextArrivalStep = world.Clock.StepCount + world.Clock.StepsIn(_definition.FirstArrivalHours);
        }

        /// <summary>
        /// Cette construction vient d'être mise en service, ses postes ouverts : si c'est le lieu de travail d'un premier
        /// habitant dont le poste « à tout faire » est encore ouvert, ce poste se ferme ; s'il le tenait, il prend un poste
        /// libre de son lieu (sans poste libre, il redevient libre) ; si le joueur l'a mis ailleurs, il y reste ; si un
        /// autre tient ce poste, il reste ouvert. Seule la mise en service l'appelle (Commissioning).
        /// </summary>
        public void Settle(Structure built, World world)
        {
            for (int i = 0; i < _settlers.Count; i++)
            {
                Settler settler = _settlers[i];
                if (settler.Opened == null || !ReferenceEquals(settler.Workplace, built.Definition))
                    continue;
                if (ReferenceEquals(settler.Villager.Slot, settler.Opened))
                {
                    if (world.WorkSlots.HasFreeSlotAt(built))
                        world.WorkSlots.Assign(settler.Villager, built);
                    else
                        world.WorkSlots.Unassign(settler.Villager);
                }
                if (settler.Opened.Holder == null)
                    world.WorkSlots.Close(settler.Opened);
                settler.Opened = null;
            }
        }

        /// <summary>Un premier habitant arrivé, son lieu de travail et le poste ouvert pour lui (null : aucun).</summary>
        private sealed class Settler
        {
            public StructureDefinition Workplace;
            public Villager Villager;
            public WorkSlot Opened;
        }
    }
}
