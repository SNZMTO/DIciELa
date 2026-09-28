using System.Diagnostics;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les îlots : sur des terrains dessinés (le nord en haut, deux mailles par mètre, « # » un bloc posé par l'ordre
    /// « poser »), après une pose, et sur une carte vide à l'échelle du jeu, où un trajet vers une arrivée enfermée est
    /// dit introuvable sans rien fouiller.
    /// </summary>
    public sealed class IslandsTests
    {
        private readonly Path _path = new Path();
        private SimulationSettings _settings;
        private StructureDefinition _block;

        [SetUp]
        public void CreateData()
        {
            _settings = TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 2);
            _block = TestData.Structure("bloc", 0.5f, 0.5f, hasDoor: false);
        }

        [TearDown]
        public void DestroyData()
        {
            Object.DestroyImmediate(_settings);
            Object.DestroyImmediate(_block);
        }

        private TestGround Ground(params string[] rows)
        {
            return TestGround.Drawn(_settings, _block, rows);
        }

        [Test]
        public void BlocksTouchingByTheirCornersCutTheGroundInTwo()
        {
            TestGround ground = Ground(
                "#.....G",
                ".#.....",
                "..#....",
                "...#...",
                "....#..",
                ".....#.",
                "S.....#");
            Islands islands = ground.World.Islands;
            int south = islands.IslandOf(ground.Starts[0]);
            int north = islands.IslandOf(ground.Goals[0]);

            Assert.Greater(south, 0);
            Assert.Greater(north, 0);
            Assert.AreNotEqual(south, north, ground.Describe(_path.Cells));
            Assert.AreEqual(south, islands.IslandOf(new Vector2Int(5, 0)), "Au sud du mur, un seul îlot.");
            Assert.AreEqual(north, islands.IslandOf(new Vector2Int(6, 1)), "Au nord du mur, un seul îlot.");
            Assert.AreEqual(0, islands.IslandOf(new Vector2Int(3, 3)), "Une maille prise n'est d'aucun îlot.");
        }

        [Test]
        public void AnEnclosureIsAnIslandOfItsOwn()
        {
            TestGround ground = Ground(
                ".......",
                ".#####.",
                ".#...#.",
                ".#.G.#.",
                ".#...#.",
                ".#####.",
                "S......");
            Islands islands = ground.World.Islands;
            int inside = islands.IslandOf(ground.Goals[0]);

            Assert.AreNotEqual(islands.IslandOf(ground.Starts[0]), inside);
            Assert.AreEqual(inside, islands.IslandOf(new Vector2Int(2, 2)));
            Assert.AreEqual(inside, islands.IslandOf(new Vector2Int(4, 4)));
            Assert.AreEqual(islands.IslandOf(ground.Starts[0]), islands.IslandOf(new Vector2Int(6, 6)));
        }

        [Test]
        public void ThePlacementThatClosesAGapCutsTheIsland()
        {
            TestGround ground = Ground(
                "....G",
                ".....",
                "##.##",
                ".....",
                "S....");
            World world = ground.World;
            Assert.AreEqual(world.Islands.IslandOf(ground.Starts[0]), world.Islands.IslandOf(ground.Goals[0]));

            var gap = new Vector2Int(2, 2);
            world.Submit(new PlaceStructureCommand(_block, new Placement(world.Grid.CellCenter(gap), 0f)));
            world.Step();

            Assert.AreEqual(0, world.Islands.IslandOf(gap));
            Assert.AreNotEqual(world.Islands.IslandOf(ground.Starts[0]), world.Islands.IslandOf(ground.Goals[0]));
            Assert.IsFalse(world.PathFinder.TryFindPath(ground.Starts, ground.Goals, _path));
        }

        [Test]
        public void AnEnclosedGoalOnTheGameMapIsAnsweredAtOnce()
        {
            // 500 m à quatre mailles par mètre : 2 000 × 2 000 mailles, comme la carte du jeu. Un enclos de blocs
            // d'une maille entoure l'arrivée, au milieu de la carte ; le départ est dans un coin.
            SimulationSettings gameScale = TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 4);
            StructureDefinition smallBlock = TestData.Structure("petit bloc", 0.25f, 0.25f, hasDoor: false);
            try
            {
                var world = new World(gameScale, TestData.FlatMap(500f));
                var goal = new Vector2Int(1000, 1000);
                for (int x = 997; x <= 1003; x++)
                {
                    for (int y = 997; y <= 1003; y++)
                    {
                        if (x == 997 || x == 1003 || y == 997 || y == 1003)
                            world.Submit(new PlaceStructureCommand(smallBlock,
                                new Placement(world.Grid.CellCenter(new Vector2Int(x, y)), 0f)));
                    }
                }
                world.Step();
                Assert.AreEqual(24, world.Structures.All.Count);
                var starts = new[] { new Vector2Int(0, 0) };
                var goals = new[] { goal };

                // La première réponse refait les îlots après la pose : toute la carte, rangée par rangée.
                Stopwatch watch = Stopwatch.StartNew();
                Assert.IsFalse(world.PathFinder.TryFindPath(starts, goals, _path));
                long labeling = watch.ElapsedMilliseconds;

                // Ensuite, rien ne change : la réponse vient sans rien fouiller. Un A* qui fouillerait toute la carte,
                // quatre millions de mailles, prendrait plus d'une seconde.
                watch.Restart();
                Assert.IsFalse(world.PathFinder.TryFindPath(starts, goals, _path));
                long answer = watch.ElapsedMilliseconds;

                TestContext.WriteLine("Îlots refaits en " + labeling + " ms ; introuvable dit en " + answer + " ms.");
                Assert.Less(labeling, 500, "Les îlots ont été refaits en " + labeling + " ms.");
                Assert.Less(answer, 200, "L'introuvable a été dit en " + answer + " ms.");
                Assert.IsEmpty(_path.Cells);
            }
            finally
            {
                Object.DestroyImmediate(gameScale);
                Object.DestroyImmediate(smallBlock);
            }
        }
    }
}
