using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Le journal : il écrit dans la console les événements clés du monde, pour qu'un Play court prouve que la
    /// simulation travaille (ARCHITECTURE, section 12). Il sert au développeur ; le joueur ne le voit jamais.
    /// </summary>
    public sealed class SimulationJournal
    {
        private const string Prefix = "DICIELA : ";

        private readonly World _world;
        private readonly NatureJournal _nature;

        public SimulationJournal(World world)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));

            _world = world;
            _world.Clock.DayStarted += OnDayStarted;
            _world.Clock.PhaseChanged += OnPhaseChanged;
            _world.Structures.StructurePlaced += OnStructurePlaced;
            _world.Structures.PlacementRefused += OnPlacementRefused;
            _world.Structures.StructureBuilt += OnStructureBuilt;
            _world.ConstructionSites.GoodsReceived += OnGoodsReceived;
            _world.WorkSlots.VillagerAssigned += OnVillagerAssigned;
            _world.WorkSlots.VillagerUnassigned += OnVillagerUnassigned;
            _world.WorkSlots.AssignmentRefused += OnAssignmentRefused;
            _world.Villagers.VillagerArrived += OnVillagerArrived;
            _world.Villagers.ChimeraRefused += OnChimeraRefused;
            _world.Villagers.VillagerWentToBed += OnVillagerWentToBed;
            _world.Villagers.VillagerFellAsleep += OnVillagerFellAsleep;
            _world.Villagers.VillagerWokeUp += OnVillagerWokeUp;
            _world.Villagers.SleepRefused += OnSleepRefused;
            _nature = new NatureJournal(world.Nature);
            Relief relief = _world.Map.Relief;
            Debug.Log(Prefix + "monde créé, carte de " + relief.SizeX.ToString("0") + " × " + relief.SizeZ.ToString("0") +
                      " m (relief de " + relief.SamplesX + " × " + relief.SamplesZ + " points), " +
                      Describe(_world.Clock) + ".");
        }

        /// <summary>Se désabonne du monde et écrit où il en était, et où sont ses habitants.</summary>
        public void Stop()
        {
            _world.Clock.DayStarted -= OnDayStarted;
            _world.Clock.PhaseChanged -= OnPhaseChanged;
            _world.Structures.StructurePlaced -= OnStructurePlaced;
            _world.Structures.PlacementRefused -= OnPlacementRefused;
            _world.Structures.StructureBuilt -= OnStructureBuilt;
            _world.ConstructionSites.GoodsReceived -= OnGoodsReceived;
            _world.WorkSlots.VillagerAssigned -= OnVillagerAssigned;
            _world.WorkSlots.VillagerUnassigned -= OnVillagerUnassigned;
            _world.WorkSlots.AssignmentRefused -= OnAssignmentRefused;
            _world.Villagers.VillagerArrived -= OnVillagerArrived;
            _world.Villagers.ChimeraRefused -= OnChimeraRefused;
            _world.Villagers.VillagerWentToBed -= OnVillagerWentToBed;
            _world.Villagers.VillagerFellAsleep -= OnVillagerFellAsleep;
            _world.Villagers.VillagerWokeUp -= OnVillagerWokeUp;
            _world.Villagers.SleepRefused -= OnSleepRefused;
            _nature.Stop();
            foreach (Villager villager in _world.Villagers.All)
                Debug.Log(Prefix + Describe(villager) + " est en " + villager.Position + ".");
            Debug.Log(Prefix + "monde arrêté après " + _world.Clock.StepCount + " pas, " + Describe(_world.Clock) + ".");
        }

        private static void OnVillagerArrived(Villager villager)
        {
            Debug.Log(Prefix + Describe(villager) + " arrive en " + villager.Position + ".");
        }

        private static void OnChimeraRefused(VillagerKindDefinition kind)
        {
            Debug.Log(Prefix + "création de la Chimère refusée (" + kind.Id + ") : elle existe déjà.");
        }

        private static void OnVillagerWentToBed(Villager villager)
        {
            Debug.Log(Prefix + Describe(villager) + " va dormir, depuis " + villager.Position + ".");
        }

        private static void OnVillagerFellAsleep(Villager villager)
        {
            Debug.Log(Prefix + Describe(villager) + " s'endort en " + villager.Position + ".");
        }

        private static void OnVillagerWokeUp(Villager villager)
        {
            Debug.Log(Prefix + Describe(villager) + " est debout, en " + villager.Position + ".");
        }

        private static void OnSleepRefused(SleepRefusal reason)
        {
            Debug.Log(Prefix + "ordre « dormir » ou « réveiller » refusé : " + reason + ".");
        }

        private static string Describe(Villager villager)
        {
            return "l'habitant n° " + villager.Id + " (" + villager.Kind.Id + ")";
        }

        private static void OnDayStarted(int day)
        {
            Debug.Log(Prefix + "jour " + day + ".");
        }

        private static void OnPhaseChanged(DayPhase phase)
        {
            Debug.Log(Prefix + PhaseName(phase) + ".");
        }

        private static void OnStructurePlaced(Structure structure)
        {
            Debug.Log(Prefix + "construction n° " + structure.Id + " (" + structure.Definition.Id + ") posée " +
                      Describe(structure.Placement) + ".");
        }

        private static void OnPlacementRefused(StructureDefinition definition, Placement placement,
            PlacementResult reason)
        {
            Debug.Log(Prefix + "pose refusée (" + definition.Id + " " + Describe(placement) + ") : " + reason + ".");
        }

        private static void OnStructureBuilt(Structure structure)
        {
            Debug.Log(Prefix + "construction n° " + structure.Id + " (" + structure.Definition.Id + ") bâtie.");
        }

        private static void OnGoodsReceived(ConstructionSite site)
        {
            string goods = "";
            for (int line = 0; line < site.LineCount; line++)
                goods += (line > 0 ? ", " : "") + site.GoodAt(line).Id + " " + site.DeliveredAt(line) + "/" +
                         site.NeededAt(line);
            Debug.Log(Prefix + "livraison au chantier n° " + site.Structure.Id + " : " + goods + ".");
        }

        private static void OnVillagerAssigned(Villager villager, WorkSlot slot)
        {
            Debug.Log(Prefix + Describe(villager) + " prend le poste « " + slot.Definition.Id + " » de la construction n° " +
                      slot.Workplace.Id + ".");
        }

        private static void OnVillagerUnassigned(Villager villager, WorkSlot slot)
        {
            Debug.Log(Prefix + Describe(villager) + " quitte le poste « " + slot.Definition.Id + " » de la construction n° " +
                      slot.Workplace.Id + ".");
        }

        private static void OnAssignmentRefused(AssignmentRefusal reason)
        {
            Debug.Log(Prefix + "ordre « affecter » ou « retirer » refusé : " + reason + ".");
        }

        private static string Describe(Placement placement)
        {
            return "en " + placement.Center + ", à " + placement.RotationDegrees + "°";
        }

        private static string Describe(Clock clock)
        {
            double hour = clock.HourOfDay;
            int hours = (int)hour;
            int minutes = (int)((hour - hours) * 60.0);
            return "jour " + clock.Day + ", " + hours.ToString("00") + " h " + minutes.ToString("00") +
                   " depuis le matin, " + PhaseName(clock.Phase);
        }

        private static string PhaseName(DayPhase phase)
        {
            switch (phase)
            {
                case DayPhase.Work:
                    return "travail";
                case DayPhase.Evening:
                    return "soirée";
                default:
                    return "nuit";
            }
        }
    }
}
