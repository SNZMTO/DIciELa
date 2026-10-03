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
    /// par matériau, enregistré avec son prefab dans Assets/Art/Modeles. Mêmes formes, mêmes matériaux du pack : l'art
    /// ne change pas, mais une maison se dessine en un appel par matériau au lieu d'un par pièce, et se crée d'un seul
    /// objet. Un maillage fait par script reste lisible : la visée pourra lire ses triangles (SAVOIR, 2.7).
    /// </summary>
    internal static class ModelMerger
    {
        public const string Folder = "Assets/Art/Modeles";

        /// <summary>
        /// Fusionne ces pièces, placées dans leur scène comme le modèle préparé doit l'être, et enregistre le modèle
        /// sous ce nom ; null si une pièce ne peut pas l'être (la raison est dans la console).
        /// </summary>
        public static GameObject Save(string name, List<MeshFilter> pieces, out string report)
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
            report = pieces.Count + " pièces fusionnées en un maillage de " + merged.vertexCount + " sommets, " +
                     Triangles(merged) + " triangles, " + materials.Count + " matériau" + (materials.Count > 1 ? "x" : "");
            return SavePrefab(name, merged, materials);
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

        /// <summary>Enregistre le maillage, puis le prefab qui le montre ; un modèle déjà préparé est remplacé au même chemin.</summary>
        private static GameObject SavePrefab(string name, Mesh merged, List<Material> materials)
        {
            EditorAssets.EnsureFolder(Folder);
            string meshPath = Folder + "/" + name + "_Maillage.asset";
            string prefabPath = Folder + "/" + name + ".prefab";
            // Un maillage déjà là garde son objet, qu'on réécrit : le recréer détruirait celui que le modèle chargé montre
            // encore, et ce modèle resterait sans maillage jusqu'à sa réimportation (SAVOIR, 3).
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(merged, meshPath);
            }
            else
            {
                Rewrite(existing, merged);
                Object.DestroyImmediate(merged);
            }

            Scene scene = EditorSceneManager.NewPreviewScene();
            try
            {
                var model = new GameObject(name);
                SceneManager.MoveGameObjectToScene(model, scene);
                model.AddComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                model.AddComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
                return PrefabUtility.SaveAsPrefabAsset(model, prefabPath);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        // Réécrit ce maillage avec le contenu de l'autre, par l'API des maillages : l'objet reste, et ses données changent
        // partout, jusqu'à ce que dessine la carte graphique (une copie de ses données enregistrées ne la rafraîchit pas).
        private static void Rewrite(Mesh target, Mesh source)
        {
            target.Clear();
            target.indexFormat = source.indexFormat;
            target.SetVertices(source.vertices);
            if (source.HasVertexAttribute(VertexAttribute.Normal))
                target.SetNormals(source.normals);
            if (source.HasVertexAttribute(VertexAttribute.Tangent))
                target.SetTangents(source.tangents);
            if (source.HasVertexAttribute(VertexAttribute.Color))
            {
                if (source.GetVertexAttributeFormat(VertexAttribute.Color) == VertexAttributeFormat.UNorm8)
                    target.SetColors(source.colors32);
                else
                    target.SetColors(source.colors);
            }
            for (int channel = 0; channel < 8; channel++)
                CopyUVs(source, target, channel);
            target.subMeshCount = source.subMeshCount;
            for (int sub = 0; sub < source.subMeshCount; sub++)
                target.SetTriangles(source.GetTriangles(sub), sub);
            target.RecalculateBounds();
            EditorUtility.SetDirty(target);
        }

        // Un canal de coordonnées de texture, recopié avec son nombre de composantes : deux le plus souvent.
        private static void CopyUVs(Mesh source, Mesh target, int channel)
        {
            VertexAttribute attribute = VertexAttribute.TexCoord0 + channel;
            if (!source.HasVertexAttribute(attribute))
                return;
            switch (source.GetVertexAttributeDimension(attribute))
            {
                case 2:
                    var flat = new List<Vector2>();
                    source.GetUVs(channel, flat);
                    target.SetUVs(channel, flat);
                    break;
                case 3:
                    var deep = new List<Vector3>();
                    source.GetUVs(channel, deep);
                    target.SetUVs(channel, deep);
                    break;
                default:
                    var full = new List<Vector4>();
                    source.GetUVs(channel, full);
                    target.SetUVs(channel, full);
                    break;
            }
        }
    }
}
