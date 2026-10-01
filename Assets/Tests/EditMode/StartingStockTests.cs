using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le stock de départ (GDD, section 6 ; décision 172), par l'ordre « poser » le campement : il le reçoit en
    /// s'installant, 30 Bois et 20 Pierre nés du départ, qui sont le stock de la ville, et ouvre le poste « à tout
    /// faire », libre ; rien d'autre ne se pose. Le stock de départ ne se donne qu'une fois ; un logis qui ne tient rien
    /// ne reçoit rien ; sans déroulé du départ, le campement reste vide.
    /// </summary>
    public sealed class StartingStockTests
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
        public void InstallingTheCampFillsItAndOpensItsPost()
        {
            World world = _town.World();
            TestTown.Place(world, _town.Camp, 20f, 14f);
            Structure camp = TestTown.CampOf(world);

            Assert.AreEqual(1, world.Structures.All.Count, "Le campement seul : plus de tas à côté.");
            Assert.AreEqual(30, world.Warehouses.StockOf(camp).CountOf(_town.Wood));
            Assert.AreEqual(20, world.Warehouses.StockOf(camp).CountOf(_town.Stone));
            Assert.AreEqual(30, world.Warehouses.CityStockOf(_town.Wood), "Le stock de la ville, c'est le campement.");
            Assert.AreEqual(50, world.Ledger.CreatedBy(GoodsCause.StartingStock), "Le stock est né du départ.");
            Assert.AreEqual(1, world.WorkSlots.All.Count);
            WorkSlot slot = world.WorkSlots.All[0];
            Assert.AreSame(camp, slot.Workplace);
            Assert.AreSame(_town.OddJobs, slot.Definition);
            Assert.IsNull(slot.Holder, "La Chimère ne prend jamais de poste d'elle-même.");
        }

        [Test]
        public void TheStartingStockIsGivenOnce()
        {
            World world = _town.World();
            TestTown.Place(world, _town.Camp, 20f, 14f);
            StructureDefinition house = _town.Keep(TestData.Home("maison", 3f, true, 100));
            Structure second = TestTown.Place(world, house, 30f, 14f);

            Assert.IsNotNull(second);
            Assert.AreEqual(0, world.Warehouses.StockOf(second).Total, "Le stock de départ est déjà donné.");
            Assert.AreEqual(50, world.Ledger.CreatedBy(GoodsCause.StartingStock));
        }

        [Test]
        public void AHomeThatHoldsNothingGetsNothing()
        {
            World world = _town.World();
            TestTown.Place(world, _town.Keep(TestData.Home("tente_vide")), 20f, 14f);

            Assert.AreEqual(0, world.Warehouses.Count);
            Assert.AreEqual(0, world.Ledger.CreatedBy(GoodsCause.StartingStock));
        }

        [Test]
        public void WithoutAnOpeningTheCampStaysEmpty()
        {
            World world = _town.World(TestData.EmptyCatalog);
            TestTown.Place(world, _town.Camp, 20f, 14f);

            Assert.AreEqual(1, world.Warehouses.Count, "Le campement tient toujours les marchandises de la ville.");
            Assert.AreEqual(0, world.Warehouses.StockOf(TestTown.CampOf(world)).Total);
        }
    }
}
