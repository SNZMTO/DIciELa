using System;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Convertit le Terrain de la carte en description pure pour la simulation (ARCHITECTURE, section 10). Le Terrain
    /// est posé à l'origine : ses mètres sont ceux du monde, un seul repère.
    /// </summary>
    public static class MapReader
    {
        public static MapDescription Read(TerrainData data, Vector3 terrainPosition)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (terrainPosition.x != 0f || terrainPosition.z != 0f)
                throw new ArgumentException("Le Terrain de la carte doit être posé à l'origine (x = 0, z = 0) : un seul " +
                                            "repère pour le monde.", nameof(terrainPosition));

            int resolution = data.heightmapResolution;
            float[,] normalized = data.GetHeights(0, 0, resolution, resolution);
            Vector3 size = data.size;

            var heights = new float[resolution * resolution];
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                    heights[z * resolution + x] = terrainPosition.y + normalized[z, x] * size.y;
            }

            return new MapDescription(new Relief(size.x, size.z, resolution, resolution, heights));
        }
    }
}
