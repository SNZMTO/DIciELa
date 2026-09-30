using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le tas de départ (GDD, section 6 ; décisions 137 et 147), par l'ordre « poser » la tente : il se pose de lui-même,
    /// à droite de la porte vue de face, contre la façade ; sinon au plus près ; il tient 30 Bois et 20 Pierre pour la
    /// ville, nés du départ, et ouvre le poste « à tout faire ». Sans tas dans le catalogue, rien ne se pose. Tente de
    /// 3 × 3 m, porte de 1 m au milieu de sa façade nord ; tas de 2 × 2 m.
    /// </summary>
    public sealed class StartingPileTests
    {
        private TestTown _town;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void PlantingTheTentLaysThePileRightOfItsDoorSeenFromTheFront()
        {
            World world = _town.World();
            TestTown.Place(world, _town.Tent, 20f, 14f);

            Structure pile = _town.PileOf(world);
            Assert.IsNotNull(pile, "Le tas se pose avec la tente.");
            // La porte, au nord, va de x = 19,5 à 20,5 ; vue de face (depuis le nord), sa droite est à l'ouest. Le tas
            // longe le devant de la porte, contre la façade (z = 15,5).
            Assert.AreEqual(new Vector2(18.5f, 16.5f), pile.Placement.Center);
            Assert.AreEqual(0f, pile.Placement.RotationDegrees);
            Assert.AreEqual(StructureState.Built, pile.State);
        }

        [Test]
        public void TheTurnedTentTurnsItsPileWithIt()
        {
            World world = _town.World();
            TestTown.Place(world, _town.Tent, 20f, 14f, 90f);

            Structure pile = _town.PileOf(world);
            Assert.IsNotNull(pile);
            // Le même tas, dans le repère de la tente tournée : la place est celle des règles de pose.
            Vector2 local = new Vector2(-1.5f, 2.5f);
            Vector2 expected = PlacementRules.Footprint(_town.Tent, world.Structures.All[0].Placement).ToWorld(local);
            Assert.Less(Vector2.Distance(expected, pile.Placement.Center), 0.001f);
            Assert.AreEqual(90f, pile.Placement.RotationDegrees);
        }

        [Test]
        public void ATakenSpotSendsThePileToTheNearestFreeOne()
        {
            World world = _town.World();
            StructureDefinition block = _town.Keep(TestData.Structure("bloc", 2f, 2f, hasDoor: false));
            TestTown.Place(world, block, 18.5f, 16.5f);
            TestTown.Place(world, _town.Tent, 20f, 14f);

            Structure pile = _town.PileOf(world);
            Assert.IsNotNull(pile, "Le tas trouve une autre place.");
            float away = Vector2.Distance(pile.Placement.Center, new Vector2(18.5f, 16.5f));
            Assert.Greater(away, 1.9f, "Il ne chevauche pas le bloc.");
            Assert.Less(away, 3f, "Il reste au plus près.");
        }

        [Test]
        public void ThePileHoldsItsStartingGoodsAndOpensItsPost()
        {
            World world = _town.World();
            TestTown.Place(world, _town.Tent, 20f, 14f);
            Structure pile = _town.PileOf(world);

            Assert.AreEqual(30, world.Warehouses.StockOf(pile).CountOf(_town.Wood));
            Assert.AreEqual(20, world.Warehouses.StockOf(pile).CountOf(_town.Stone));
            Assert.AreEqual(30, world.Warehouses.CityStockOf(_town.Wood), "Le stock de la ville, c'est le tas.");
            Assert.AreEqual(30, world.Ledger.CreatedOf(_town.Wood), "Le Bois est né du départ.");
            Assert.AreEqual(1, world.WorkSlots.All.Count);
            WorkSlot slot = world.WorkSlots.All[0];
            Assert.AreSame(pile, slot.Workplace);
            Assert.AreSame(_town.OddJobs, slot.Definition);
            Assert.IsNull(slot.Holder, "La Chimère ne prend jamais de poste d'elle-même.");
        }

        [Test]
        public void WithoutAStartingPileNothingIsLaid()
        {
            World world = _town.World(TestData.EmptyCatalog);
            TestTown.Place(world, _town.Tent, 20f, 14f);

            Assert.AreEqual(1, world.Structures.All.Count, "La tente seule.");
            Assert.AreEqual(0, world.Warehouses.Count);
        }
    }
}
