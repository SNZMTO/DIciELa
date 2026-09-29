using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Une paire d'oreilles en deux moitiés (SAVOIR, 2.11) : les triangles d'un modèle, dans ses axes, partagés selon le
    /// côté de leur centre (à droite ou à gauche du milieu), en deux maillages enfants d'un même prefab, le pivot à la
    /// même place ; l'habillage écarte chaque moitié selon les oreilles de la tête choisie (PiecePlacement). Les oreilles
    /// découpées d'une tête s'enregistrent ainsi ; une paire du pack est copiée ainsi, dans Assets/Art/Personnages/Pieces :
    /// le pack ne change pas. Réécrites à chaque passage, sous le même nom.
    /// </summary>
    internal static class EarHalves
    {
        private const string Folder = "Assets/Art/Personnages/Pieces";

        /// <summary>La paire de ce maillage, de ces matériaux (un par sous-maillage), enregistrée en deux moitiés.</summary>
        public static GameObject Save(Mesh mesh, Material[] materials, string name, Quaternion rotation)
        {
            EditorAssets.EnsureFolder(Folder);
            var pair = new GameObject(name);
            try
            {
                pair.transform.localRotation = rotation;
                foreach (float side in new[] { 1f, -1f })
                {
                    string half = side > 0f ? "Droite" : "Gauche";
                    Mesh saved = BodyCutter.Save(Half(mesh, side, name + "_" + half),
                        Folder + "/" + name + "_" + half + ".asset");
                    var child = new GameObject(half);
                    child.transform.SetParent(pair.transform, false);
                    child.AddComponent<MeshFilter>().sharedMesh = saved;
                    child.AddComponent<MeshRenderer>().sharedMaterials = materials;
                }
                return PrefabUtility.SaveAsPrefabAsset(pair, Folder + "/" + name + ".prefab");
            }
            finally
            {
                Object.DestroyImmediate(pair);
            }
        }

        /// <summary>Copie en deux moitiés la paire de ce modèle du pack (tous ses rendus, dans ses axes), sous ce nom.</summary>
        public static GameObject Copy(GameObject model, string name)
        {
            (Mesh mesh, Material[] materials, Quaternion rotation) = PackCopy.Flatten(model, name);
            GameObject pair = Save(mesh, materials, name, rotation);
            Object.DestroyImmediate(mesh);
            return pair;
        }

        // Les triangles de ce côté (le centre de chacun), sous-maillage par sous-maillage, avec seulement leurs sommets.
        private static Mesh Half(Mesh mesh, float side, string name)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            var uvs = new List<Vector2>();
            mesh.GetUVs(0, uvs);
            var map = new Dictionary<int, int>();
            var positions = new List<Vector3>();
            var keptNormals = new List<Vector3>();
            var keptUvs = new List<Vector2>();
            var half = new Mesh { name = name, subMeshCount = mesh.subMeshCount };
            var parts = new List<int>[mesh.subMeshCount];
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                parts[sub] = new List<int>();
                int[] triangles = mesh.GetTriangles(sub);
                for (int t = 0; t < triangles.Length; t += 3)
                {
                    float x = vertices[triangles[t]].x + vertices[triangles[t + 1]].x + vertices[triangles[t + 2]].x;
                    if (x * side <= 0f)
                        continue;
                    for (int k = 0; k < 3; k++)
                    {
                        int index = triangles[t + k];
                        if (!map.TryGetValue(index, out int kept))
                        {
                            map[index] = kept = positions.Count;
                            positions.Add(vertices[index]);
                            keptNormals.Add(normals.Length == vertices.Length ? normals[index] : Vector3.up);
                            keptUvs.Add(uvs.Count == vertices.Length ? uvs[index] : Vector2.zero);
                        }
                        parts[sub].Add(kept);
                    }
                }
            }
            half.SetVertices(positions);
            half.SetNormals(keptNormals);
            half.SetUVs(0, keptUvs);
            for (int sub = 0; sub < parts.Length; sub++)
                half.SetTriangles(parts[sub], sub);
            half.RecalculateBounds();
            return half;
        }
    }
}
