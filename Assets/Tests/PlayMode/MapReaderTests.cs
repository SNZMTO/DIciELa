using System;
using Diciela.Game;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La lecture du Terrain. Un Terrain en mémoire de 32 × 32 m, haut de 10 m, posé à 5 m : sa hauteur monte en ligne
    /// droite d'est en ouest, de 5 m à 15 m. Pente attendue : atan(10 / 32) = 17,354°. Un Terrain range ses hauteurs
    /// par marches de 1 / 32 766 de sa hauteur (0,3 mm ici) : sur une maille de 1 m, la pente peut s'écarter de
    /// 0,03° ; d'où les tolérances.
    /// </summary>
    public sealed class MapReaderTests
    {
        private TerrainData _data;

        [SetUp]
        public void CreateTerrain()
        {
            _data = new TerrainData { heightmapResolution = 33 };
            _data.size = new Vector3(32f, 10f, 32f);
            var heights = new float[33, 33];
            for (int z = 0; z < 33; z++)
            {
                for (int x = 0; x < 33; x++)
                    heights[z, x] = x / 32f;
            }
            _data.SetHeights(0, 0, heights);
        }

        [TearDown]
        public void DestroyTerrain()
        {
            UnityEngine.Object.DestroyImmediate(_data);
        }

        [Test]
        public void ReadsTheReliefInWorldMeters()
        {
            MapDescription map = MapReader.Read(_data, new Vector3(0f, 5f, 0f));
            Relief relief = map.Relief;

            Assert.AreEqual(33, relief.SamplesX);
            Assert.AreEqual(33, relief.SamplesZ);
            Assert.AreEqual(32f, relief.SizeX, 1e-4f);
            Assert.AreEqual(5f, relief.HeightAt(0f, 20f), 1e-3f);
            Assert.AreEqual(10f, relief.HeightAt(16f, 3f), 1e-3f);
            Assert.AreEqual(15f, relief.HeightAt(32f, 32f), 1e-3f);
            Assert.AreEqual(17.354f, relief.SlopeDegreesAt(8.5f, 8.5f), 0.05f);
        }

        [Test]
        public void RefusesATerrainAwayFromTheOrigin()
        {
            Assert.Throws<ArgumentException>(() => MapReader.Read(_data, new Vector3(1f, 0f, 0f)));
        }
    }
}
