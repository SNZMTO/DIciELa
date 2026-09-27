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
        private const string BodyName = "Corps";
        private const string DoorName = "Porte";

        // La hauteur d'une boîte et de sa porte, en mètres : de quoi lire la taille et le sens, sans plus.
        private const float BoxHeight = 3f;
        private const float DoorHeight = 2.2f;
        private const float DoorThickness = 0.1f;

        private static readonly Color BodyColor = new Color(0.86f, 0.8f, 0.68f);
        private static readonly Color DoorColor = new Color(0.36f, 0.24f, 0.14f);

        /// <summary>Vrai si ce modèle est un remplaçant : il vit dans le dossier des remplaçants.</summary>
        public static bool IsPlaceholder(GameObject model)
        {
            return model != null && AssetDatabase.GetAssetPath(model).StartsWith(Folder + "/");
        }

        /// <summary>
        /// Le remplaçant de cette construction, à la taille de sa fiche : créé s'il manque, refait si la fiche a changé.
        /// Le fichier reste le même, donc la fiche garde son modèle.
        /// </summary>
        public static GameObject EnsureBox(StructureDefinition definition)
        {
            string path = Folder + "/" + definition.name + "_Remplacant.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null && Matches(existing, definition))
                return existing;

            Material body = MaterialBuilder.OpaqueIfMissing(BodyMaterialPath, BodyColor);
            Material door = MaterialBuilder.OpaqueIfMissing(DoorMaterialPath, DoorColor);
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var root = new GameObject(definition.name + "_Remplacant");
                SceneManager.MoveGameObjectToScene(root, preview);
                AddBox(root, BodyName, BodyPosition(), BodySize(definition), body);
                if (definition.HasDoor)
                    AddBox(root, DoorName, DoorPosition(definition), DoorSize(definition), door);

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log("DICIELA : remplaçant " + (existing != null ? "refait à la taille de sa fiche" : "créé") + " : " +
                          path + " (une boîte, en attendant le modèle).");
                return prefab;
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        /// <summary>Vrai si la boîte a encore la taille et la porte de la fiche.</summary>
        private static bool Matches(GameObject box, StructureDefinition definition)
        {
            Transform body = box.transform.Find(BodyName);
            Transform door = box.transform.Find(DoorName);
            if (body == null || body.localPosition != BodyPosition() || body.localScale != BodySize(definition))
                return false;
            if (definition.HasDoor != (door != null))
                return false;
            return door == null || (door.localPosition == DoorPosition(definition) && door.localScale == DoorSize(definition));
        }

        private static Vector3 BodyPosition()
        {
            return new Vector3(0f, BoxHeight * 0.5f, 0f);
        }

        private static Vector3 BodySize(StructureDefinition definition)
        {
            return new Vector3(definition.WidthMeters, BoxHeight, definition.DepthMeters);
        }

        private static Vector3 DoorPosition(StructureDefinition definition)
        {
            return new Vector3(definition.DoorOffsetMeters, DoorHeight * 0.5f, (definition.DepthMeters + DoorThickness) * 0.5f);
        }

        private static Vector3 DoorSize(StructureDefinition definition)
        {
            return new Vector3(definition.DoorWidthMeters, DoorHeight, DoorThickness);
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
