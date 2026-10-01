using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Ce qui interrompt le défrichage au poste « à tout faire » (GDD, sections 2, 6 et 7 ; décisions 157 à 160), par
    /// les ordres du joueur : envoyée dormir avant d'abattre, la Chimère laisse l'arbre avec le travail fait, et le reprend
    /// à son réveil ; après, sa charge rentre au tas. Démarqué pendant qu'on l'abat, l'arbre reste là, et personne n'y
    /// travaille plus. Le défrichage sans interruption est dans ClearingTests.
    /// </summary>
    public sealed class ClearingInterruptionTests
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
        public void SentToBedBeforeFellingSheLeavesTheTreeWithItsWorkDone()
        {
            World world = _town.WorkingWorld(new[] { NatureTests.Spot(_town.Tree, 28f, 22f) });
            NatureItem tree = world.Nature.Find(1);
            Inventory pile = world.Warehouses.StockOf(_town.PileOf(world));
            TestTown.Send(world, new MarkForClearingCommand(new[] { tree.Id }));
            TestGrids.StepUntil(world, () => world.Clearings.ProgressOf(tree) > 0.2d, 3000, "l'arbre à moitié abattu");

            TestTown.Send(world, new SendChimeraToBedCommand());
            double done = world.Clearings.ProgressOf(tree);
            for (int i = 0; i < 100; i++)
                world.Step();

            Assert.IsFalse(tree.IsRemoved);
            Assert.IsTrue(tree.IsMarked, "Il reste marqué.");
            Assert.AreEqual(done, world.Clearings.ProgressOf(tree), 1e-9, "Le travail fait se garde.");
            Assert.AreEqual(0, pile.SpaceReserved, "La place retenue est rendue.");

            TestTown.Send(world, new WakeChimeraCommand());
            TestGrids.StepUntil(world, () => tree.IsRemoved, 3000, "l'arbre retiré au réveil");
        }

        [Test]
        public void SentToBedWithTheWoodSheGivesItBackToThePile()
        {
            World world = _town.WorkingWorld(new[] { NatureTests.Spot(_town.Tree, 28f, 22f) });
            NatureItem tree = world.Nature.Find(1);
            Villager chimera = world.Villagers.Chimera;
            Inventory pile = world.Warehouses.StockOf(_town.PileOf(world));
            TestTown.Send(world, new MarkForClearingCommand(new[] { tree.Id }));
            TestGrids.StepUntil(world, () => tree.IsRemoved, 3000, "l'arbre retiré");

            TestTown.Send(world, new SendChimeraToBedCommand());

            Assert.AreEqual(0, chimera.Load.Total, "Sa charge est rendue.");
            Assert.AreEqual(33, pile.CountOf(_town.Wood), "Le tas a le Bois de l'arbre.");
            Assert.AreEqual(0, pile.SpaceReserved);
        }

        [Test]
        public void UnmarkedWhileFellingTheTreeIsLeft()
        {
            World world = _town.WorkingWorld(new[] { NatureTests.Spot(_town.Tree, 28f, 22f) });
            NatureItem tree = world.Nature.Find(1);
            Inventory pile = world.Warehouses.StockOf(_town.PileOf(world));
            TestTown.Send(world, new MarkForClearingCommand(new[] { tree.Id }));
            TestGrids.StepUntil(world, () => world.Clearings.ProgressOf(tree) > 0d, 3000, "l'arbre entamé");

            TestTown.Send(world, new UnmarkForClearingCommand(new[] { tree.Id }));
            double done = world.Clearings.ProgressOf(tree);
            for (int i = 0; i < 300; i++)
                world.Step();

            Assert.IsFalse(tree.IsRemoved);
            Assert.AreEqual(done, world.Clearings.ProgressOf(tree), 1e-9, "Personne n'y travaille plus.");
            Assert.AreEqual(0, pile.SpaceReserved);
        }
    }
}
