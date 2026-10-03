using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;

namespace Diciela.Tests.EditMode.Architecture
{
    /// <summary>
    /// Seule la simulation change la simulation (ARCHITECTURE, section 5) : ce qui modifie un système est internal, et
    /// seules les couches de la simulation ouvrent leurs internals, aux seules couches de la simulation. Ni la vue, ni
    /// les gestes, ni les tests ne reçoivent de porte dérobée. Ces tests lisent le code compilé, sans rien écrire : les
    /// ouvertures (InternalsVisibleTo) se lisent sur les assemblages eux-mêmes, quelle que soit leur écriture.
    /// </summary>
    public sealed class InternalAccessTests
    {
        private const string SimulationPrefix = "Diciela.Simulation";

        /// <summary>Ce qui change l'état de la simulation ; une méthode de ce genre qui naît s'ajoute ici.</summary>
        private static readonly (Type Owner, string Method)[] Mutators =
        {
            (typeof(Clock), "Advance"),
            (typeof(NavGrid), "SetOccupant"),
            (typeof(Doorways), "Add"),
            (typeof(NavGrid), "ReleaseOccupant"),
            (typeof(NavGrid), "Reclaim"),
            (typeof(Nature), "Mark"),
            (typeof(Nature), "Unmark"),
            (typeof(Nature), "Remove"),
            (typeof(Nature), "DeliverEvents"),
            (typeof(NatureItem), "SetMarked"),
            (typeof(NatureItem), "BecomeRemoved"),
            (typeof(Structures), "Place"),
            (typeof(Structures), "Finish"),
            (typeof(Structures), "Record"),
            (typeof(Structure), "MoveTo"),
            (typeof(Structure), "StartMove"),
            (typeof(Structure), "TakeDown"),
            (typeof(Structure), "EndMove"),
            (typeof(ConstructionSites), "TrackMove"),
            (typeof(ConstructionSite), "MarkLanded"),
            (typeof(CartSpot), "MoveTo"),
            (typeof(CartSpot), "SetLine"),
            (typeof(CartSpot), "RemoveLine"),
            (typeof(Stalls), "SetLine"),
            (typeof(Stalls), "RemoveLine"),
            (typeof(Stalls), "Promise"),
            (typeof(Stalls), "Unpromise"),
            (typeof(Stalls), "DeliverEvents"),
            (typeof(StallOffers), "TryTakeStock"),
            (typeof(StallOffers), "PickUp"),
            (typeof(StallOffers), "DropOff"),
            (typeof(StallOffers), "Cancel"),
            (typeof(StallDelivery), "MarkPickedUp"),
            (typeof(StallDelivery), "MarkOver"),
            (typeof(Doorways), "Remove"),
            (typeof(Structure), "BecomeBuilt"),
            (typeof(Structures), "DeliverEvents"),
            (typeof(Villagers), "CreateChimera"),
            (typeof(Villagers), "Arrive"),
            (typeof(Housing), "Lodge"),
            (typeof(WorkSlots), "OpenFor"),
            (typeof(WorkSlots), "Close"),
            (typeof(Knowledge), "Learn"),
            (typeof(Knowledge), "DeliverEvents"),
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
            (typeof(ConstructionSites), "ForgetFinished"),
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
            (typeof(Carts), "Track"),
            (typeof(Carts), "BeginStep"),
            (typeof(Carts), "Call"),
            (typeof(Carts), "Hitch"),
            (typeof(Carts), "Pull"),
            (typeof(Carts), "Park"),
            (typeof(Carts), "SendHome"),
            (typeof(Carts), "DeliverEvents"),
            (typeof(Cart), "BeginStep"),
            (typeof(Cart), "Answer"),
            (typeof(Cart), "Hitch"),
            (typeof(Cart), "MoveTo"),
            (typeof(Cart), "Park"),
            (typeof(Cart), "ReturnHome"),
            (typeof(Drive), "MarkHitched"),
            (typeof(Drive), "MarkOver"),
            (typeof(CartOffers), "TryTakeDrive"),
            (typeof(CartOffers), "Hitch"),
            (typeof(CartOffers), "Pull"),
            (typeof(CartOffers), "Park"),
            (typeof(CartOffers), "Cancel"),
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
            var declarations = new List<string>();
            var faults = new List<string>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                if (!name.StartsWith("Diciela."))
                    continue;
                foreach (var opening in assembly.GetCustomAttributes<InternalsVisibleToAttribute>())
                {
                    // Le nom de l'assemblage ouvert, sans sa clé publique s'il en a une.
                    string target = opening.AssemblyName.Split(',')[0].Trim();
                    declarations.Add(target);
                    if (!IsSimulation(name) || !IsSimulation(target))
                        faults.Add(name + " ouvre ses internals à " + target + ".");
                }
            }

            Assert.IsNotEmpty(declarations,
                "Aucune déclaration InternalsVisibleTo trouvée : le socle et le terrain en ont une.");
            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        private static bool IsSimulation(string assembly)
        {
            return assembly == SimulationPrefix || assembly.StartsWith(SimulationPrefix + ".");
        }
    }
}
