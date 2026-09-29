using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Un modèle du pack aplati en un seul maillage, dans ses axes (SAVOIR, 2.11) : tous ses rendus, sous-maillage par
    /// sous-maillage, avec leurs matériaux, et la rotation de sa racine. Sert aux copies qu'on modifie hors du pack (une
    /// paire d'oreilles en deux moitiés, une barbe teinte comme le poil) : le pack ne change pas.
    /// </summary>
    internal static class PackCopy
    {
        /// <summary>Le maillage aplati de ce modèle (à détruire par l'appelant), ses matériaux et sa rotation.</summary>
        public static (Mesh Mesh, Material[] Materials, Quaternion Rotation) Flatten(GameObject model, string name)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(model));
            try
            {
                var positions = new List<Vector3>();
                var normals = new List<Vector3>();
                var uvs = new List<Vector2>();
                var parts = new List<List<int>>();
                var materials = new List<Material>();
                Matrix4x4 toModel = root.transform.worldToLocalMatrix;
                foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(false))
                {
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null)
                        continue;
                    Mesh source = filter.sharedMesh;
                    Matrix4x4 toRoot = toModel * renderer.transform.localToWorldMatrix;
                    int first = positions.Count;
                    foreach (Vector3 vertex in source.vertices)
                        positions.Add(toRoot.MultiplyPoint3x4(vertex));
                    foreach (Vector3 normal in source.normals)
                        normals.Add(toRoot.MultiplyVector(normal).normalized);
                    var sourceUvs = new List<Vector2>();
                    source.GetUVs(0, sourceUvs);
                    uvs.AddRange(sourceUvs.Count == source.vertexCount ? sourceUvs : new List<Vector2>(new Vector2[source.vertexCount]));
                    for (int sub = 0; sub < source.subMeshCount; sub++)
                    {
                        var triangles = new List<int>();
                        foreach (int index in source.GetTriangles(sub))
                            triangles.Add(first + index);
                        parts.Add(triangles);
                        materials.Add(renderer.sharedMaterials[Mathf.Min(sub, renderer.sharedMaterials.Length - 1)]);
                    }
                }
                var mesh = new Mesh { name = name };
                mesh.SetVertices(positions);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.subMeshCount = parts.Count;
                for (int sub = 0; sub < parts.Count; sub++)
                    mesh.SetTriangles(parts[sub], sub);
                return (mesh, materials.ToArray(), root.transform.localRotation);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
