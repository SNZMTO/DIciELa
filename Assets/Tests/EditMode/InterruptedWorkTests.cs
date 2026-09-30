using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le travail interrompu (GDD, section 6 ; décisions 46 et 150), par les ordres du joueur : envoyée dormir ou retirée
    /// pendant qu'elle porte, la Chimère rend sa charge au tas ; endormie, elle garde son poste, et réveillée, elle
    /// reprend le travail ; retirée, elle n'a plus de poste.
    /// </summary>
    public sealed class InterruptedWorkTests
    {
        private TestTown _town;
        private World _world;
        private Villager _chimera;
        private Inventory _pileStock;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            _world = _town.WorkingWorld();
            TestTown.Place(_world, _town.Cabin, 28f, 20f);
            _chimera = _world.Villagers.Chimera;
            _pileStock = _world.Warehouses.StockOf(_town.PileOf(_world));
            TestGrids.StepUntil(_world, () => _chimera.Load.Total > 0, 2000, "une charge prise au tas");
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
            Assert.AreEqual(45, _pileStock.Total, "Elle a pris 5 marchandises au tas.");

            Send(new SendChimeraToBedCommand());

            Assert.IsTrue(_chimera.IsGoingToBed);
            Assert.AreEqual(0, _chimera.Load.Total, "Sa charge est rendue.");
            Assert.AreEqual(50, _pileStock.Total, "Le tas a tout repris.");
            Assert.IsNotNull(_chimera.Slot, "Endormie, elle garde son poste.");
            Assert.AreEqual(_world.Ledger.CreatedOf(_town.Wood), TestTown.EverywhereOf(_world, _town.Wood));

            TestGrids.StepUntil(_world, () => _chimera.IsAsleep, 2000, "la Chimère endormie");
            for (int i = 0; i < 300; i++)
                _world.Step();
            Assert.IsTrue(_chimera.IsAsleep);
            Assert.AreEqual(50, _pileStock.Total, "Endormie, elle ne travaille pas.");

            Send(new WakeChimeraCommand());
            TestGrids.StepUntil(_world, () => _chimera.Load.Total > 0, 2000, "le travail repris");
        }

        [Test]
        public void RemovedSheGivesHerLoadBackAndStopsWorking()
        {
            Send(new UnassignVillagerCommand(_chimera.Id));

            Assert.IsNull(_chimera.Slot);
            Assert.AreEqual(0, _chimera.Load.Total);
            Assert.AreEqual(50, _pileStock.Total);
            for (int i = 0; i < 600; i++)
                _world.Step();
            Assert.AreEqual(50, _pileStock.Total, "Sans poste, elle ne porte plus rien.");
        }
    }
}
