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
    /// Les trajets dans un monde : autour d'un fantôme posé par l'ordre « poser », jusqu'au devant de sa porte, et tendus
    /// contre ses coins ; à travers une carte vide à l'échelle du jeu ; sans allocation d'une recherche à l'autre, même
    /// quand les îlots se refont. Carte de 20 m à deux mailles
    /// par mètre (40 × 40) ; la cabane de 4 × 4 m posée en (10 ; 10) couvre les mailles 16 à 23 en x et en z, et le devant
    /// de sa porte (1 m de large, 1 m de profondeur) les mailles 19 et 20 en x, 24 et 25 en z.
    /// </summary>
    public sealed class PathFinderWorldTests
    {
        private static readonly Vector2Int[] South = { new Vector2Int(20, 8) };
        private static readonly Vector2Int[] North = { new Vector2Int(20, 32) };

        private readonly Path _path = new Path();
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
            string fault = SlowPaths.Judge(TestGrids.Occupied(world.Grid), South, North, _path.Cells, out int cost);
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
            Assert.AreEqual(doorway, _path.Cells[_path.Cells.Count - 1]);
        }

        [Test]
        public void TheTautLineHugsTheCornersOfTheGhost()
        {
            // L'escalier contourne le fantôme par l'est. Tendue, la ligne plie deux fois contre sa façade est : près du
            // coin sud-est, au milieu de la maille (24 ; 16), la dernière d'où l'on voit le départ ; près du coin nord-est,
            // en (24 ; 23), la première d'où l'on voit l'arrivée. En mailles : racine de 80, plus 7, plus racine de 97.
            World world = CabinWorld();

            Assert.IsTrue(world.PathFinder.TryFindPath(South, North, _path));
            var expected = new[]
            {
                new Vector2(10.25f, 4.25f), new Vector2(12.25f, 8.25f), new Vector2(12.25f, 11.75f),
                new Vector2(10.25f, 16.25f)
            };
            Assert.AreEqual(expected, _path.Corners, Corners());
            Assert.AreEqual((Mathf.Sqrt(80f) + 7f + Mathf.Sqrt(97f)) * 0.5f, _path.Length, 1e-3f);
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

                string fault = SlowPaths.Judge(new bool[2000, 2000], starts, goals, _path.Cells, out int cost);
                Assert.IsNull(fault, fault);
                Assert.AreEqual(25990, cost);
                Assert.Less(watch.ElapsedMilliseconds, 200, "La recherche a pris " + watch.ElapsedMilliseconds + " ms.");

                // Sur l'herbe libre, la ligne tendue va tout droit, du milieu de la première maille à celui de la
                // dernière : de (0,125 ; 0,125) à (499,875 ; 375,125).
                Assert.AreEqual(2, _path.Corners.Count, Corners());
                Assert.AreEqual(Mathf.Sqrt(499.75f * 499.75f + 375f * 375f), _path.Length, 1e-2f);
            }
            finally
            {
                Object.DestroyImmediate(gameScale);
            }
        }

        [Test]
        public void ASecondSearchAllocatesNothingEvenWhenTheIslandsAreRedone()
        {
            World world = CabinWorld();
            PathFinder finder = world.PathFinder;
            Assert.IsTrue(finder.TryFindPath(South, North, _path));

            // Un bloc posé loin du trajet : la recherche suivante refait les îlots, puis cherche et tend.
            StructureDefinition block = TestData.Structure("bloc", 0.5f, 0.5f, hasDoor: false);
            try
            {
                world.Submit(new PlaceStructureCommand(block, new Placement(new Vector2(2.25f, 2.25f), 0f)));
                world.Step();
                Assert.AreEqual(2, world.Structures.All.Count);

                // Le compteur voit bien une allocation : sans cela, la seconde assertion ne prouverait rien.
                Assert.That(() => { _sink = new int[4]; }, Is.AllocatingGCMemory());
                Assert.AreEqual(4, _sink.Length);
                Assert.That(() => { finder.TryFindPath(South, North, _path); }, Is.Not.AllocatingGCMemory());
                Assert.AreEqual(North[0], _path.Cells[_path.Cells.Count - 1]);
                Assert.AreEqual(0, world.Islands.IslandOf(new Vector2Int(4, 4)));
            }
            finally
            {
                Object.DestroyImmediate(block);
            }
        }

        /// <summary>Les coins de la ligne tendue, en texte, pour lire un échec.</summary>
        private string Corners()
        {
            var text = new System.Text.StringBuilder("Coins :");
            foreach (Vector2 corner in _path.Corners)
                text.Append(' ').Append(corner.ToString("F3"));
            return text.ToString();
        }
    }
}
