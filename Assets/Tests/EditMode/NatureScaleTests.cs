using System.Diagnostics;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Une forêt à l'échelle du jeu (500 m à quatre mailles par mètre ; 5 000 arbres de 1 m semés au hasard, graine fixe,
    /// un tous les 50 m²) : le monde naît, refait ses îlots et trouve un trajet qui la traverse dans des temps qui laissent
    /// jouer. Les bornes, larges, attrapent un parcours qui exploserait avec le nombre d'arbres, pas quelques millisecondes.
    /// </summary>
    public sealed class NatureScaleTests
    {
        private const int TreeCount = 5000;

        [Test]
        public void AGameScaleForestStaysCheap()
        {
            SimulationSettings gameScale = TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 4);
            NatureDefinition tree = TestData.Nature("arbre", 1f, default, 0.5f);
            try
            {
                var spots = new NatureSpot[TreeCount];
                uint seed = 17;
                for (int i = 0; i < TreeCount; i++)
                    spots[i] = new NatureSpot(tree, new Placement(new Vector2(Next(ref seed), Next(ref seed)), 0f));

                Stopwatch watch = Stopwatch.StartNew();
                var world = new World(gameScale, TestData.FlatMap(500f, null, spots), TestData.EmptyCatalog);
                long birth = watch.ElapsedMilliseconds;

                var starts = new[] { new Vector2Int(0, 0) };
                var goals = new[] { new Vector2Int(1999, 1500) };
                var path = new Path();
                watch.Restart();
                Assert.IsTrue(world.PathFinder.TryFindPath(starts, goals, path), "La forêt se traverse.");
                long firstSearch = watch.ElapsedMilliseconds;
                watch.Restart();
                Assert.IsTrue(world.PathFinder.TryFindPath(starts, goals, path));
                long search = watch.ElapsedMilliseconds;

                TestContext.WriteLine("Forêt de " + TreeCount + " arbres : monde né en " + birth +
                                      " ms ; îlots et trajet en " + firstSearch + " ms ; trajet seul en " + search + " ms.");
                Assert.AreEqual(TreeCount, world.Nature.All.Count);
                Assert.Less(birth, 500, "Le monde est né en " + birth + " ms.");
                Assert.Less(firstSearch, 500, "Les îlots et le trajet ont pris " + firstSearch + " ms.");
                Assert.Less(search, 200, "Le trajet a pris " + search + " ms.");
            }
            finally
            {
                Object.DestroyImmediate(gameScale);
                Object.DestroyImmediate(tree);
            }
        }

        /// <summary>Un nombre de 10 à 490 m, tiré d'une graine qui avance : la même forêt à chaque fois.</summary>
        private static float Next(ref uint seed)
        {
            seed = seed * 1664525u + 1013904223u;
            return 10f + (seed >> 8) / (float)(1 << 24) * 480f;
        }
    }
}
