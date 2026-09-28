using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La marche de la Chimère quand on pose autour d'elle, par l'ordre « poser », le chemin du joueur (décision 73) : un
    /// fantôme qui coupe sa ligne la fait passer autour ; posé loin, il ne change rien ; posé sur son but, elle en
    /// choisit un autre après une pause. Sur le banc de TestChimera, carte de 40 m.
    /// </summary>
    public sealed class WalkTests
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

        private static Vector2 Last(IReadOnlyList<Vector2> corners)
        {
            return corners[corners.Count - 1];
        }

        [Test]
        public void AGhostAcrossHerLineMakesHerGoAround()
        {
            World world = _test.World();
            Villager chimera = TestChimera.StartALongWalk(world);
            Vector2 from = chimera.Position;
            Vector2 to = Last(chimera.Path.Corners);

            // Un mur de 2,5 × 1 m en travers, au milieu de sa ligne : sa longueur perpendiculaire à la marche. La maille
            // où tombe ce milieu a le sien à moins de 0,36 m, donc dans le mur : la ligne la touche.
            Vector2 along = (to - from).normalized;
            float angle = Mathf.Atan2(along.x, along.y) * Mathf.Rad2Deg;
            world.Submit(new PlaceStructureCommand(_test.Block(2.5f, 1f), new Placement((from + to) * 0.5f, angle)));
            world.Step();
            Assert.AreEqual(1, world.Structures.All.Count, "Le mur doit se poser.");

            // Le mur coupe bien la ligne d'avant : l'oracle essaie chaque maille qu'elle touche.
            bool[,] occupied = TestGrids.Occupied(world.Grid);
            Assert.IsTrue(Cuts(occupied, world.Grid.CellOf(from), world.Grid.CellOf(to)), "Le mur ne coupe rien.");

            // Elle cherche un autre trajet vers le même but, juste d'après l'oracle, et le suit sans entrer dans le mur.
            Assert.IsTrue(chimera.IsWalking);
            Assert.AreEqual(to, Last(chimera.Path.Corners));
            string fault = SlowLines.Judge(occupied, TestChimera.CellsPerMeter, chimera.Path);
            Assert.IsNull(fault, fault);
            TestGrids.StepUntil(world, () =>
            {
                Assert.IsTrue(world.Grid.IsWalkable(world.Grid.CellOf(chimera.Position)), "Elle entre dans le mur.");
                return !chimera.IsWalking;
            }, 400, "l'arrivée");
            Assert.AreEqual(to, chimera.Position);
        }

        [Test]
        public void AGhostAwayFromHerLineChangesNothing()
        {
            // Cinq pas après son départ, elle a quitté sa première maille : un nouveau trajet partirait d'ailleurs.
            World world = _test.World();
            Villager chimera = TestChimera.StartALongWalk(world);
            for (int i = 0; i < 5; i++)
                world.Step();
            var before = new List<Vector2>(chimera.Path.Corners);

            world.Submit(new PlaceStructureCommand(_test.Block(1f, 1f), new Placement(new Vector2(2f, 2f), 0f)));
            world.Step();

            Assert.AreEqual(1, world.Structures.All.Count);
            Assert.AreEqual(before, new List<Vector2>(chimera.Path.Corners));
        }

        [Test]
        public void AGhostOnHerGoalMakesHerPauseThenGoElsewhere()
        {
            World world = _test.World();
            Villager chimera = TestChimera.StartALongWalk(world);
            Vector2 goal = Last(chimera.Path.Corners);

            world.Submit(new PlaceStructureCommand(_test.Block(1f, 1f), new Placement(goal, 0f)));
            world.Step();
            Assert.IsFalse(world.Grid.IsWalkable(world.Grid.CellOf(goal)));

            // Destination perdue : une pause (30 pas au moins), puis un autre point.
            Assert.IsFalse(chimera.IsWalking);
            Vector2 stopped = chimera.Position;
            for (int step = 0; step < 29; step++)
            {
                world.Step();
                Assert.AreEqual(stopped, chimera.Position, "Elle repart sans sa pause.");
            }
            TestGrids.StepUntil(world, () => chimera.IsWalking, 100, "un nouveau départ");
            Assert.AreNotEqual(goal, Last(chimera.Path.Corners));
        }

        /// <summary>Vrai si la ligne droite du milieu de from à celui de to touche une maille prise, d'après l'oracle.</summary>
        private static bool Cuts(bool[,] occupied, Vector2Int from, Vector2Int to)
        {
            for (int x = 0; x < occupied.GetLength(0); x++)
            {
                for (int y = 0; y < occupied.GetLength(1); y++)
                {
                    if (occupied[x, y] && SlowLines.Touches(from, to, new Vector2Int(x, y)))
                        return true;
                }
            }
            return false;
        }
    }
}
