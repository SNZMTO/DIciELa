using System.Collections.Generic;
using System.Diagnostics;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les trajets dans un monde : autour d'un fantôme posé par l'ordre « poser » et jusqu'au devant de sa porte ; à
    /// travers une carte vide à l'échelle du jeu ; sans allocation d'une recherche à l'autre. Carte de 20 m à deux mailles
    /// par mètre (40 × 40) ; la cabane de 4 × 4 m posée en (10 ; 10) couvre les mailles 16 à 23 en x et en z, et le devant
    /// de sa porte (1 m de large, 1 m de profondeur) les mailles 19 et 20 en x, 24 et 25 en z.
    /// </summary>
    public sealed class PathFinderWorldTests
    {
        private static readonly Vector2Int[] South = { new Vector2Int(20, 8) };
        private static readonly Vector2Int[] North = { new Vector2Int(20, 32) };

        private readonly List<Vector2Int> _path = new List<Vector2Int>();
        private SimulationSettings _settings;
        private StructureDefinition _cabin;
        private int[] _sink;

        [SetUp]
        public void CreateData()
        {
            _settings = TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 2);
            _cabin = TestData.Structure("cabane", 4f, 4f);
        }

        [TearDown]
        public void DestroyData()
        {
            Object.DestroyImmediate(_settings);
            Object.DestroyImmediate(_cabin);
        }

        private World CabinWorld()
        {
            var world = new World(_settings, TestData.FlatMap(20f));
            world.Submit(new PlaceStructureCommand(_cabin, new Placement(new Vector2(10f, 10f), 0f)));
            world.Step();
            Assert.AreEqual(StructureState.Planned, world.Structures.All[0].State);
            return world;
        }

        [Test]
        public void AGhostBlocksThePassageFromItsPlacement()
        {
            // Tout droit coûterait 240 ; le moins cher contourne le fantôme par l'est : quatre pas en diagonale pour
            // s'écarter, seize droits, quatre pour revenir, 272.
            World world = CabinWorld();

            Assert.IsTrue(world.PathFinder.TryFindPath(South, North, _path));
            string fault = SlowPaths.Judge(Occupied(world.Grid), South, North, _path, out int cost);
            Assert.IsNull(fault, fault);
            Assert.AreEqual(272, cost);
        }

        [Test]
        public void TheFrontOfAGhostsDoorIsReached()
        {
            World world = CabinWorld();
            var doorway = new Vector2Int(20, 25);

            Assert.IsTrue(world.Grid.IsDoorway(doorway));
            Assert.IsTrue(world.PathFinder.TryFindPath(South, new[] { doorway }, _path));
            Assert.AreEqual(doorway, _path[_path.Count - 1]);
        }

        [Test]
        public void AnOpenMapAtGameScaleIsCrossedAtTheOctileDistance()
        {
            // 500 m à quatre mailles par mètre : 2 000 × 2 000 mailles, comme la carte du jeu. De (0 ; 0) à
            // (1 999 ; 1 500) : 1 500 pas en diagonale et 499 droits, 1 500 × 14 + 499 × 10 = 25 990.
            SimulationSettings gameScale = TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 4);
            try
            {
                var world = new World(gameScale, TestData.FlatMap(500f));
                Assert.AreEqual(2000, world.Grid.Width);
                var starts = new[] { new Vector2Int(0, 0) };
                var goals = new[] { new Vector2Int(1999, 1500) };
                Assert.IsTrue(world.PathFinder.TryFindPath(starts, goals, _path));

                // Sur l'herbe libre, la recherche file droit : quelques millisecondes. La borne, très large, attrape une
                // recherche qui fouillerait toute la bande des trajets les moins chers, des centaines de milliers de
                // mailles.
                Stopwatch watch = Stopwatch.StartNew();
                Assert.IsTrue(world.PathFinder.TryFindPath(starts, goals, _path));
                watch.Stop();

                string fault = SlowPaths.Judge(new bool[2000, 2000], starts, goals, _path, out int cost);
                Assert.IsNull(fault, fault);
                Assert.AreEqual(25990, cost);
                Assert.Less(watch.ElapsedMilliseconds, 200, "La recherche a pris " + watch.ElapsedMilliseconds + " ms.");
            }
            finally
            {
                Object.DestroyImmediate(gameScale);
            }
        }

        [Test]
        public void ASecondSearchAllocatesNothing()
        {
            World world = CabinWorld();
            PathFinder finder = world.PathFinder;
            Assert.IsTrue(finder.TryFindPath(South, North, _path));

            // Le compteur voit bien une allocation : sans cela, la seconde assertion ne prouverait rien.
            Assert.That(() => { _sink = new int[4]; }, Is.AllocatingGCMemory());
            Assert.AreEqual(4, _sink.Length);
            Assert.That(() => { finder.TryFindPath(South, North, _path); }, Is.Not.AllocatingGCMemory());
            Assert.AreEqual(North[0], _path[_path.Count - 1]);
        }

        /// <summary>Les mailles prises du maillage, pour juger un trajet autour d'autre chose que des blocs dessinés.</summary>
        private static bool[,] Occupied(NavGrid grid)
        {
            var occupied = new bool[grid.Width, grid.Depth];
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Depth; y++)
                    occupied[x, y] = grid.OccupantAt(new Vector2Int(x, y)) != 0;
            }
            return occupied;
        }
    }
}
