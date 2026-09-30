using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode.Architecture
{
    /// <summary>
    /// Seule la simulation change la simulation (ARCHITECTURE, section 5) : ce qui modifie un système est internal, et
    /// seules les couches de la simulation ouvrent leurs internals, aux seules couches de la simulation. Ni la vue, ni
    /// les gestes, ni les tests ne reçoivent de porte dérobée. Ces tests lisent le code, sans rien écrire.
    /// </summary>
    public sealed class InternalAccessTests
    {
        private const string SimulationPrefix = "Diciela.Simulation";

        private static readonly Regex Declaration = new Regex(
            @"\[\s*assembly\s*:\s*(?:System\.Runtime\.CompilerServices\.)?InternalsVisibleTo\s*\(\s*""([^""]+)""");

        /// <summary>Ce qui change l'état de la simulation ; une méthode de ce genre qui naît s'ajoute ici.</summary>
        private static readonly (Type Owner, string Method)[] Mutators =
        {
            (typeof(Clock), "Advance"),
            (typeof(NavGrid), "SetOccupant"),
            (typeof(NavGrid), "AddDoorway"),
            (typeof(NavGrid), "ReleaseOccupant"),
            (typeof(Nature), "Mark"),
            (typeof(Nature), "Unmark"),
            (typeof(Nature), "Remove"),
            (typeof(Nature), "DeliverEvents"),
            (typeof(NatureItem), "SetMarked"),
            (typeof(NatureItem), "BecomeRemoved"),
            (typeof(Structures), "Place"),
            (typeof(Structures), "Finish"),
            (typeof(Structure), "BecomeBuilt"),
            (typeof(Structures), "DeliverEvents"),
            (typeof(Villagers), "CreateChimera"),
            (typeof(Villagers), "SendChimeraToBed"),
            (typeof(Villagers), "WakeChimera"),
            (typeof(Villagers), "Step"),
            (typeof(Villagers), "DeliverEvents"),
            (typeof(Villager), "Step"),
            (typeof(Villager), "TryGoToBed"),
            (typeof(Villager), "WakeUp"),
            (typeof(Command), "Execute"),
            (typeof(Inventory), "Add"),
            (typeof(Inventory), "Remove"),
            (typeof(Inventory), "Reserve"),
            (typeof(Inventory), "Release"),
            (typeof(Inventory), "ReserveSpace"),
            (typeof(Inventory), "ReleaseSpace"),
            (typeof(GoodsLedger), "Created"),
            (typeof(GoodsLedger), "Destroyed"),
            (typeof(Warehouses), "Track"),
            (typeof(Warehouses), "Create"),
            (typeof(Warehouses), "Reserve"),
            (typeof(Warehouses), "Release"),
            (typeof(Warehouses), "Take"),
            (typeof(Warehouses), "ReturnToTown"),
            (typeof(Warehouses), "ReserveSpace"),
            (typeof(Warehouses), "ReleaseSpace"),
            (typeof(Warehouses), "Store"),
            (typeof(Warehouses), "DeliverEvents"),
            (typeof(ConstructionSite), "Promise"),
            (typeof(ConstructionSite), "Unpromise"),
            (typeof(ConstructionSite), "Receive"),
            (typeof(ConstructionSite), "AddBuilder"),
            (typeof(ConstructionSite), "RemoveBuilder"),
            (typeof(ConstructionSite), "AddWork"),
            (typeof(ConstructionSite), "MarkFinished"),
            (typeof(ConstructionSites), "Track"),
            (typeof(ConstructionSites), "Receive"),
            (typeof(ConstructionSites), "Work"),
            (typeof(ConstructionSites), "DeliverEvents"),
            (typeof(Delivery), "MarkPickedUp"),
            (typeof(Delivery), "MarkOver"),
            (typeof(BuildShift), "MarkOver"),
            (typeof(TaskOffers), "TryTakeDelivery"),
            (typeof(TaskOffers), "TryTakeBuilding"),
            (typeof(TaskOffers), "PickUp"),
            (typeof(TaskOffers), "DropOff"),
            (typeof(TaskOffers), "Cancel"),
            (typeof(TaskOffers), "Work"),
            (typeof(TaskOffers), "Leave"),
            (typeof(Clearing), "MarkFelled"),
            (typeof(Clearing), "MarkOver"),
            (typeof(ClearingOffers), "TryTakeClearing"),
            (typeof(ClearingOffers), "Work"),
            (typeof(ClearingOffers), "Store"),
            (typeof(ClearingOffers), "Cancel"),
            (typeof(WorkSlot), "SetHolder"),
            (typeof(WorkSlots), "Track"),
            (typeof(WorkSlots), "Assign"),
            (typeof(WorkSlots), "Unassign"),
            (typeof(WorkSlots), "DeliverEvents"),
            (typeof(Villager), "TakeSlot"),
            (typeof(Villager), "LeaveSlot"),
        };

        [Test]
        public void WhatChangesTheSimulationStaysInternal()
        {
            var faults = new List<string>();
            foreach ((Type owner, string name) in Mutators)
            {
                // Toutes les formes d'une fonction : une seconde forme publique ouvrirait la même porte.
                int found = 0;
                foreach (MethodInfo method in owner.GetMethods(
                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (method.Name != name)
                        continue;
                    found++;
                    if (!method.IsAssembly)
                        faults.Add(owner.Name + "." + name + " doit rester internal : au-dessus, on envoie des ordres.");
                }
                if (found == 0)
                    faults.Add(owner.Name + "." + name + " est introuvable.");
            }

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        [Test]
        public void OnlyTheSimulationOpensItsInternalsAndOnlyToItself()
        {
            string scriptsFolder = Normalized(System.IO.Path.Combine(Application.dataPath, "Scripts"));
            string simulationFolder = scriptsFolder + "/Simulation/";
            var declarations = new List<string>();
            var faults = new List<string>();

            foreach (string file in Directory.GetFiles(scriptsFolder, "*.cs", SearchOption.AllDirectories))
            {
                string path = Normalized(file);
                foreach (Match match in Declaration.Matches(File.ReadAllText(file)))
                {
                    string target = match.Groups[1].Value;
                    declarations.Add(target);
                    if (!path.StartsWith(simulationFolder) || !target.StartsWith(SimulationPrefix))
                        faults.Add(path.Substring(scriptsFolder.Length + 1) + " ouvre ses internals à " + target + ".");
                }
            }

            Assert.IsNotEmpty(declarations,
                "Aucune déclaration InternalsVisibleTo trouvée : le socle et le terrain en ont une.");
            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        private static string Normalized(string path)
        {
            return System.IO.Path.GetFullPath(path).Replace('\\', '/');
        }
    }
}
