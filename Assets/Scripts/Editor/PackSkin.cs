using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Ce qu'on lit du corps allumé d'un personnage du pack, un maillage à peau : la place et l'orientation de ses
    /// sommets sur le personnage debout (pieds à l'origine, face vers +z), la part du poids de chaque sommet que portent
    /// certains os, la place d'un os.
    /// </summary>
    internal static class PackSkin
    {
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

        /// <summary>Les sommets et leurs normales, sur le personnage debout.</summary>
        public static (Vector3[] Positions, Vector3[] Normals) Standing(SkinnedMeshRenderer skin)
        {
            var baked = new Mesh();
            skin.BakeMesh(baked, false);
            Vector3[] positions = baked.vertices;
            Vector3[] normals = baked.normals;
            Object.DestroyImmediate(baked);
            for (int i = 0; i < positions.Length; i++)
                positions[i] = skin.transform.TransformPoint(positions[i]);
            for (int i = 0; i < normals.Length; i++)
                normals[i] = skin.transform.TransformDirection(normals[i]).normalized;
            return (positions, normals);
        }

        /// <summary>Pour chaque sommet, la part de son poids que portent les os de ces noms.</summary>
        public static float[] Share(SkinnedMeshRenderer skin, ICollection<string> boneNames)
        {
            Transform[] bones = skin.bones;
            var counted = new bool[bones.Length];
            for (int i = 0; i < bones.Length; i++)
                counted[i] = bones[i] != null && boneNames.Contains(bones[i].name);
            BoneWeight[] weights = skin.sharedMesh.boneWeights;
            var share = new float[weights.Length];
            for (int v = 0; v < weights.Length; v++)
            {
                BoneWeight w = weights[v];
                share[v] = (counted[w.boneIndex0] ? w.weight0 : 0f) + (counted[w.boneIndex1] ? w.weight1 : 0f) +
                           (counted[w.boneIndex2] ? w.weight2 : 0f) + (counted[w.boneIndex3] ? w.weight3 : 0f);
            }
            return share;
        }

        /// <summary>La place de cet os du personnage debout.</summary>
        public static Vector3 Bone(SkinnedMeshRenderer skin, string name)
        {
            foreach (Transform bone in skin.bones)
            {
                if (bone != null && bone.name == name)
                    return bone.position;
            }
            throw new System.InvalidOperationException("DICIELA : " + skin.name + " n'a pas d'os « " + name + " ».");
        }
    }
}
