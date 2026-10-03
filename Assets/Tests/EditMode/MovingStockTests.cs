using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le stock d'un entrepôt qu'on déplace (GDD, section 9 ; décision 199) : le temps du déplacement, on n'y prend rien
    /// de neuf ; remonté, il sert de nouveau. Le banc de la ville au travail : la Chimère au poste « à tout faire » du
    /// campement, en (20 ; 14), le seul stock de la ville ; deux premiers habitants, charretiers (porter, et rien
    /// d'autre), arrivés dans une maison de deux lits ; c'est la Chimère qui défait et remonte le campement. Rien ne lui
    /// était promis avant : défait, il garde tout, jusqu'à la dernière marchandise.
    /// </summary>
    public sealed class MovingStockTests
    {
        private TestTown _town;
        private StructureDefinition _house;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            WorkSlotDefinition carters = _town.Keep(TestData.WorkSlot("charretier", _town.Carry));
            TestData.FirstVillagers(_town.Opening,
                _town.Keep(TestData.FirstVillager("un", _town.Carry.SubStatistic, _town.Keep(TestData.Character("a")))),
                _town.Keep(TestData.FirstVillager("deux", _town.Carry.SubStatistic, _town.Keep(TestData.Character("b")))));
            TestData.FirstVillagersSlot(_town.Opening, carters);
            TestData.Ordinary(_town.Catalog, _town.Keep(TestData.OrdinaryKind()));
            _house = _town.KeepKnown(TestData.House("maison", 4f, 2));
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void AWarehouseThatMovesGivesNothingNewUntilItIsBackUp()
        {
            World world = _town.WorkingWorld();
            Structure camp = TestTown.CampOf(world);
            Inventory stock = world.Warehouses.StockOf(camp);
            TestTown.Place(world, _house, 30f, 8f);
            TestGrids.StepUntil(world, () => world.Villagers.All.Count == 3, 4000, "les deux charretiers arrivés");

            TestTown.Send(world, new MoveStructureCommand(camp.Id, new Placement(new Vector2(8f, 14f), 0f)));
            TestGrids.StepUntil(world, () => camp.State == StructureState.Moving, 4000, "le campement qu'on défait");
            int wood = stock.CountOf(_town.Wood);
            int stone = stock.CountOf(_town.Stone);
            Structure cabin = TestTown.Place(world, _town.Cabin, 30f, 30f);
            int checkedSteps = 0;
            TestGrids.StepUntil(world, () =>
            {
                if (camp.State != StructureState.Moving)
                    return true;
                checkedSteps++;
                Assert.AreEqual(0, Promised(stock), "Défait, le campement ne promet rien.");
                Assert.AreEqual(wood, stock.CountOf(_town.Wood), "Personne n'y prend de Bois.");
                Assert.AreEqual(stone, stock.CountOf(_town.Stone), "Personne n'y prend de Pierre.");
                return false;
            }, 4000, "le campement remonté");
            Assert.Greater(checkedSteps, 0, "Le campement est resté défait quelques pas.");
            Assert.AreEqual(StructureState.Built, camp.State);

            ConstructionSite site = world.ConstructionSites.SiteOf(cabin);
            TestGrids.StepUntil(world, () => site.DeliveredAt(0) > 0, 4000, "une livraison à la cabane");
        }

        // Ce que le stock a promis à des porteurs, en Bois et en Pierre : ce qu'il tient sans pouvoir le donner.
        private int Promised(Inventory stock)
        {
            return stock.CountOf(_town.Wood) - stock.AvailableOf(_town.Wood) + stock.CountOf(_town.Stone) -
                   stock.AvailableOf(_town.Stone);
        }
    }
}
