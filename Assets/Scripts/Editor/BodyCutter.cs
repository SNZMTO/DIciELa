using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Sépare le modèle d'un animal du pack en deux maillages à peau, son corps et son crâne (SAVOIR, 2.11) : dans le
    /// pack, la tête est déjà un ensemble d'morceaux qu'aucun triangle ne relie au corps (HeadShells). Le crâne garde ses
    /// yeux, ses sourcils et ses moustaches ; ses oreilles, ses touffes et son museau détachables deviennent des pièces
    /// (HeadPartCutter). Les deux maillages gardent le squelette commun (poids et poses d'origine) et sont enregistrés dans
    /// Assets/Art/Personnages, refaits en place à chaque passage : même fichier, mêmes références.
    /// </summary>
    internal static class BodyCutter
    {
        public const string Folder = "Assets/Art/Personnages";

        /// <summary>Le crâne et le corps de ce personnage ; name nomme leurs fichiers (Chat_A).</summary>
        public static (Mesh Skull, Mesh Body) Cut(SkinnedMeshRenderer skin, HeadShells shells, string name)
        {
            EditorAssets.EnsureFolder(Folder);
            Mesh skull = Save(Part(skin.sharedMesh, shells.TrianglesOf(HeadShells.Kind.Skull), name + "_Tete"),
                Folder + "/" + name + "_Tete.asset");
            Mesh body = Save(Part(skin.sharedMesh, shells.TrianglesOf(HeadShells.Kind.Body), name + "_Corps"),
                Folder + "/" + name + "_Corps.asset");
            return (skull, body);
        }

        /// <summary>Enregistre ce maillage à ce chemin ; s'il y en a déjà un, le remplace en place (même GUID).</summary>
        public static Mesh Save(Mesh mesh, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            // Copié champ par champ : une copie sérialisée d'un maillage garde l'ancien découpage de ses sommets.
            existing.Clear();
            existing.name = mesh.name;
            existing.indexFormat = mesh.indexFormat;
            existing.SetVertices(mesh.vertices);
            existing.SetNormals(mesh.normals);
            existing.SetTangents(mesh.tangents);
            existing.SetUVs(0, mesh.uv);
            existing.boneWeights = mesh.boneWeights;
            existing.bindposes = mesh.bindposes;
            existing.subMeshCount = mesh.subMeshCount;
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
                existing.SetTriangles(mesh.GetTriangles(sub), sub);
            existing.RecalculateBounds();
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        /// <summary>
        /// Un maillage à peau fait de ces triangles de la source, sous-maillage par sous-maillage : seuls les sommets
        /// qu'ils touchent, avec leurs normales, tangentes, coordonnées de texture et poids ; les poses d'origine des os.
        /// </summary>
        private static Mesh Part(Mesh source, List<int>[] triangles, string name)
        {
            Vector3[] vertices = source.vertices;
            Vector3[] normals = source.normals;
            Vector4[] tangents = source.tangents;
            BoneWeight[] weights = source.boneWeights;
            var uvs = new List<Vector2>();
            source.GetUVs(0, uvs);

            (List<int> kept, List<int>[] parts) = Compact(vertices.Length, triangles);
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

        /// <summary>
        /// Les sommets que touchent ces triangles, dans l'ordre où on les rencontre, et les triangles renumérotés sur eux.
        /// </summary>
        public static (List<int> Kept, List<int>[] Parts) Compact(int vertexCount, List<int>[] triangles)
        {
            var remap = new int[vertexCount];
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
            return (kept, parts);
        }

        public static List<T> Pick<T>(T[] values, List<int> kept)
        {
            var picked = new List<T>(kept.Count);
            foreach (int index in kept)
                picked.Add(values[index]);
            return picked;
        }
    }
}
