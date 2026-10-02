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
    /// ne reçoit rien ; sans déroulé du départ, le campement reste vide. Un logis qui sort de son chantier est mis en
    /// service de la même façon, dès qu'on l'entend bâti : son stock, son poste, le stock de départ (décision 179).
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
            StructureDefinition house = _town.KeepKnown(TestData.Home("maison", 3f, true, 100));
            Structure second = TestTown.Place(world, house, 30f, 14f);

            Assert.IsNotNull(second);
            Assert.AreEqual(0, world.Warehouses.StockOf(second).Total, "Le stock de départ est déjà donné.");
            Assert.AreEqual(50, world.Ledger.CreatedBy(GoodsCause.StartingStock));
        }

        [Test]
        public void AHomeThatComesOutOfItsSiteIsCommissionedLikeTheCamp()
        {
            // Un abri qui ne tient rien ouvre le poste ; la maison, qui tient des marchandises, attend ses bâtisseurs.
            World world = _town.World();
            Structure shelter = TestTown.Place(world, _town.KeepKnown(TestData.Home("abri", 3f, true, 0, _town.OddJobs)),
                20f, 14f);
            TestTown.Send(world, new AssignVillagerCommand(world.Villagers.Chimera.Id, shelter.Id));
            StructureDefinition plan = _town.KeepKnown(TestData.Home("maison", 3f, false, 100, _town.OddJobs));
            TestData.Cost(plan, 0.5f);
            Structure house = TestTown.Place(world, plan, 28f, 14f);
            Assert.AreEqual(StructureState.Planned, house.State);
            int stockWhenHeard = -1;
            world.Structures.StructureBuilt += built =>
            {
                Inventory stock = world.Warehouses.StockOf(built);
                stockWhenHeard = stock == null ? -2 : stock.Total;
            };

            TestGrids.StepUntil(world, () => house.State == StructureState.Built, 3000, "la maison bâtie");

            Assert.AreEqual(30, world.Warehouses.StockOf(house).CountOf(_town.Wood),
                "Le premier logis qui tient des marchandises reçoit le stock de départ, même sorti d'un chantier.");
            Assert.AreEqual(20, world.Warehouses.StockOf(house).CountOf(_town.Stone));
            Assert.AreEqual(50, world.Ledger.CreatedBy(GoodsCause.StartingStock));
            Assert.AreEqual(50, stockWhenHeard, "Quand on l'entend bâtie, elle est déjà en service.");
            Assert.AreEqual(2, world.WorkSlots.All.Count);
            Assert.AreSame(house, world.WorkSlots.All[1].Workplace, "Son poste s'ouvre avec elle.");
        }

        [Test]
        public void AHomeThatHoldsNothingGetsNothing()
        {
            World world = _town.World();
            Assert.IsNotNull(TestTown.Place(world, _town.KeepKnown(TestData.Home("abri_vide")), 20f, 14f),
                "Le logis vide se pose : rien ne lui est donné, ce n'est pas faute d'être là.");

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
