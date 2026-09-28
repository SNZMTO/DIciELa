using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les trajets, cas par cas, sur des terrains dessinés (le nord en haut) à deux mailles par mètre : « # » un bloc
    /// posé par l'ordre « poser », « S » un départ, « G » une arrivée. Les coûts attendus sont comptés à la main : un pas
    /// droit coûte 10, un pas en diagonale 14.
    /// </summary>
    public sealed class PathFinderTests
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
            UnityEngine.Object.DestroyImmediate(_settings);
            UnityEngine.Object.DestroyImmediate(_block);
        }

        private TestGround Ground(params string[] rows)
        {
            return TestGround.Drawn(_settings, _block, rows);
        }

        private bool Find(TestGround ground)
        {
            return ground.World.PathFinder.TryFindPath(ground.Starts, ground.Goals, _path);
        }

        /// <summary>Le coût du trajet trouvé, après avoir vérifié chacun de ses pas.</summary>
        private int CostOf(TestGround ground)
        {
            string fault = SlowPaths.Judge(ground.Blocked, ground.Starts, ground.Goals, _path.Cells, out int cost);
            Assert.IsNull(fault, fault + "\n" + ground.Describe(_path.Cells));
            return cost;
        }

        [Test]
        public void ADiagonalNeverCutsACorner()
        {
            // Frôler l'angle du bloc coûterait 34 ; le trajet fait quatre pas droits autour de lui.
            TestGround ground = Ground(
                ".....",
                "...G.",
                "..#..",
                ".S...",
                ".....");

            Assert.IsTrue(Find(ground));
            Assert.AreEqual(40, CostOf(ground), ground.Describe(_path.Cells));
        }

        [Test]
        public void BlocksTouchingByTheirCornersMakeAWall()
        {
            TestGround ground = Ground(
                "#.....G",
                ".#.....",
                "..#....",
                "...#...",
                "....#..",
                ".....#.",
                "S.....#");

            Assert.IsFalse(Find(ground), ground.Describe(_path.Cells));
            Assert.IsEmpty(_path.Cells);
        }

        [Test]
        public void AnEnclosedGoalHasNoTrajet()
        {
            TestGround ground = Ground(
                ".......",
                ".#####.",
                ".#...#.",
                ".#.G.#.",
                ".#...#.",
                ".#####.",
                "S......");

            Assert.IsFalse(Find(ground), ground.Describe(_path.Cells));
            Assert.IsEmpty(_path.Cells);
        }

        [Test]
        public void AStartOnAGoalIsATrajetOfOneCell()
        {
            TestGround ground = Ground(
                "...",
                ".S.",
                "...");
            ground.Goals.Add(ground.Starts[0]);

            Assert.IsTrue(Find(ground));
            Assert.AreEqual(new[] { new Vector2Int(1, 1) }, _path.Cells);
        }

        [Test]
        public void TheCheapestGoalWinsNotTheNearestAsTheCrowFlies()
        {
            // L'arrivée du bas paraît plus près (34 à vol d'oiseau), mais son enclos ne s'ouvre qu'à l'est : il faut
            // contourner le mur par le nord. Celle du haut coûte 40, tout droit.
            TestGround ground = Ground(
                "G......",
                ".......",
                ".......",
                "..###..",
                "S.#....",
                "..#G...",
                "..###..");

            Assert.IsTrue(Find(ground));
            Assert.AreEqual(new Vector2Int(0, 6), _path.Cells[_path.Cells.Count - 1], ground.Describe(_path.Cells));
            Assert.AreEqual(40, CostOf(ground));
        }

        [Test]
        public void StartsAndGoalsOnATakenCellAreIgnored()
        {
            TestGround ground = Ground(
                ".....",
                ".#.G.",
                ".....",
                ".S.#.",
                ".....");
            var takenStart = new Vector2Int(3, 1);
            var takenGoal = new Vector2Int(1, 3);
            ground.Starts.Insert(0, takenStart);
            ground.Goals.Insert(0, takenGoal);

            Assert.IsTrue(Find(ground));
            Assert.AreEqual(new Vector2Int(1, 1), _path.Cells[0]);
            Assert.AreEqual(28, CostOf(ground), ground.Describe(_path.Cells));

            PathFinder finder = ground.World.PathFinder;
            Assert.IsFalse(finder.TryFindPath(new[] { takenStart }, ground.Goals, _path));
            Assert.IsFalse(finder.TryFindPath(ground.Starts, new[] { takenGoal }, _path));
            Assert.IsEmpty(_path.Cells);
        }

        [Test]
        public void ACellOutsideTheGridIsAnError()
        {
            TestGround ground = Ground(
                "...",
                ".S.",
                "..G");
            PathFinder finder = ground.World.PathFinder;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                finder.TryFindPath(new[] { new Vector2Int(-1, 0) }, ground.Goals, _path));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                finder.TryFindPath(ground.Starts, new[] { new Vector2Int(3, 0) }, _path));
        }
    }
}
