using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le sommeil de la Chimère, par les ordres « dormir » et « réveiller » (GDD, section 6), sur le banc de TestBedroom :
    /// elle marche jusqu'à la porte de son campement et s'y couche, sans plus bouger ; réveillée, elle flâne devant sa porte,
    /// même réveillée en chemin, loin d'elle. Les refus ont leurs tests à part (SleepRefusalTests).
    /// </summary>
    public sealed class SleepTests
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

        [Test]
        public void SheWalksToHerDoorAndFallsAsleepThere()
        {
            World world = TestBedroom.WorldWithCamp(_test);
            Villager chimera = world.Villagers.Chimera;
            var wentToBed = new List<Villager>();
            var fellAsleep = new List<Villager>();
            world.Villagers.VillagerWentToBed += wentToBed.Add;
            world.Villagers.VillagerFellAsleep += fellAsleep.Add;
            world.Villagers.SleepRefused += _refusals.Add;

            world.Submit(new SendChimeraToBedCommand());
            world.Step();
            Assert.IsTrue(chimera.IsGoingToBed);
            Assert.IsTrue(chimera.IsWalking, "Elle marche vers son campement.");
            Assert.AreEqual(new[] { chimera }, wentToBed);

            TestGrids.StepUntil(world, () =>
            {
                Assert.AreEqual(chimera.IsAsleep ? 1 : 0, fellAsleep.Count, "Annoncé à la fin du pas même.");
                return chimera.IsAsleep;
            }, 600, "son sommeil");
            Assert.AreEqual(TestBedroom.Bed, chimera.Position, "Elle dort devant sa porte, au milieu de la maille.");
            Assert.IsFalse(chimera.IsGoingToBed);
            Assert.IsFalse(chimera.IsWalking);

            for (int step = 0; step < 1000; step++)
            {
                world.Step();
                Assert.IsTrue(chimera.IsAsleep, "Elle dort jusqu'à ce qu'on la réveille.");
                Assert.AreEqual(TestBedroom.Bed, chimera.Position);
                Assert.AreEqual(TestBedroom.Bed, chimera.PreviousPosition);
            }
            Assert.AreEqual(1, fellAsleep.Count);
            Assert.IsEmpty(_refusals);
        }

        [Test]
        public void WokenSheGetsUpAtHerDoorAndWandersAround()
        {
            World world = TestBedroom.WorldWithCamp(_test);
            Villager chimera = world.Villagers.Chimera;
            TestBedroom.SleepUntilAsleep(world);
            var wokeUp = new List<Villager>();
            world.Villagers.VillagerWokeUp += wokeUp.Add;

            world.Submit(new WakeChimeraCommand());
            world.Step();

            Assert.AreEqual(new[] { chimera }, wokeUp);
            Assert.IsFalse(chimera.IsAsleep);
            Assert.AreEqual(TestBedroom.Bed, chimera.Position, "Elle se lève devant sa porte, et y fait d'abord une pause.");
            TestGrids.StepUntil(world, () => chimera.Position != TestBedroom.Bed, 3000, "sa flânerie");
            // Flâner la mène à 6 m au plus de sa porte ; le tour du campement peut l'en écarter un peu plus.
            for (int step = 0; step < 3000; step++)
            {
                world.Step();
                Assert.IsFalse(chimera.IsAsleep);
                Assert.Less(Vector2.Distance(chimera.Position, TestBedroom.Bed), 9f, "Elle flâne autour de sa porte.");
            }
        }

        [Test]
        public void WokenOnHerWaySheWandersBeforeHerDoor()
        {
            World world = TestBedroom.WorldWithCamp(_test);
            Villager chimera = world.Villagers.Chimera;
            var wokeUp = new List<Villager>();
            world.Villagers.VillagerWokeUp += wokeUp.Add;

            world.Submit(new SendChimeraToBedCommand());
            world.Step();
            world.Submit(new WakeChimeraCommand());
            world.Step();

            Assert.AreEqual(new[] { chimera }, wokeUp);
            Assert.IsFalse(chimera.IsGoingToBed);
            Assert.Greater(Vector2.Distance(chimera.Position, TestBedroom.Bed), 7f, "Réveillée à 8 m de sa porte.");
            // Flâner autour de l'endroit du réveil l'écarterait de sa porte jusqu'à 14 m ; autour de sa porte, à 6 m au plus,
            // et le tour du campement peut l'en écarter un peu plus.
            TestGrids.StepUntil(world, () => Vector2.Distance(chimera.Position, TestBedroom.Bed) < 6.5f, 3000,
                "son retour devant sa porte");
            for (int step = 0; step < 3000; step++)
            {
                world.Step();
                Assert.IsFalse(chimera.IsAsleep, "Réveillée, elle ne se couche pas.");
                Assert.Less(Vector2.Distance(chimera.Position, TestBedroom.Bed), 9f, "Elle flâne devant sa porte.");
            }
        }
    }
}
