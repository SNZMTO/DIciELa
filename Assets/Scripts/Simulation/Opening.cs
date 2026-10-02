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
    /// Le déroulé du départ (GDD, section 6 ; décisions 172 et 187) : ce qui se passe de soi-même au début d'une partie.
    /// Le premier logis de la Chimère qui tient les marchandises de la ville, le campement, reçoit le stock de départ à
    /// sa mise en service, une seule fois par partie : le bilan des marchandises le dit, rien d'autre ne le retient. Puis
    /// les premiers habitants arrivent, dans l'ordre du déroulé, un par un : chacun quand une maison bâtie a un lit
    /// libre, jamais la nuit (GDD, section 3), au moins tant d'heures après le précédent ; il arrive au point d'arrivée de
    /// la carte, fort dans son métier, et reçoit le lit libre le plus proche de là. Où en est le déroulé appartient au
    /// monde (ARCHITECTURE, section 4). Un catalogue sans déroulé du départ, ou sans sorte d'habitant, n'en fait rien.
    /// </summary>
    internal sealed class Opening
    {
        private readonly OpeningDefinition _definition;
        private readonly VillagerKindDefinition _villagerKind;
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
            Structure house = world.Housing.NearestWithAFreeBed(at);
            if (house == null)
                return;

            FirstVillagerDefinition first = firsts[_nextFirstVillager++];
            Villager villager = world.Villagers.Arrive(_villagerKind, new Appearance(first.Character), at,
                new Strength(first.Strength, _definition.FirstVillagerStrength));
            world.Housing.Lodge(villager, house);
            _nextArrivalStep = world.Clock.StepCount + world.Clock.StepsIn(_definition.FirstArrivalHours);
        }
    }
}
