using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Le passage des mailles aux texels de la peinture d'un Terrain (SAVOIR, 2.5) : les texels que touchent des
    /// mailles, le milieu d'un texel sur le sol, et les boîtes de mailles qu'on agrandit, réunit ou borne à la carte.
    /// </summary>
    internal static class FloorTexels
    {
        /// <summary>Les texels de la peinture de ce Terrain que touchent ces mailles, bornés au Terrain.</summary>
        public static RectInt Of(Terrain terrain, NavGrid grid, RectInt cells)
        {
            TerrainData data = terrain.terrainData;
            int resolution = data.alphamapResolution;
            float size = grid.Metrics.CellSize;
            Vector3 origin = terrain.transform.position;
            int fromX = Mathf.Clamp(Mathf.FloorToInt((cells.xMin * size - origin.x) / data.size.x * resolution), 0,
                resolution);
            int toX = Mathf.Clamp(Mathf.CeilToInt((cells.xMax * size - origin.x) / data.size.x * resolution), 0,
                resolution);
            int fromY = Mathf.Clamp(Mathf.FloorToInt((cells.yMin * size - origin.z) / data.size.z * resolution), 0,
                resolution);
            int toY = Mathf.Clamp(Mathf.CeilToInt((cells.yMax * size - origin.z) / data.size.z * resolution), 0,
                resolution);
            return new RectInt(fromX, fromY, toX - fromX, toY - fromY);
        }

        /// <summary>Le milieu de ce texel de la peinture de ce Terrain, sur le sol, en mètres.</summary>
        public static Vector2 Center(Terrain terrain, int x, int y)
        {
            TerrainData data = terrain.terrainData;
            Vector3 origin = terrain.transform.position;
            return new Vector2(origin.x + (x + 0.5f) / data.alphamapResolution * data.size.x,
                origin.z + (y + 0.5f) / data.alphamapResolution * data.size.z);
        }

        /// <summary>Ces mailles, bornées à la carte.</summary>
        public static RectInt OnGrid(NavGrid grid, RectInt cells)
        {
            int fromX = Mathf.Max(cells.xMin, 0), fromY = Mathf.Max(cells.yMin, 0);
            int toX = Mathf.Min(cells.xMax, grid.Width), toY = Mathf.Min(cells.yMax, grid.Depth);
            return new RectInt(fromX, fromY, Mathf.Max(0, toX - fromX), Mathf.Max(0, toY - fromY));
        }

        /// <summary>Ces mailles, et autant de mailles tout autour.</summary>
        public static RectInt Expand(RectInt cells, int margin)
        {
            return new RectInt(cells.xMin - margin, cells.yMin - margin, cells.width + 2 * margin,
                cells.height + 2 * margin);
        }

        /// <summary>La plus petite boîte qui contient les deux.</summary>
        public static RectInt Union(RectInt a, RectInt b)
        {
            var min = Vector2Int.Min(a.min, b.min);
            var max = Vector2Int.Max(a.max, b.max);
            return new RectInt(min, max - min);
        }
    }
}
