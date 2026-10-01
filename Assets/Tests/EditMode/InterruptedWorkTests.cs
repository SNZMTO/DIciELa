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
    /// Le travail interrompu (GDD, sections 6 et 9 ; décisions 46, 150 et 156), par les ordres du joueur : envoyée dormir,
    /// retirée, ou couverte par une construction pendant qu'elle porte, la Chimère rend sa charge au campement ; endormie, elle
    /// garde son poste, et réveillée, elle reprend le travail ; retirée, elle n'a plus de poste ; couverte, elle sort,
    /// puis reprend le travail.
    /// </summary>
    public sealed class InterruptedWorkTests
    {
        private TestTown _town;
        private World _world;
        private Villager _chimera;
        private Inventory _campStock;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            _world = _town.WorkingWorld();
            TestTown.Place(_world, _town.Cabin, 28f, 20f);
            _chimera = _world.Villagers.Chimera;
            _campStock = _world.Warehouses.StockOf(TestTown.CampOf(_world));
            TestGrids.StepUntil(_world, () => _chimera.Load.Total > 0, 2000, "une charge prise au campement");
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        private void Send(Command command)
        {
            _world.Submit(command);
            _world.Step();
        }

        [Test]
        public void SentToBedSheGivesHerLoadBackAndKeepsHerPost()
        {
            Assert.AreEqual(45, _campStock.Total, "Elle a pris 5 marchandises au campement.");

            Send(new SendChimeraToBedCommand());

            Assert.IsTrue(_chimera.IsGoingToBed);
            Assert.AreEqual(0, _chimera.Load.Total, "Sa charge est rendue.");
            Assert.AreEqual(50, _campStock.Total, "Le campement a tout repris.");
            Assert.IsNotNull(_chimera.Slot, "Endormie, elle garde son poste.");
            Assert.AreEqual(_world.Ledger.CreatedOf(_town.Wood), TestTown.EverywhereOf(_world, _town.Wood));

            TestGrids.StepUntil(_world, () => _chimera.IsAsleep, 2000, "la Chimère endormie");
            for (int i = 0; i < 300; i++)
                _world.Step();
            Assert.IsTrue(_chimera.IsAsleep);
            Assert.AreEqual(50, _campStock.Total, "Endormie, elle ne travaille pas.");

            Send(new WakeChimeraCommand());
            TestGrids.StepUntil(_world, () => _chimera.Load.Total > 0, 2000, "le travail repris");
        }

        [Test]
        public void CoveredSheGivesHerLoadBackThenGoesBackToWork()
        {
            for (int i = 0; i < 30; i++)
                _world.Step();
            Assert.Greater(_chimera.Load.Total, 0, "Elle porte encore, en chemin.");
            Vector2 cell = _world.Grid.CellCenter(_world.Grid.CellOf(_chimera.Position));
            StructureDefinition crate = _town.Keep(TestData.Structure("caisse", 1f, 1f, hasDoor: false));

            Assert.IsNotNull(TestTown.Place(_world, crate, cell.x, cell.y), "La caisse se pose sur elle.");

            Assert.AreEqual(0, _chimera.Load.Total, "Couverte, elle laisse sa tâche : sa charge est rendue.");
            Assert.AreEqual(50, _campStock.Total, "Le campement a tout repris.");
            Assert.IsNotNull(_chimera.Slot, "Elle garde son poste.");
            TestGrids.StepUntil(_world, () => _chimera.Load.Total > 0, 2000, "le travail repris, une fois sortie");
        }

        [Test]
        public void RemovedSheGivesHerLoadBackAndStopsWorking()
        {
            Send(new UnassignVillagerCommand(_chimera.Id));

            Assert.IsNull(_chimera.Slot);
            Assert.AreEqual(0, _chimera.Load.Total);
            Assert.AreEqual(50, _campStock.Total);
            for (int i = 0; i < 600; i++)
                _world.Step();
            Assert.AreEqual(50, _campStock.Total, "Sans poste, elle ne porte plus rien.");
        }
    }
}
