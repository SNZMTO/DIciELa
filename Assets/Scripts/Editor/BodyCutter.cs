using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Sépare le modèle d'un animal du pack en deux maillages, sa tête et son corps (SAVOIR, 2.11). Dans le pack, la tête
    /// est déjà une pièce à part, qu'aucun triangle ne relie au corps : un triangle est à la tête quand ses sommets suivent
    /// en moyenne pour moitié au moins les os de la tête (head, eyes, eyebrows). Les deux maillages gardent le squelette
    /// commun (poids et poses d'origine) et sont enregistrés dans Assets/Art/Personnages. Un maillage déjà découpé n'est
    /// pas refait : le pack ne change pas.
    /// </summary>
    internal static class BodyCutter
    {
        public const string Folder = "Assets/Art/Personnages";
        private const float HeadShare = 0.5f;
        private static readonly HashSet<string> HeadBones = new HashSet<string> { "head", "eyes", "eyebrows" };

        /// <summary>La tête et le corps de ce personnage, découpés s'ils manquent ; name nomme leurs fichiers (Chat_A).</summary>
        public static (Mesh Head, Mesh Body) CutIfMissing(GameObject character, string name)
        {
            string headPath = Folder + "/" + name + "_Tete.asset";
            string bodyPath = Folder + "/" + name + "_Corps.asset";
            if (EditorAssets.Exists(headPath) && EditorAssets.Exists(bodyPath))
                return (EditorAssets.Load<Mesh>(headPath), EditorAssets.Load<Mesh>(bodyPath));

            SkinnedMeshRenderer skin = VisibleBody(character);
            Mesh source = skin.sharedMesh;
            float[] head = HeadWeights(source, skin.bones);
            var headParts = new List<int>[source.subMeshCount];
            var bodyParts = new List<int>[source.subMeshCount];
            for (int sub = 0; sub < source.subMeshCount; sub++)
            {
                headParts[sub] = new List<int>();
                bodyParts[sub] = new List<int>();
                int[] triangles = source.GetTriangles(sub);
                for (int t = 0; t < triangles.Length; t += 3)
                {
                    float share = (head[triangles[t]] + head[triangles[t + 1]] + head[triangles[t + 2]]) / 3f;
                    List<int> part = share >= HeadShare ? headParts[sub] : bodyParts[sub];
                    part.Add(triangles[t]);
                    part.Add(triangles[t + 1]);
                    part.Add(triangles[t + 2]);
                }
            }

            EditorAssets.EnsureFolder(Folder);
            AssetDatabase.CreateAsset(Part(source, headParts, name + "_Tete"), headPath);
            AssetDatabase.CreateAsset(Part(source, bodyParts, name + "_Corps"), bodyPath);
            Debug.Log("DICIELA : " + name + " découpé en tête et corps (" + Folder + ").");
            return (EditorAssets.Load<Mesh>(headPath), EditorAssets.Load<Mesh>(bodyPath));
        }

        /// <summary>Le corps allumé du personnage : le maillage à peau, enfant direct du prefab, qui se montre.</summary>
        public static SkinnedMeshRenderer VisibleBody(GameObject character)
        {
            foreach (Transform child in character.transform)
            {
                var skin = child.GetComponent<SkinnedMeshRenderer>();
                if (skin != null && child.gameObject.activeSelf)
                    return skin;
            }
            throw new System.InvalidOperationException("DICIELA : " + character.name + " n'a pas de corps allumé.");
        }

        /// <summary>Pour chaque sommet, la part de son poids que portent les os de la tête.</summary>
        private static float[] HeadWeights(Mesh source, Transform[] bones)
        {
            var isHead = new bool[bones.Length];
            for (int i = 0; i < bones.Length; i++)
                isHead[i] = bones[i] != null && HeadBones.Contains(bones[i].name);
            BoneWeight[] weights = source.boneWeights;
            var head = new float[weights.Length];
            for (int v = 0; v < weights.Length; v++)
            {
                BoneWeight w = weights[v];
                head[v] = (isHead[w.boneIndex0] ? w.weight0 : 0f) + (isHead[w.boneIndex1] ? w.weight1 : 0f) +
                          (isHead[w.boneIndex2] ? w.weight2 : 0f) + (isHead[w.boneIndex3] ? w.weight3 : 0f);
            }
            return head;
        }

        /// <summary>
        /// Un maillage fait de ces triangles de la source, sous-maillage par sous-maillage : seuls les sommets qu'ils
        /// touchent, avec leurs normales, tangentes, coordonnées de texture et poids ; les poses d'origine des os.
        /// </summary>
        private static Mesh Part(Mesh source, List<int>[] triangles, string name)
        {
            Vector3[] vertices = source.vertices;
            Vector3[] normals = source.normals;
            Vector4[] tangents = source.tangents;
            BoneWeight[] weights = source.boneWeights;
            var uvs = new List<Vector2>();
            source.GetUVs(0, uvs);

            var remap = new int[vertices.Length];
            for (int i = 0; i < remap.Length; i++)
                remap[i] = -1;
            var kept = new List<int>();
            var parts = new List<int>[triangles.Length];
            for (int sub = 0; sub < triangles.Length; sub++)
            {
                parts[sub] = new List<int>(triangles[sub].Count);
                foreach (int index in triangles[sub])
                {
                    if (remap[index] < 0)
                    {
                        remap[index] = kept.Count;
                        kept.Add(index);
                    }
                    parts[sub].Add(remap[index]);
                }
            }

            var mesh = new Mesh
            {
                name = name,
                indexFormat = kept.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(Pick(vertices, kept));
            if (normals.Length == vertices.Length)
                mesh.SetNormals(Pick(normals, kept));
            if (tangents.Length == vertices.Length)
                mesh.SetTangents(Pick(tangents, kept));
            if (uvs.Count == vertices.Length)
                mesh.SetUVs(0, Pick(uvs.ToArray(), kept));
            mesh.boneWeights = Pick(weights, kept).ToArray();
            mesh.bindposes = source.bindposes;
            mesh.subMeshCount = parts.Length;
            for (int sub = 0; sub < parts.Length; sub++)
                mesh.SetTriangles(parts[sub], sub);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static List<T> Pick<T>(T[] values, List<int> kept)
        {
            var picked = new List<T>(kept.Count);
            foreach (int index in kept)
                picked.Add(values[index]);
            return picked;
        }
    }
}
