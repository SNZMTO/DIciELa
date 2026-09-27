using Diciela.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée les scènes de carte qui manquent ; ne touche jamais une carte qui existe.
    /// La carte naît d'une copie de la démo du pack dont on ne garde que le soleil, le volume de filtres et le Terrain :
    /// le rendu du pack est repris tel quel (GDD, section 16), et le Terrain reçoit un relief vierge à peindre.
    /// </summary>
    internal static class MapSceneBuilder
    {
        private const string DemoScenePath = "Assets/Synty/PolygonFantasyVillage/Scenes/Demo.unity";
        private const string MapTerrainPath = "Assets/Terrain/Carte.asset";
        private const string TestMapTerrainPath = "Assets/Terrain/CarteTest.asset";
        private const string MapVolumeProfilePath = "Assets/Art/Carte_Filtres.asset";
        private const string VolumeProfileField = "sharedProfile";

        public static void CreateMapIfMissing()
        {
            if (EditorAssets.Exists(ScenePaths.Map))
            {
                Debug.Log("DICIELA : la carte existe déjà ; elle n'est pas touchée.");
                return;
            }
            if (!EditorAssets.Exists(DemoScenePath))
            {
                Debug.LogError("DICIELA : la démo du pack est introuvable (" + DemoScenePath + ") : importer le pack.");
                return;
            }

            EditorAssets.EnsureFolder("Assets/Scenes");
            AssetDatabase.CopyAsset(DemoScenePath, ScenePaths.Map);
            Scene scene = EditorSceneManager.OpenScene(ScenePaths.Map, OpenSceneMode.Single);
            Terrain terrain = KeepSunFiltersAndTerrain(scene);
            var creation = EditorAssets.Load<MapCreationSettings>(DataAssetBuilder.MapCreationPath);
            TerrainData data = terrain != null
                ? TerrainBuilder.CreateFlat(creation, terrain.terrainData.terrainLayers, MapTerrainPath)
                : null;
            if (data == null)
            {
                Debug.LogError("DICIELA : la carte n'a pas pu être créée ; la copie de la démo est retirée.");
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                AssetDatabase.DeleteAsset(ScenePaths.Map);
                return;
            }

            UseTerrainData(terrain, data);
            CopyFiltersProfile(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("DICIELA : carte créée dans " + ScenePaths.Map + ", à peindre.");
        }

        public static void CreateTestMapIfMissing()
        {
            if (EditorAssets.Exists(ScenePaths.MapTest))
                return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var creation = EditorAssets.Load<MapCreationSettings>(DataAssetBuilder.TestMapCreationPath);
            TerrainData data = TerrainBuilder.CreateFlat(creation, new TerrainLayer[0], TestMapTerrainPath);
            if (data == null)
                return;

            GameObject terrainObject = Terrain.CreateTerrainGameObject(data);
            terrainObject.name = "CarteTest";
            UseTerrainData(terrainObject.GetComponent<Terrain>(), data);
            EditorAssets.EnsureFolder("Assets/Scenes/Test");
            EditorSceneManager.SaveScene(scene, ScenePaths.MapTest);
            Debug.Log("DICIELA : carte de test créée dans " + ScenePaths.MapTest + ".");
        }

        /// <summary>Ne garde, à la racine, que le soleil, le volume de filtres et le Terrain ; rend ce Terrain.</summary>
        private static Terrain KeepSunFiltersAndTerrain(Scene scene)
        {
            Terrain terrain = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Terrain rootTerrain = root.GetComponent<Terrain>();
                Light light = root.GetComponent<Light>();
                bool keep = rootTerrain != null || (light != null && light.type == LightType.Directional) ||
                            FindFilters(root) != null;
                if (!keep)
                {
                    Object.DestroyImmediate(root);
                    continue;
                }
                if (terrain == null && rootTerrain != null)
                    terrain = rootTerrain;
            }
            return terrain;
        }

        /// <summary>Le Terrain posé à l'origine, relié à ses données et marqué comme racine de la carte.</summary>
        private static void UseTerrainData(Terrain terrain, TerrainData data)
        {
            terrain.terrainData = data;
            TerrainCollider collider = terrain.GetComponent<TerrainCollider>();
            if (collider != null)
                collider.terrainData = data;
            terrain.transform.position = Vector3.zero;
            if (terrain.GetComponent<MapRoot>() == null)
                terrain.gameObject.AddComponent<MapRoot>();
        }

        /// <summary>
        /// Le profil de filtres de la démo appartient au pack, où rien ne s'écrit : la carte en reçoit une copie dans
        /// Assets/Art, que le développeur peut retoucher.
        /// </summary>
        private static void CopyFiltersProfile(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                SerializedObject filters = FindFilters(root);
                if (filters == null)
                    continue;

                SerializedProperty profile = filters.FindProperty(VolumeProfileField);
                string sourcePath = AssetDatabase.GetAssetPath(profile.objectReferenceValue);
                if (string.IsNullOrEmpty(sourcePath))
                    continue;
                EditorAssets.EnsureFolder("Assets/Art");
                if (!EditorAssets.Exists(MapVolumeProfilePath))
                    AssetDatabase.CopyAsset(sourcePath, MapVolumeProfilePath);
                profile.objectReferenceValue = AssetDatabase.LoadMainAssetAtPath(MapVolumeProfilePath);
                filters.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        /// <summary>Le composant de volume de filtres de cet objet (reconnu à son profil), ou null.</summary>
        private static SerializedObject FindFilters(GameObject gameObject)
        {
            foreach (MonoBehaviour behaviour in gameObject.GetComponents<MonoBehaviour>())
            {
                if (behaviour == null)
                    continue;
                var serialized = new SerializedObject(behaviour);
                if (serialized.FindProperty(VolumeProfileField) != null)
                    return serialized;
            }
            return null;
        }
    }
}
