using System.Text;
using Diciela.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Compose les modèles faits d'objets du pack (PackComposition ; décision 170) : chacun devient un prefab de
    /// Assets/Art/Compositions, en instances des prefabs du pack (chacune garde son nom), créé s'il manque ; il devient le
    /// modèle source de sa construction si elle n'en a pas, et la construction est préparée. Ensuite chaque composition
    /// est au développeur : il la retouche dans Unity, puis relance ce menu ou « Préparer les modèles ». Il dit la taille
    /// de chaque objet, mesurée, pour aider à retoucher.
    /// </summary>
    internal static class CompositionBuilder
    {
        private const string Folder = "Assets/Art/Compositions";
        private const string DoorName = "Door";

        [MenuItem("DICIELA/Contenu/Composer le campement et le tas")]
        public static void ComposeAll()
        {
            foreach (PackComposition composition in PackComposition.All)
            {
                string path = Folder + "/" + composition.Name + ".prefab";
                if (!EditorAssets.Exists(path) && !Build(composition, path))
                    continue;
                var structure = EditorAssets.Load<StructureDefinition>(composition.StructurePath);
                EditorAssets.AssignIfMissing(structure, "_sourceModel", AssetDatabase.LoadAssetAtPath<GameObject>(path));
                ModelPreparer.Prepare(structure);
            }
            AssetDatabase.SaveAssets();
        }

        private static bool Build(PackComposition composition, string path)
        {
            EditorAssets.EnsureFolder(Folder);
            Scene scene = EditorSceneManager.NewPreviewScene();
            try
            {
                var root = new GameObject(composition.Name);
                SceneManager.MoveGameObjectToScene(root, scene);
                Transform door = null;
                var sizes = new StringBuilder();
                foreach (PackComposition.Piece piece in composition.Pieces)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(piece.Path);
                    if (prefab == null)
                    {
                        Debug.LogError("DICIELA : " + composition.Name + " ne trouve pas " + piece.Path + ".");
                        return false;
                    }
                    if (piece.AtTheDoor && door == null)
                    {
                        door = new GameObject(DoorName).transform;
                        door.SetParent(root.transform, false);
                    }
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                    instance.transform.SetParent(piece.AtTheDoor ? door : root.transform, false);
                    instance.transform.localPosition = piece.Place;
                    instance.transform.localRotation = Quaternion.Euler(piece.Pitch, piece.Yaw, 0f);
                    instance.transform.localScale = Vector3.one * piece.Scale;
                    if (instance.GetComponentInChildren<MeshFilter>() != null)
                    {
                        Vector3 size = PackMeasures.GeometryOf(prefab).Box.size * piece.Scale;
                        sizes.Append(" ; ").Append(prefab.name).Append(' ').Append(size.x.ToString("0.00"))
                            .Append(" × ").Append(size.z.ToString("0.00")).Append(" × ").Append(size.y.ToString("0.00"));
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log("DICIELA : " + composition.Name + " composé (" + path + "), largeur × profondeur × hauteur en " +
                          "mètres" + sizes + ".");
                return true;
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
