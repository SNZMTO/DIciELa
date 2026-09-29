using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les mesures des personnages et des pièces du pack, dans leur pose d'origine, chargés à part sans toucher la scène
    /// ouverte : la place des os d'un personnage ; les triangles d'une pièce, ses maillages allumés dans ses propres axes
    /// (ceux à peau pris dans leur pose), et leur boîte autour de son pivot. Les monteurs s'en servent pour mesurer la
    /// face d'une pièce qui touche et son réglage de départ (SAVOIR, 2.11).
    /// </summary>
    internal static class PackMeasures
    {
        /// <summary>Les triangles d'une pièce : leurs sommets, dans les axes de la pièce, et leurs indices.</summary>
        public readonly struct Geometry
        {
            public Geometry(Vector3[] positions, List<int> triangles)
            {
                Positions = positions;
                Triangles = triangles;
                // La boîte se construit dans une variable : une propriété de structure se lit par copie.
                var box = new Bounds(positions.Length > 0 ? positions[0] : Vector3.zero, Vector3.zero);
                foreach (Vector3 position in positions)
                    box.Encapsulate(position);
                Box = box;
            }

            public Vector3[] Positions { get; }
            public List<int> Triangles { get; }
            public Bounds Box { get; }
        }

        /// <summary>La place de chaque os de ce personnage.</summary>
        public static Dictionary<string, Vector3> Bones(GameObject character)
        {
            var bones = new Dictionary<string, Vector3>();
            GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(character));
            try
            {
                foreach (Transform bone in root.GetComponentsInChildren<Transform>(true))
                    bones[bone.name] = bone.position;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return bones;
        }

        /// <summary>Les triangles des maillages allumés d'une pièce, dans ses axes.</summary>
        public static Geometry GeometryOf(GameObject piece)
        {
            var positions = new List<Vector3>();
            var triangles = new List<int>();
            GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(piece));
            try
            {
                Matrix4x4 toPiece = root.transform.worldToLocalMatrix;
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(false))
                {
                    Mesh mesh = MeshOf(renderer, out bool baked);
                    if (mesh == null)
                        continue;
                    // Un maillage à peau cuit dans sa pose, sans l'échelle de son rendu, a ses sommets dans les axes du rendu.
                    Matrix4x4 toRoot = toPiece * renderer.transform.localToWorldMatrix;
                    int first = positions.Count;
                    foreach (Vector3 vertex in mesh.vertices)
                        positions.Add(toRoot.MultiplyPoint3x4(vertex));
                    foreach (int index in mesh.triangles)
                        triangles.Add(first + index);
                    if (baked)
                        Object.DestroyImmediate(mesh);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return new Geometry(positions.ToArray(), triangles);
        }

        private static Mesh MeshOf(Renderer renderer, out bool baked)
        {
            baked = false;
            if (renderer is SkinnedMeshRenderer skin)
            {
                if (skin.sharedMesh == null)
                    return null;
                var mesh = new Mesh();
                skin.BakeMesh(mesh, false);
                baked = true;
                return mesh;
            }
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            return filter != null ? filter.sharedMesh : null;
        }
    }
}
