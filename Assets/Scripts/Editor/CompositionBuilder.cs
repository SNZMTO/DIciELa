using System.Text;
using Diciela.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Compose les modèles faits d'objets du pack (PackComposition ; décisions 170, 172 et 193) : chacun devient un prefab
    /// de Assets/Art/Compositions, en instances des prefabs du pack (chacune garde son nom), et son dallage s'il en a un
    /// (son maillage et sa matière, faits une fois à côté de lui), créé s'il manque ; un repère de couche qui
    /// lui manque y est ajouté ; il devient le modèle source de sa construction si elle n'en a pas, et la construction
    /// est préparée. Ensuite chaque composition est au développeur : il la retouche dans Unity, puis relance ce menu ou
    /// « Préparer les modèles ». Il dit la taille de chaque objet, mesurée, pour aider à retoucher.
    /// </summary>
    internal static class CompositionBuilder
    {
        private const string Folder = "Assets/Art/Compositions";
        private const string DoorName = "Door";

        [MenuItem("DICIELA/Contenu/Composer les modèles")]
        public static void ComposeAll()
        {
            foreach (PackComposition composition in PackComposition.All)
            {
                string path = Folder + "/" + composition.Name + ".prefab";
                if (!EditorAssets.Exists(path) && !Build(composition, path))
                    continue;
                AddCouchIfMissing(composition, path);
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
                if (composition.Paving != null && !AddPaving(composition, root.transform))
                    return false;
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
                    bool measured = instance.GetComponentInChildren<MeshFilter>() != null;
                    Bounds box = measured ? PackMeasures.GeometryOf(prefab).Box : default;
                    piece.Pose(instance.transform, piece.AtTheDoor ? door : root.transform, box);
                    if (measured)
                    {
                        Vector3 size = box.size * piece.Scale;
                        sizes.Append(" ; ").Append(prefab.name).Append(' ').Append(size.x.ToString("0.00"))
                            .Append(" × ").Append(size.z.ToString("0.00")).Append(" × ").Append(size.y.ToString("0.00"));
                    }
                }
                if (composition.Couch.HasValue)
                    AddCouch(root.transform, composition.Couch.Value);
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

        // Le dallage d'une composition : son maillage et sa matière, faits une fois à côté d'elle (FloorMesh, et une copie
        // de la matière du pack portant sa texture), puis gardés tels quels : le développeur peut les retoucher.
        private static bool AddPaving(PackComposition composition, Transform root)
        {
            PackComposition.Floor floor = composition.Paving;
            Material material = PavingMaterial(composition.Name, floor);
            if (material == null)
                return false;
            string meshPath = Folder + "/" + composition.Name + "_Dallage.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null)
            {
                mesh = FloorMesh.Create(composition.Name + "_Dallage", floor.Diameter, floor.Top, floor.Skirt, floor.Pattern);
                AssetDatabase.CreateAsset(mesh, meshPath);
            }
            var paving = new GameObject("Dallage", typeof(MeshFilter), typeof(MeshRenderer));
            paving.transform.SetParent(root, false);
            paving.GetComponent<MeshFilter>().sharedMesh = mesh;
            paving.GetComponent<MeshRenderer>().sharedMaterial = material;
            return true;
        }

        // La matière du dallage : celle déjà faite, sinon une copie de la matière du pack portant sa texture et ses reliefs.
        private static Material PavingMaterial(string name, PackComposition.Floor floor)
        {
            string path = Folder + "/" + name + "_Dallage.mat";
            var made = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (made != null)
                return made;
            Material material = floor.Dressed(name + "_Dallage");
            if (material == null)
            {
                Debug.LogError("DICIELA : " + name + " ne trouve pas la matière ou les textures de son dallage dans le pack.");
                return null;
            }
            AssetDatabase.CreateAsset(material, path);
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        // Une composition déjà faite (retouchée, peut-être, par le développeur) reçoit le repère de sa couche s'il lui manque.
        private static void AddCouchIfMissing(PackComposition composition, string path)
        {
            if (!composition.Couch.HasValue)
                return;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (SourceModelParts.Couch(root) != null)
                    return;
                AddCouch(root.transform, composition.Couch.Value);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log("DICIELA : " + composition.Name + " reçoit le repère de sa couche (« " + SourceModelParts.CouchName +
                          " »), à déplacer à la main si la Chimère y dort mal.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AddCouch(Transform root, PackComposition.Spot couch)
        {
            var mark = new GameObject(SourceModelParts.CouchName).transform;
            mark.SetParent(root, false);
            mark.localPosition = couch.Place;
            mark.localRotation = Quaternion.Euler(0f, couch.Yaw, 0f);
        }
    }
}
