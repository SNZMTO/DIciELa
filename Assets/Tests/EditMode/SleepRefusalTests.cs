using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Society;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les ordres « dormir » et « réveiller » refusés (GDD, section 6 ; décision 96), sur le banc de TestBedroom : sans
    /// logis bâti, déjà couchée ou en chemin, debout, sans Chimère, ou sa porte hors d'atteinte (fermée par des blocs, ou
    /// elle encore sur un fantôme posé sur elle). Refusés, ils le disent et ne changent rien. En chemin, si sa porte est
    /// fermée, elle renonce.
    /// </summary>
    public sealed class SleepRefusalTests
    {
        private TestChimera _test;
        private readonly List<SleepRefusal> _refusals = new List<SleepRefusal>();

        [SetUp]
        public void CreateData()
        {
            _test = new TestChimera();
            _refusals.Clear();
        }

        [TearDown]
        public void DestroyData()
        {
            _test.Dispose();
        }

        private World WorldWithTent()
        {
            World world = TestBedroom.WorldWithTent(_test);
            world.Villagers.SleepRefused += _refusals.Add;
            return world;
        }

        [Test]
        public void WithoutABuiltHomeTheOrderIsRefused()
        {
            World world = _test.World();
            Villager chimera = world.Villagers.Chimera;
            world.Villagers.SleepRefused += _refusals.Add;

            world.Submit(new SendChimeraToBedCommand());
            world.Step();
            // Une maison qui attend ses bâtisseurs n'est pas encore un logis.
            TestBedroom.Place(world, _test.Keep(TestData.Home("maison", 4f, builtWhenPlaced: false)), 10f, 10f);
            world.Step();
            world.Submit(new SendChimeraToBedCommand());
            world.Step();

            Assert.AreEqual(new[] { SleepRefusal.NoHome, SleepRefusal.NoHome }, _refusals);
            Assert.IsFalse(chimera.IsGoingToBed);
        }

        [Test]
        public void SheIsNotSentTwiceNorWokenWhenUp()
        {
            World world = WorldWithTent();

            world.Submit(new WakeChimeraCommand());
            world.Step();
            world.Submit(new SendChimeraToBedCommand());
            world.Submit(new SendChimeraToBedCommand());
            world.Step();
            TestGrids.StepUntil(world, () => world.Villagers.Chimera.IsAsleep, 600, "son sommeil");
            world.Submit(new SendChimeraToBedCommand());
            world.Step();

            Assert.AreEqual(new[] { SleepRefusal.NotAsleep, SleepRefusal.AlreadyInBed, SleepRefusal.AlreadyInBed },
                _refusals);
        }

        [Test]
        public void WithoutChimeraBothOrdersAreRefused()
        {
            var world = new World(_test.Settings, TestData.FlatMap(40f), TestData.EmptyCatalog);
            world.Villagers.SleepRefused += _refusals.Add;

            world.Submit(new SendChimeraToBedCommand());
            world.Submit(new WakeChimeraCommand());
            world.Step();

            Assert.AreEqual(new[] { SleepRefusal.NoChimera, SleepRefusal.NoChimera }, _refusals);
        }

        [Test]
        public void ADoorShutInIsOutOfReach()
        {
            World world = WorldWithTent();
            Villager chimera = world.Villagers.Chimera;
            TestBedroom.EncloseTheDoor(world, _test);
            world.Step();
            Assert.AreEqual(4, world.Structures.All.Count, "Les trois blocs sont posés.");

            world.Submit(new SendChimeraToBedCommand());
            world.Step();

            Assert.AreEqual(new[] { SleepRefusal.Unreachable }, _refusals);
            Assert.IsFalse(chimera.IsGoingToBed);
        }

        [Test]
        public void WhileSheLeavesAGhostSheCannotBeSentToBed()
        {
            // Un bloc de 2 × 2 m posé sur elle : elle en sort à pied, en quelques pas. Tant qu'elle est sur le bloc, sa
            // porte est hors d'atteinte ; sur le sol libre, l'ordre passe, même avant la fin de sa sortie.
            World world = WorldWithTent();
            Villager chimera = world.Villagers.Chimera;
            TestBedroom.Place(world, _test.Block(2f, 2f), chimera.Position.x, chimera.Position.y);
            world.Step();

            world.Submit(new SendChimeraToBedCommand());
            world.Step();
            Assert.AreEqual(new[] { SleepRefusal.Unreachable }, _refusals);

            TestGrids.StepUntil(world, () => world.Grid.IsWalkable(world.Grid.CellOf(chimera.Position)), 100,
                "sa sortie");
            world.Submit(new SendChimeraToBedCommand());
            world.Step();
            Assert.IsTrue(chimera.IsGoingToBed);
            Assert.AreEqual(1, _refusals.Count);
        }

        [Test]
        public void SheGivesUpWhenHerDoorIsShutOnHerWay()
        {
            World world = WorldWithTent();
            Villager chimera = world.Villagers.Chimera;

            world.Submit(new SendChimeraToBedCommand());
            for (int step = 0; step < 3; step++)
                world.Step();
            TestBedroom.EncloseTheDoor(world, _test);
            world.Step();
            Assert.AreEqual(4, world.Structures.All.Count, "Les trois blocs sont posés.");

            TestGrids.StepUntil(world, () => !chimera.IsGoingToBed, 600, "son renoncement");
            for (int step = 0; step < 2000; step++)
            {
                world.Step();
                Assert.IsFalse(chimera.IsAsleep, "Sa porte est fermée : elle ne dort pas.");
                Assert.IsTrue(world.Grid.IsWalkable(world.Grid.CellOf(chimera.Position)));
            }
        }
    }
}
