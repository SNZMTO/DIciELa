using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Déplacer un bâtiment bâti (GDD, section 9 ; décisions 199 et 200), par l'ordre du joueur : sa nouvelle place est
    /// retenue tout de suite et il sert encore ; un ouvrier le défait (il ne sert plus), il saute à sa nouvelle place,
    /// puis on le remonte, sans nouvelle mise en service. Le banc de la ville au travail : la Chimère au poste « à tout
    /// faire » du campement, en (20 ; 14), qui a reçu 30 Bois et 20 Pierre ; une remise de 4 × 4 m dressée dès la pose.
    /// </summary>
    public sealed class MoveBuildingTests
    {
        private TestTown _town;
        private StructureDefinition _shed;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            _shed = _town.KeepKnown(TestData.Structure("remise", 4f, 4f));
            TestData.BuiltWhenPlaced(_shed);
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void ABuildingIsTakenDownMovedAndPutBackByTheChimera()
        {
            World world = _town.WorkingWorld();
            Villager chimera = world.Villagers.Chimera;
            Structure shed = TestTown.Place(world, _shed, 12f, 24f);
            var told = new List<string>();
            world.Structures.MoveStarted += structure => told.Add("déplacement");
            world.Structures.StructureMoved += structure => told.Add("saut");
            world.Structures.StructureBuilt += structure => told.Add("remontée");
            Vector2 target = new Vector2(28f, 24f);

            TestTown.Send(world, new MoveStructureCommand(shed.Id, new Placement(target, 0f)));
            Assert.AreEqual(StructureState.Built, shed.State, "Elle sert encore, tant que personne ne l'a défaite.");
            Assert.AreEqual(target, shed.MovingTo.Value.Center);
            Assert.AreEqual(shed.Id, OccupantAt(world, target), "Sa nouvelle place est retenue.");
            Assert.AreEqual(shed.Id, OccupantAt(world, new Vector2(12f, 24f)), "Elle tient encore l'ancienne.");

            TestGrids.StepUntil(world, () => shed.State == StructureState.Moving, 3000, "la remise qu'on défait");
            Assert.Less(Vector2.Distance(chimera.Position, new Vector2(12f, 24f)), 2.9f + 1f,
                "La Chimère la défait à son ancienne place.");
            TestGrids.StepUntil(world, () => shed.Placement.Center == target, 3000, "la remise défaite");
            Assert.AreEqual(0, OccupantAt(world, new Vector2(12f, 24f)), "Défaite, elle a rendu son ancienne place.");
            Assert.AreEqual(StructureState.Moving, shed.State, "Elle n'est pas encore remontée.");
            TestGrids.StepUntil(world, () => shed.State == StructureState.Built, 3000, "la remise remontée");
            Assert.IsFalse(shed.MovingTo.HasValue, "Le déplacement est fini.");
            Assert.IsNull(world.ConstructionSites.SiteOf(shed), "Son petit chantier a quitté la file.");
            CollectionAssert.AreEqual(new[] { "déplacement", "saut", "remontée" }, told);
        }

        [Test]
        public void TheMovedCampKeepsItsStockAndIsNotWelcomedAgain()
        {
            World world = _town.WorkingWorld();
            Structure camp = TestTown.CampOf(world);
            Inventory stock = world.Warehouses.StockOf(camp);
            int wood = stock.CountOf(_town.Wood);
            int stone = stock.CountOf(_town.Stone);

            TestTown.Send(world, new MoveStructureCommand(camp.Id, new Placement(new Vector2(8f, 14f), 0f)));
            TestGrids.StepUntil(world, () => camp.State == StructureState.Built && !camp.MovingTo.HasValue, 4000,
                "le campement remonté");
            Assert.AreEqual(new Vector2(8f, 14f), camp.Placement.Center);
            Assert.AreSame(stock, world.Warehouses.StockOf(camp), "Son stock a voyagé avec lui.");
            Assert.AreEqual(wood, stock.CountOf(_town.Wood), "Le stock de départ n'est pas redonné.");
            Assert.AreEqual(stone, stock.CountOf(_town.Stone));
            Assert.AreSame(camp, world.Structures.ChimeraHome, "Il reste le logis de la Chimère.");
            Assert.AreSame(camp, world.Villagers.Chimera.Slot.Workplace, "Elle garde son poste.");
        }

        [Test]
        public void ABuildingThatMovesIsNotMovedAgain()
        {
            World world = _town.WorkingWorld();
            Structure shed = TestTown.Place(world, _shed, 12f, 24f);
            var refused = new List<PlacementResult>();
            world.Structures.MoveRefused += (structure, placement, result) => refused.Add(result);

            TestTown.Send(world, new MoveStructureCommand(shed.Id, new Placement(new Vector2(28f, 24f), 0f)));
            TestTown.Send(world, new MoveStructureCommand(shed.Id, new Placement(new Vector2(28f, 32f), 0f)));
            CollectionAssert.AreEqual(new[] { PlacementResult.Moving }, refused, "On attend qu'elle soit remontée.");
            Assert.AreEqual(new Vector2(28f, 24f), shed.MovingTo.Value.Center, "Elle va toujours à la première place.");
        }

        [Test]
        public void AMovedPlaceTakesItsSpotsAndItsParkedCartAlong()
        {
            TestData.Carts(_town.Camp, 1, 0);
            StructureDefinition plan = _town.KeepKnown(TestData.Floor("place", 12f, 12f));
            TestData.Carts(plan, 0, 2);
            TestData.BuiltWhenPlaced(plan);
            World world = _town.WorkingWorld();
            Structure place = TestTown.Place(world, plan, 20f, 26f);
            Cart cart = world.Carts.All[0];
            TestGrids.StepUntil(world, () => cart.State == CartState.Parked, 3000, "la charrette garée");

            Vector2 target = new Vector2(27f, 27f);
            TestTown.Send(world, new MoveStructureCommand(place.Id, new Placement(target, 0f)));
            TestGrids.StepUntil(world, () => place.Placement.Center == target, 4000, "la Place défaite");
            foreach (CartSpot spot in world.Carts.Spots)
                Assert.AreEqual(6f * 0.55f, Vector2.Distance(spot.Position, target), 1e-3f, "Ses places l'ont suivie.");
            Assert.AreEqual(CartState.Parked, cart.State, "La charrette garée y reste garée…");
            Assert.AreEqual(cart.Spot.Position, cart.Position, "… à sa place, là où est la Place maintenant.");
            TestGrids.StepUntil(world, () => place.State == StructureState.Built, 4000, "la Place remontée");
        }

        private static int OccupantAt(World world, Vector2 point)
        {
            return world.Grid.OccupantAt(world.Grid.CellOf(point));
        }
    }
}
