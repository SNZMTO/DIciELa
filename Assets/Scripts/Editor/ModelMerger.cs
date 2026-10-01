using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La fusion d'un modèle préparé (SAVOIR, 2.9) : toutes ses pièces deviennent un seul maillage, avec un sous-maillage
    /// par matériau, enregistré avec son prefab dans Assets/Art/Modeles ; ses effets (des particules) y sont copiés à
    /// leur place, à côté du maillage. Mêmes formes, mêmes matériaux du pack : l'art
    /// ne change pas, mais une maison se dessine en un appel par matériau au lieu d'un par pièce, et se crée d'un seul
    /// objet. Un maillage fait par script reste lisible : la visée pourra lire ses triangles (SAVOIR, 2.7).
    /// </summary>
    internal static class ModelMerger
    {
        public const string Folder = "Assets/Art/Modeles";

        /// <summary>
        /// Fusionne ces pièces, placées dans leur scène comme le modèle préparé doit l'être, et enregistre le modèle
        /// sous ce nom, avec une copie de ces effets, placés de même ; null si une pièce ne peut pas l'être (la raison est
        /// dans la console).
        /// </summary>
        public static GameObject Save(string name, List<MeshFilter> pieces, List<GameObject> effects, out string report)
        {
            report = "";
            var materials = new List<Material>();
            var parts = new List<List<CombineInstance>>();
            foreach (MeshFilter piece in pieces)
            {
                Matrix4x4 matrix = piece.transform.localToWorldMatrix;
                if (matrix.determinant < 0f)
                {
                    Debug.LogError("DICIELA : " + name + " a une pièce en miroir (" + piece.name + ") : la fusion " +
                                   "retournerait ses faces, elle ne sait pas encore le faire.");
                    return null;
                }
                Material[] shared = piece.GetComponent<MeshRenderer>().sharedMaterials;
                Mesh mesh = piece.sharedMesh;
                for (int sub = 0; sub < mesh.subMeshCount && sub < shared.Length; sub++)
                {
                    if (shared[sub] == null)
                        continue;
                    int index = materials.IndexOf(shared[sub]);
                    if (index < 0)
                    {
                        materials.Add(shared[sub]);
                        parts.Add(new List<CombineInstance>());
                        index = materials.Count - 1;
                    }
                    parts[index].Add(new CombineInstance { mesh = mesh, subMeshIndex = sub, transform = matrix });
                }
            }

            Mesh merged = Merge(name, parts);
            GameObject prefab = SavePrefab(name, merged, materials, effects);
            report = pieces.Count + " pièces fusionnées en un maillage de " + merged.vertexCount + " sommets, " +
                     Triangles(merged) + " triangles, " + materials.Count + " matériau" + (materials.Count > 1 ? "x" : "") +
                     (effects.Count > 0 ? ", " + effects.Count + " effet" + (effects.Count > 1 ? "s" : "") + " gardé" +
                                          (effects.Count > 1 ? "s" : "") : "");
            return prefab;
        }

        private static long Triangles(Mesh mesh)
        {
            long indices = 0;
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
                indices += (long)mesh.GetIndexCount(sub);
            return indices / 3;
        }

        /// <summary>Un maillage par matériau, puis un seul maillage dont chacun devient un sous-maillage, dans l'ordre.</summary>
        private static Mesh Merge(string name, List<List<CombineInstance>> parts)
        {
            var byMaterial = new CombineInstance[parts.Count];
            int vertices = 0;
            for (int i = 0; i < parts.Count; i++)
            {
                var part = new Mesh { indexFormat = IndexFormat.UInt32 };
                part.CombineMeshes(parts[i].ToArray(), true, true);
                vertices += part.vertexCount;
                byMaterial[i] = new CombineInstance { mesh = part, transform = Matrix4x4.identity };
            }

            var merged = new Mesh
            {
                name = name + "_Maillage",
                indexFormat = vertices > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            merged.CombineMeshes(byMaterial, false, false);
            merged.RecalculateBounds();
            foreach (CombineInstance part in byMaterial)
                Object.DestroyImmediate(part.mesh);
            return merged;
        }

        /// <summary>
        /// Enregistre le maillage, puis le prefab qui le montre, avec ses effets ; un modèle déjà préparé est remplacé au
        /// même chemin. Le modèle est à l'origine : la place d'un effet dans la scène de la préparation est sa place sous lui.
        /// </summary>
        private static GameObject SavePrefab(string name, Mesh merged, List<Material> materials, List<GameObject> effects)
        {
            EditorAssets.EnsureFolder(Folder);
            string meshPath = Folder + "/" + name + "_Maillage.asset";
            string prefabPath = Folder + "/" + name + ".prefab";
            AssetDatabase.CreateAsset(merged, meshPath);

            Scene scene = EditorSceneManager.NewPreviewScene();
            try
            {
                var model = new GameObject(name);
                SceneManager.MoveGameObjectToScene(model, scene);
                model.AddComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                model.AddComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
                foreach (GameObject effect in effects)
                {
                    // Une instance du prefab d'origine (celui du pack), jamais la copie d'un objet de la scène de la
                    // préparation, qui est lui-même la partie d'une instance.
                    GameObject original = PrefabUtility.GetCorrespondingObjectFromOriginalSource(effect);
                    GameObject copy = original != null
                        ? (GameObject)PrefabUtility.InstantiatePrefab(original, model.transform)
                        : Object.Instantiate(effect, model.transform);
                    copy.name = effect.name;
                    copy.transform.SetPositionAndRotation(effect.transform.position, effect.transform.rotation);
                    copy.transform.localScale = effect.transform.lossyScale;
                }
                return PrefabUtility.SaveAsPrefabAsset(model, prefabPath);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
