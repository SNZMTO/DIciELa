using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Détache d'une tête du pack ses oreilles, ses touffes ou son museau (HeadShells) en une pièce rigide, comme celles
    /// de Chr_Attach : ces morceaux ne suivent que les os de la tête, un maillage sans peau accroché à l'os head les porte
    /// aussi bien. Le maillage est debout, dans les axes du personnage, son pivot à sa base (le bas des oreilles et des
    /// touffes, l'arrière du museau) ; il garde ses normales et ses coordonnées de texture. Enregistré avec son prefab dans
    /// Assets/Art/Personnages/Pieces, refait en place à chaque passage.
    /// </summary>
    internal static class HeadPartCutter
    {
        public const string Folder = BodyCutter.Folder + "/Pieces";
        // Deux pièces sont la même à 5 mm près : les oreilles des chats A et B ne diffèrent que d'un millimètre, invisible.
        private const float SameMeters = 0.005f;

        /// <summary>Une pièce détachée, pas encore enregistrée : son maillage, et la place de son pivot sur sa tête.</summary>
        public readonly struct Part
        {
            public Part(Mesh mesh, Vector3 pivot)
            {
                Mesh = mesh;
                Pivot = pivot;
            }

            public Mesh Mesh { get; }
            public Vector3 Pivot { get; }
        }

        /// <summary>Les triangles de cette sorte en une pièce ; faux si la tête n'en a pas.</summary>
        public static bool TryCut(SkinnedMeshRenderer skin, HeadShells shells, HeadShells.Kind kind, out Part part)
        {
            part = default;
            List<int>[] triangles = shells.TrianglesOf(kind);
            (List<int> kept, List<int>[] parts) = BodyCutter.Compact(shells.Positions.Length, triangles);
            if (kept.Count == 0)
                return false;

            List<Vector3> positions = BodyCutter.Pick(shells.Positions, kept);
            var box = new Bounds(positions[0], Vector3.zero);
            foreach (Vector3 position in positions)
                box.Encapsulate(position);
            Vector3 pivot = kind == HeadShells.Kind.Snout
                ? new Vector3(0f, box.center.y, box.min.z)
                : new Vector3(0f, box.min.y, box.center.z);
            for (int i = 0; i < positions.Count; i++)
                positions[i] -= pivot;

            var uvs = new List<Vector2>();
            skin.sharedMesh.GetUVs(0, uvs);
            var mesh = new Mesh { name = kind.ToString() };
            mesh.SetVertices(positions);
            mesh.SetNormals(BodyCutter.Pick(shells.Normals, kept));
            if (uvs.Count == shells.Positions.Length)
                mesh.SetUVs(0, BodyCutter.Pick(uvs.ToArray(), kept));
            mesh.subMeshCount = parts.Length;
            for (int sub = 0; sub < parts.Length; sub++)
                mesh.SetTriangles(parts[sub], sub);
            // Une pièce détachée d'un crâne où elle était cousue (les oreilles du raton) a le dessous ouvert : on le
            // referme, comme les autres pièces, fermées d'elles-mêmes ; le couvercle, dans un sous-maillage à lui.
            SkullCap.Close(mesh, true);
            mesh.RecalculateBounds();
            part = new Part(mesh, pivot);
            return true;
        }

        /// <summary>
        /// Vrai si ces deux pièces se voient pareilles : autant de triangles, la même boîte et le même pivot à 5 mm près. Les
        /// modèles A et B du pack ne rangent ni ne cousent leurs sommets de la même façon.
        /// </summary>
        public static bool Same(Part a, Part b)
        {
            Bounds left = a.Mesh.bounds, right = b.Mesh.bounds;
            return a.Mesh.triangles.Length == b.Mesh.triangles.Length &&
                   (a.Pivot - b.Pivot).sqrMagnitude <= SameMeters * SameMeters &&
                   (left.min - right.min).sqrMagnitude <= SameMeters * SameMeters &&
                   (left.max - right.max).sqrMagnitude <= SameMeters * SameMeters;
        }

        /// <summary>
        /// Enregistre la pièce sous ce nom (Oreilles_Chat), de ce matériau, en deux moitiés si elle va par paire (EarHalves) ;
        /// rend son prefab.
        /// </summary>
        public static GameObject Save(Part part, string name, Material material, bool pair)
        {
            EditorAssets.EnsureFolder(Folder);
            part.Mesh.name = name;
            if (pair)
            {
                var same = new Material[part.Mesh.subMeshCount];
                for (int i = 0; i < same.Length; i++)
                    same[i] = material;
                GameObject halves = EarHalves.Save(part.Mesh, same, name, Quaternion.identity);
                Object.DestroyImmediate(part.Mesh);
                return halves;
            }
            Mesh mesh = BodyCutter.Save(part.Mesh, Folder + "/" + name + ".asset");
            var model = new GameObject(name);
            try
            {
                model.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = model.AddComponent<MeshRenderer>();
                var materials = new Material[mesh.subMeshCount];
                for (int i = 0; i < materials.Length; i++)
                    materials[i] = material;
                renderer.sharedMaterials = materials;
                return PrefabUtility.SaveAsPrefabAsset(model, Folder + "/" + name + ".prefab");
            }
            finally
            {
                Object.DestroyImmediate(model);
            }
        }
    }
}
