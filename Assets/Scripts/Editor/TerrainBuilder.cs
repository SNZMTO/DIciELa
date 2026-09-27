using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée un Terrain vierge et plat d'après les réglages d'une carte neuve. Il n'écrase jamais un Terrain qui existe :
    /// c'est peut-être une carte peinte à la main.
    /// </summary>
    internal static class TerrainBuilder
    {
        /// <summary>Le Terrain créé, ou null (avec une erreur dans la console) si le chemin est déjà pris.</summary>
        public static TerrainData CreateFlat(MapCreationSettings creation, TerrainLayer[] layers, string assetPath)
        {
            if (EditorAssets.Exists(assetPath))
            {
                Debug.LogError("DICIELA : un Terrain existe déjà à " + assetPath + " ; il n'est pas écrasé.");
                return null;
            }

            var data = new TerrainData
            {
                heightmapResolution = creation.HeightmapResolution,
                alphamapResolution = creation.AlphamapResolution
            };
            // La taille se règle après la résolution : changer la résolution recalcule l'échelle.
            data.size = new Vector3(creation.SizeMeters, creation.MaxHeightMeters, creation.SizeMeters);
            data.terrainLayers = layers;

            int resolution = data.heightmapResolution;
            float ground = Mathf.Clamp01(creation.GroundHeightMeters / creation.MaxHeightMeters);
            var heights = new float[resolution, resolution];
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                    heights[z, x] = ground;
            }
            data.SetHeights(0, 0, heights);

            EditorAssets.EnsureFolder(assetPath.Substring(0, assetPath.LastIndexOf('/')));
            AssetDatabase.CreateAsset(data, assetPath);
            Debug.Log("DICIELA : Terrain créé à " + assetPath + " : " + data.size.x + " × " + data.size.z + " m, relief de " +
                      data.heightmapResolution + " points, sol à " + creation.GroundHeightMeters + " m.");
            return data;
        }
    }
}
