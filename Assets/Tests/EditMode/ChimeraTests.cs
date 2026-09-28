using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La Chimère, par ses ordres, sur le banc de TestChimera : elle naît au point d'arrivée, une seule par monde, puis
    /// flâne à 6 m au plus de là où elle est née, avec des pauses de 30 à 90 pas.
    /// </summary>
    public sealed class ChimeraTests
    {
        private TestChimera _test;

        [SetUp]
        public void CreateData()
        {
            _test = new TestChimera();
        }

        [TearDown]
        public void DestroyData()
        {
            _test.Dispose();
        }

        [Test]
        public void TheOrderCreatesHerOnTheMiddleOfTheArrivalCell()
        {
            // Le point d'arrivée (5,3 ; 7,9) est dans la maille (10 ; 15), dont le milieu est (5,25 ; 7,75).
            World world = _test.World(arrival: new Vector2(5.3f, 7.9f));
            Villager chimera = world.Villagers.Chimera;

            Assert.IsNotNull(chimera);
            Assert.AreEqual(1, world.Villagers.All.Count);
            Assert.AreSame(chimera, world.Villagers.All[0]);
            Assert.AreEqual(1, chimera.Id);
            Assert.AreSame(_test.Kind, chimera.Kind);
            Assert.AreEqual(new Vector2(5.25f, 7.75f), chimera.Position);
            Assert.AreEqual(chimera.Position, chimera.PreviousPosition);
        }

        [Test]
        public void WithoutArrivalPointSheAppearsAtTheCenter()
        {
            // Le centre (20 ; 20) est le coin de quatre mailles : il tombe dans la maille (40 ; 40).
            Assert.AreEqual(new Vector2(20.25f, 20.25f), _test.World().Villagers.Chimera.Position);
        }

        [Test]
        public void OnlyOneChimeraPerWorldAnnouncedOnce()
        {
            var world = new World(_test.Settings, TestData.FlatMap(40f));
            var arrivals = new List<Villager>();
            world.Villagers.VillagerArrived += arrivals.Add;

            world.Submit(new CreateChimeraCommand(_test.Kind));
            world.Submit(new CreateChimeraCommand(_test.Kind));
            world.Step();
            world.Submit(new CreateChimeraCommand(_test.Kind));
            world.Step();

            Assert.AreEqual(1, world.Villagers.All.Count);
            Assert.AreEqual(1, arrivals.Count);
            Assert.AreSame(world.Villagers.Chimera, arrivals[0]);
        }

        [Test]
        public void SheWandersWithinHerRadiusPausingBetweenHops()
        {
            World world = _test.World();
            Villager chimera = world.Villagers.Chimera;
            Vector2 anchor = chimera.Position;

            float farthest = 0f;
            var idleRuns = new List<int>();
            int idle = 1;
            Vector2 stop = anchor;
            int hops = 0;
            // 12 000 pas : 40 heures de jeu, plus d'une centaine de sauts.
            for (int step = 0; step < 12000; step++)
            {
                world.Step();
                farthest = Mathf.Max(farthest, Vector2.Distance(anchor, chimera.Position));
                float moved = Vector2.Distance(chimera.PreviousPosition, chimera.Position);
                Assert.LessOrEqual(moved, TestChimera.MetersPerStep + 1e-4f, "Plus de 15 cm en un pas.");
                if (moved > 0f)
                {
                    if (idle > 0)
                        idleRuns.Add(idle);
                    idle = 0;
                    continue;
                }
                if (idle == 0)
                {
                    // Un arrêt : au moins 2 m (un tiers du rayon) depuis l'arrêt d'avant.
                    Assert.GreaterOrEqual(Vector2.Distance(stop, chimera.Position), 2f - 1e-3f, "Un saut minuscule.");
                    stop = chimera.Position;
                    hops++;
                }
                idle++;
            }

            Assert.LessOrEqual(farthest, 6f + 1e-3f, "Elle s'éloigne de plus de 6 m de là où elle flâne.");
            Assert.Greater(hops, 50);
            // Chaque arrêt dure une pause (30 à 90 pas), plus le pas où elle tire son point : 31 pas au moins. Un tirage
            // raté ajoute une pause ; jamais plusieurs de suite sur l'herbe libre.
            idleRuns.Sort();
            Assert.GreaterOrEqual(idleRuns[0], 31, "Une pause trop courte.");
            Assert.LessOrEqual(idleRuns[idleRuns.Count - 1], 2 * 91, "Une pause trop longue.");
            int median = idleRuns[idleRuns.Count / 2];
            Assert.IsTrue(median >= 31 && median <= 91, "La pause médiane est de " + median + " pas.");
        }

        [Test]
        public void TheSameOrdersPlayTheSameWanderings()
        {
            World first = _test.World();
            World second = _test.World();

            for (int step = 0; step < 3000; step++)
            {
                first.Step();
                second.Step();
                Assert.AreEqual(first.Villagers.Chimera.Position, second.Villagers.Chimera.Position, "Au pas " + step);
            }
        }
    }
}
