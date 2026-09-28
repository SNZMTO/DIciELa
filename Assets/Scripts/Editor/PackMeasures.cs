using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les mesures des personnages et des pièces du pack, dans leur pose d'origine, chargés à part sans toucher la scène
    /// ouverte : la place des os ; la tête du corps allumé (le haut du crâne et l'avant de la face, pris sur la ligne du
    /// milieu, hors oreilles) ; la boîte d'une pièce autour de son pivot. Le monteur des pièces s'en sert pour leur
    /// point d'accroche de départ (SAVOIR, 2.11).
    /// </summary>
    internal static class PackMeasures
    {
        // La ligne du milieu de la tête : à moins de 8 cm du plan de symétrie, les oreilles n'y sont pas.
        private const float MiddleMeters = 0.08f;

        /// <summary>La tête d'un personnage : le haut du crâne, l'avant de la face à hauteur des yeux, le milieu.</summary>
        public readonly struct Head
        {
            public Head(float top, float front, float middle)
            {
                Top = top;
                Front = front;
                Middle = middle;
            }

            public float Top { get; }
            public float Front { get; }
            public float Middle { get; }
        }

        /// <summary>La place de chaque os de ce personnage, et sa tête : celle de son corps allumé.</summary>
        public static Dictionary<string, Vector3> Measure(GameObject character, out Head head)
        {
            var bones = new Dictionary<string, Vector3>();
            head = default;
            GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(character));
            try
            {
                foreach (Transform bone in root.GetComponentsInChildren<Transform>(true))
                    bones[bone.name] = bone.position;
                float eyes = bones["eyes"].y;
                float neck = bones["head"].y;
                foreach (Transform child in root.transform)
                {
                    var body = child.GetComponent<SkinnedMeshRenderer>();
                    if (body != null && child.gameObject.activeSelf)
                        head = HeadOf(body, neck, eyes);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return bones;
        }

        /// <summary>La boîte des maillages d'une pièce, autour de son pivot.</summary>
        public static Bounds BoxOf(GameObject piece)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(piece));
            try
            {
                var box = new Bounds();
                bool started = false;
                foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh == null)
                        continue;
                    Bounds local = filter.sharedMesh.bounds;
                    for (int corner = 0; corner < 8; corner++)
                    {
                        var sign = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f,
                            (corner & 4) == 0 ? -1f : 1f);
                        Vector3 point = filter.transform.TransformPoint(local.center + Vector3.Scale(local.extents, sign));
                        if (started)
                            box.Encapsulate(point);
                        else
                            box = new Bounds(point, Vector3.zero);
                        started = true;
                    }
                }
                return box;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Head HeadOf(SkinnedMeshRenderer body, float neck, float eyes)
        {
            var baked = new Mesh();
            body.BakeMesh(baked, true);
            float top = float.MinValue, front = float.MinValue, back = float.MaxValue;
            foreach (Vector3 vertex in baked.vertices)
            {
                Vector3 point = body.transform.TransformPoint(vertex);
                if (Mathf.Abs(point.x) > MiddleMeters || point.y < neck)
                    continue;
                top = Mathf.Max(top, point.y);
                back = Mathf.Min(back, point.z);
                if (Mathf.Abs(point.y - eyes) < 0.15f)
                    front = Mathf.Max(front, point.z);
            }
            Object.DestroyImmediate(baked);
            return new Head(top, front, (front + back) * 0.5f);
        }
    }
}
