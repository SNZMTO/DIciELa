using System.Collections.Generic;
using Diciela.Data;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le A* contre l'oracle (SAVOIR, 4) : 400 terrains tirés au hasard, de 4 à 24 mailles de côté, bloqués de 0 à 45 %,
    /// avec un à trois départs et une à trois arrivées, pris ou non. La graine est fixe : le même tirage à chaque
    /// campagne. Sur chaque terrain, le trajet trouvé est juste pas à pas et coûte ce que coûte le moins cher selon le
    /// Dijkstra ; s'il n'y en a pas, la recherche le dit.
    /// </summary>
    public sealed class PathFinderOracleTests
    {
        private const int Grounds = 400;
        private const int Seed = 20260928;

        private readonly List<Vector2Int> _path = new List<Vector2Int>();
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
                TestGround ground = RandomGround(random);
                int expected = SlowPaths.CheapestCost(ground.Blocked, ground.Starts, ground.Goals);
                bool found = ground.World.PathFinder.TryFindPath(ground.Starts, ground.Goals, _path);
                string where = "Terrain " + n + " :\n" + ground.Describe(_path);

                if (expected == SlowPaths.NoPath)
                {
                    Assert.IsFalse(found, where);
                    Assert.IsEmpty(_path, where);
                    continue;
                }
                Assert.IsTrue(found, where);
                string fault = SlowPaths.Judge(ground.Blocked, ground.Starts, ground.Goals, _path, out int cost);
                Assert.IsNull(fault, fault + "\n" + where);
                Assert.AreEqual(expected, cost, where);
                reachable++;
            }

            // Le tirage essaie largement les deux issues.
            Assert.GreaterOrEqual(reachable, 20, "Trop peu de terrains avec un trajet.");
            Assert.GreaterOrEqual(Grounds - reachable, 20, "Trop peu de terrains sans trajet.");
        }

        private TestGround RandomGround(System.Random random)
        {
            int size = random.Next(4, 25);
            double density = random.NextDouble() * 0.45;
            var blocked = new bool[size, size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                    blocked[x, y] = random.NextDouble() < density;
            }
            return TestGround.Build(_settings, _block, blocked, RandomCells(random, size), RandomCells(random, size));
        }

        private static List<Vector2Int> RandomCells(System.Random random, int size)
        {
            int count = random.Next(1, 4);
            var cells = new List<Vector2Int>(count);
            for (int i = 0; i < count; i++)
                cells.Add(new Vector2Int(random.Next(size), random.Next(size)));
            return cells;
        }
    }
}
