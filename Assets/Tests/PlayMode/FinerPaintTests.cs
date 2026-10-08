using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La peinture des sols plus fine que la carte (décision 233), sur le banc des sols : la carte, peinte à 64 texels
    /// de côté (62,5 cm chacun) d'un dégradé de deux calques, d'ouest en est ; la vue des sols ouverte, sa copie
    /// s'affine à 25 cm le texel au plus (256 texels, la puissance de deux qui suit), et garde le dégradé de la carte,
    /// à la précision près de sa peinture ; la carte, elle, ne bouge pas.
    /// </summary>
    public sealed class FinerPaintTests
    {
        private const int MapResolution = 64;

        [Test]
        public void TheCopyIsFinerAndKeepsThePaintingOfTheMap()
        {
            using (var bench = new TestFloorBench(MapResolution, Gradient))
            {
                Assert.AreEqual(MapResolution, bench.Original.alphamapResolution, "La carte ne bouge pas.");
                int resolution = bench.Copy.alphamapResolution;
                Assert.AreEqual(256, resolution, "La copie s'affine à 25 cm le texel au plus.");
                float[,,] weights = bench.Copy.GetAlphamaps(0, 0, resolution, resolution);
                int row = resolution / 2, checkedTexels = 0;
                for (int x = 0; x < resolution; x++)
                {
                    float east = (x + 0.5f) / resolution * TestFloorBench.SideMeters;
                    // Au bord, la peinture de la carte s'arrête au milieu de son dernier texel : on n'y regarde pas.
                    if (east < 1f || east > TestFloorBench.SideMeters - 1f)
                        continue;
                    Assert.AreEqual(east / TestFloorBench.SideMeters, weights[row, x, 1], 0.01f,
                        "Le dégradé de la carte, à " + east + " m de l'ouest.");
                    Assert.AreEqual(1f, weights[row, x, 0] + weights[row, x, 1], 0.01f, "Les poids font un.");
                    checkedTexels++;
                }
                Assert.Greater(checkedTexels, 200, "Le dégradé est lu tout du long.");
            }
        }

        // La carte d'un dégradé : le second calque de 0 à l'ouest à 1 à l'est, pris au milieu de chaque texel.
        private static void Gradient(TestFloorBench bench)
        {
            TerrainData map = bench.Original;
            map.terrainLayers = new[] { map.terrainLayers[0], bench.Layer("terre") };
            var weights = new float[MapResolution, MapResolution, 2];
            for (int y = 0; y < MapResolution; y++)
            {
                for (int x = 0; x < MapResolution; x++)
                {
                    float share = (x + 0.5f) / MapResolution;
                    weights[y, x, 0] = 1f - share;
                    weights[y, x, 1] = share;
                }
            }
            map.SetAlphamaps(0, 0, weights);
        }
    }
}
