using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Convertit la carte en description pure pour la simulation (ARCHITECTURE, section 10) : son Terrain, son point
    /// d'arrivée s'il y en a un, et les arbres et rochers peints sur le Terrain. Le Terrain est posé à l'origine : ses mètres sont ceux du monde, un seul repère.
    /// </summary>
    public static class MapReader
    {
        /// <param name="arrivalPosition">La place du point d'arrivée posé dans la carte ; null, le centre.</param>
        /// <param name="nature">Les sortes d'arbres et de rochers du catalogue ; null : la carte est lue nue.</param>
        /// <param name="natureInstances">
        /// Rempli, s'il est donné, du numéro d'instance sur le Terrain de chaque arbre ou rocher de la description, dans
        /// son ordre : la vue les retrouve ainsi.
        /// </param>
        public static MapDescription Read(TerrainData data, Vector3 terrainPosition, Vector3? arrivalPosition = null,
            IReadOnlyList<NatureDefinition> nature = null, List<int> natureInstances = null)
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

            Vector2? arrival = null;
            if (arrivalPosition.HasValue)
                arrival = new Vector2(arrivalPosition.Value.x, arrivalPosition.Value.z);
            var spots = new List<NatureSpot>();
            natureInstances?.Clear();
            if (nature != null)
                ReadNature(data, terrainPosition, nature, spots, natureInstances);
            return new MapDescription(new Relief(size.x, size.z, resolution, resolution, heights), arrival, spots);
        }

        /// <summary>
        /// Les arbres et les rochers peints sur le Terrain (décision 157) : chaque instance dont le modèle est rangé dans
        /// une sorte, à sa place, son angle et sa largeur. Un modèle rangé nulle part est dit une fois dans la console, et
        /// ses instances restent de simples décors.
        /// </summary>
        private static void ReadNature(TerrainData data, Vector3 terrainPosition, IReadOnlyList<NatureDefinition> nature,
            List<NatureSpot> spots, List<int> natureInstances)
        {
            TreePrototype[] prototypes = data.treePrototypes;
            var sortOf = new NatureDefinition[prototypes.Length];
            for (int p = 0; p < prototypes.Length; p++)
            {
                GameObject model = prototypes[p].prefab;
                sortOf[p] = SortOf(model, nature);
                if (sortOf[p] == null)
                    Debug.LogWarning("DICIELA : le modèle « " + (model != null ? model.name : "?") + " », peint sur la carte, n'est rangé dans aucune sorte d'arbre ou de rocher " +
                                     "(Assets/Data/Nature) : ses arbres ne se retirent pas et ne gênent personne.");
            }

            Vector3 size = data.size;
            int count = data.treeInstanceCount;
            for (int i = 0; i < count; i++)
            {
                TreeInstance tree = data.GetTreeInstance(i);
                NatureDefinition sort = sortOf[tree.prototypeIndex];
                if (sort == null || tree.widthScale <= 0f)
                    continue;
                var foot = new Vector2(terrainPosition.x + tree.position.x * size.x,
                    terrainPosition.z + tree.position.z * size.z);
                spots.Add(new NatureSpot(sort, new Placement(foot, tree.rotation * Mathf.Rad2Deg), tree.widthScale));
                natureInstances?.Add(i);
            }
        }

        private static NatureDefinition SortOf(GameObject model, IReadOnlyList<NatureDefinition> nature)
        {
            if (model == null)
                return null;
            foreach (NatureDefinition sort in nature)
            {
                foreach (GameObject each in sort.Models)
                {
                    if (each == model)
                        return sort;
                }
            }
            return null;
        }
    }
}
