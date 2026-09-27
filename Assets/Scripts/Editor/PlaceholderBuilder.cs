using Diciela.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Le remplaçant d'une construction dont le modèle n'est pas encore choisi (l'art est au développeur) : une boîte à
    /// la taille de son empreinte, et sa porte marquée sur la façade nord, pour lire sa taille et son sens. Il se monte
    /// dans une scène d'aperçu, hors de la scène ouverte.
    /// </summary>
    internal static class PlaceholderBuilder
    {
        private const string Folder = "Assets/Art/Remplacants";
        private const string BodyMaterialPath = Folder + "/Remplacant_Corps.mat";
        private const string DoorMaterialPath = Folder + "/Remplacant_Porte.mat";

        // La hauteur d'une boîte et de sa porte, en mètres : de quoi lire la taille et le sens, sans plus.
        private const float BoxHeight = 3f;
        private const float DoorHeight = 2.2f;
        private const float DoorThickness = 0.1f;

        private static readonly Color BodyColor = new Color(0.86f, 0.8f, 0.68f);
        private static readonly Color DoorColor = new Color(0.36f, 0.24f, 0.14f);

        /// <summary>Le remplaçant de cette construction, créé s'il manque.</summary>
        public static GameObject CreateBoxIfMissing(StructureDefinition definition)
        {
            string path = Folder + "/" + definition.name + "_Remplacant.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
                return existing;

            Material body = MaterialBuilder.OpaqueIfMissing(BodyMaterialPath, BodyColor);
            Material door = MaterialBuilder.OpaqueIfMissing(DoorMaterialPath, DoorColor);
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var root = new GameObject(definition.name + "_Remplacant");
                SceneManager.MoveGameObjectToScene(root, preview);
                float width = definition.WidthMeters, depth = definition.DepthMeters;
                AddBox(root, "Corps", new Vector3(0f, BoxHeight * 0.5f, 0f), new Vector3(width, BoxHeight, depth), body);
                if (definition.HasDoor)
                {
                    AddBox(root, "Porte",
                        new Vector3(definition.DoorOffsetMeters, DoorHeight * 0.5f, (depth + DoorThickness) * 0.5f),
                        new Vector3(definition.DoorWidthMeters, DoorHeight, DoorThickness), door);
                }

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log("DICIELA : remplaçant créé : " + path + " (une boîte, en attendant le modèle).");
                return prefab;
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        private static void AddBox(GameObject parent, string name, Vector3 position, Vector3 size, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent.transform, false);
            box.transform.localPosition = position;
            box.transform.localScale = size;
            box.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(box.GetComponent<Collider>());
        }
    }
}
