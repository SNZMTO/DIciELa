using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// L'encombrement (GDD, section 8 ; décision 194), par le chemin du joueur : chaque marchandise prend autant de places
    /// que dit sa fiche, dans un entrepôt comme dans la charge d'un porteur. Le banc de la ville au travail : le campement
    /// a 100 places et reçoit au départ 30 Bois (une place chacun) et 10 meubles (deux places chacun) ; la Chimère porte
    /// 5 places.
    /// </summary>
    public sealed class BulkyGoodsTests
    {
        private TestTown _town;
        private GoodDefinition _furniture;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            _furniture = _town.Keep(TestData.Good("meuble", 2));
            TestData.StartingGoods(_town.Opening, new GoodAmount(_town.Wood, 30), new GoodAmount(_furniture, 10));
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void TheStartingStockTakesItsPlacesInTheCamp()
        {
            World world = _town.World();
            TestTown.Place(world, _town.Camp, 20f, 14f);
            Inventory stock = world.Warehouses.StockOf(TestTown.CampOf(world));

            Assert.AreEqual(40, stock.Total, "Quarante marchandises.");
            Assert.AreEqual(50, stock.Used, "Trente places de Bois, vingt de meubles.");
            Assert.AreEqual(50, stock.FreeSpace);
            Assert.AreEqual(25, stock.RoomFor(_furniture), "Il reste la place de vingt-cinq meubles.");
            Assert.AreEqual(50, stock.RoomFor(_town.Wood), "Ou de cinquante Bois.");
        }

        [Test]
        public void ACarrierTakesTwoFurnituresPerTrip()
        {
            // Une remise qui demande 4 meubles : la Chimère, qui porte 5 places, en prend deux par voyage.
            World world = _town.WorkingWorld();
            StructureDefinition shed = _town.KeepKnown(TestData.Structure("remise", 4f, 4f));
            TestData.Cost(shed, 0.5f, new GoodAmount(_furniture, 4));
            Structure site = TestTown.Place(world, shed, 20f, 24f);
            Villager chimera = world.Villagers.Chimera;

            int most = 0;
            TestGrids.StepUntil(world, () =>
            {
                most = Math.Max(most, chimera.Load.CountOf(_furniture));
                return site.State == StructureState.Built;
            }, 4000, "la remise bâtie");
            Assert.AreEqual(2, most, "Deux meubles par voyage : sa charge de cinq places n'en tient pas trois.");
            Assert.AreEqual(6, world.Warehouses.StockOf(TestTown.CampOf(world)).CountOf(_furniture),
                "Les quatre meubles sont pris dans la remise.");
        }
    }
}
