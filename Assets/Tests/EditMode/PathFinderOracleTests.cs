using Diciela.Data;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les trajets contre les oracles (SAVOIR, 4) : 400 terrains tirés au hasard, de 4 à 24 mailles de côté, bloqués de 0
    /// à 45 %, avec un à trois départs et une à trois arrivées, pris ou non. La graine est fixe : le même tirage à chaque
    /// campagne. Sur chaque terrain, l'escalier trouvé est juste pas à pas et coûte ce que coûte le moins cher selon le
    /// Dijkstra ; s'il n'y en a pas, la recherche le dit. La ligne tendue qu'on en tire ne touche que des mailles libres,
    /// selon un juge lent qui essaie chaque maille autour de chaque segment.
    /// </summary>
    public sealed class PathFinderOracleTests
    {
        private const int Grounds = 400;
        private const int Seed = 20260928;

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

        [Test]
        public void TheTrajetCostsWhatTheSlowSearchFindsOnRandomGrounds()
        {
            var random = new System.Random(Seed);
            int reachable = 0;
            for (int n = 0; n < Grounds; n++)
            {
                TestGround ground = TestGround.Random(_settings, _block, random);
                int expected = SlowPaths.CheapestCost(ground.Blocked, ground.Starts, ground.Goals);
                bool found = ground.World.PathFinder.TryFindPath(ground.Starts, ground.Goals, _path);
                string where = "Terrain " + n + " :\n" + ground.Describe(_path.Cells);

                if (expected == SlowPaths.NoPath)
                {
                    Assert.IsFalse(found, where);
                    Assert.IsEmpty(_path.Cells, where);
                    Assert.IsEmpty(_path.Corners, where);
                    continue;
                }
                Assert.IsTrue(found, where);
                string fault = SlowPaths.Judge(ground.Blocked, ground.Starts, ground.Goals, _path.Cells, out int cost);
                Assert.IsNull(fault, fault + "\n" + where);
                Assert.AreEqual(expected, cost, where);
                reachable++;
            }

            // Le tirage essaie largement les deux issues.
            Assert.GreaterOrEqual(reachable, 20, "Trop peu de terrains avec un trajet.");
            Assert.GreaterOrEqual(Grounds - reachable, 20, "Trop peu de terrains sans trajet.");
        }

        [Test]
        public void TheTautLineOnlyTouchesFreeCellsOnRandomGrounds()
        {
            var random = new System.Random(Seed);
            int tightened = 0;
            for (int n = 0; n < Grounds; n++)
            {
                TestGround ground = TestGround.Random(_settings, _block, random);
                if (!ground.World.PathFinder.TryFindPath(ground.Starts, ground.Goals, _path))
                    continue;

                string fault = SlowLines.Judge(ground.Blocked, _settings.CellsPerMeter, _path);
                Assert.IsNull(fault, "Terrain " + n + " : " + fault + "\n" + ground.Describe(_path.Cells));
                if (_path.Corners.Count < _path.Cells.Count)
                    tightened++;
            }

            // Le tirage donne largement des escaliers à tendre.
            Assert.GreaterOrEqual(tightened, 20, "Trop peu de trajets tendus.");
        }
    }
}
