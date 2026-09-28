using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les îlots contre l'oracle (SAVOIR, 4) : les 400 terrains tirés au hasard des trajets, avec la même graine. Sur
    /// chacun, les îlots du monde découpent le terrain comme l'oracle, qui les remplit pas à pas en huit directions sans
    /// couper de coin : c'est aussi la preuve que les tronçons en quatre directions suffisent.
    /// </summary>
    public sealed class IslandsOracleTests
    {
        private const int Grounds = 400;
        private const int Seed = 20260928;

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
        public void TheIslandsAreTheSlowFloodsOnRandomGrounds()
        {
            var random = new System.Random(Seed);
            int split = 0;
            for (int n = 0; n < Grounds; n++)
            {
                TestGround ground = TestGround.Random(_settings, _block, random);
                int[,] expected = SlowPaths.Islands(ground.Blocked);
                string fault = SamePartition(expected, ground.World.Islands, out int islands);
                Assert.IsNull(fault, "Terrain " + n + " : " + fault + "\n" + ground.Describe(new Vector2Int[0]));
                if (islands > 1)
                    split++;
            }

            // Le tirage donne largement des terrains coupés en plusieurs îlots.
            Assert.GreaterOrEqual(split, 20, "Trop peu de terrains à plusieurs îlots.");
        }

        /// <summary>
        /// Ce qui distingue les îlots du monde de ceux de l'oracle, ou null s'ils découpent le terrain de la même façon :
        /// une maille bloquée n'est d'aucun îlot, et deux mailles libres sont du même îlot chez l'un comme chez l'autre.
        /// </summary>
        private static string SamePartition(int[,] expected, Islands islands, out int count)
        {
            count = 0;
            var actualOf = new Dictionary<int, int>();
            var expectedOf = new Dictionary<int, int>();
            for (int x = 0; x < expected.GetLength(0); x++)
            {
                for (int y = 0; y < expected.GetLength(1); y++)
                {
                    int want = expected[x, y];
                    int got = islands.IslandOf(new Vector2Int(x, y));
                    if (want == 0 || got == 0)
                    {
                        if (want != got)
                            return "la maille (" + x + ", " + y + ") est d'un îlot d'un côté seulement";
                        continue;
                    }
                    if ((actualOf.TryGetValue(want, out int seenGot) && seenGot != got) ||
                        (expectedOf.TryGetValue(got, out int seenWant) && seenWant != want))
                        return "la maille (" + x + ", " + y + ") n'est pas du même îlot chez l'oracle";
                    actualOf[want] = got;
                    expectedOf[got] = want;
                }
            }
            count = actualOf.Count;
            return null;
        }
    }
}
