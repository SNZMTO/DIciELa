using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les pièces que le pack pose lui-même sur ses personnages (SAVOIR, 2.11) : les cheveux, chapeaux et barbes des
    /// humains, à l'origine de leur os Head ; la huppe de l'oiseau sous son os head ; les queues du chat et du renard sous
    /// pelvis. Ce sont des copies des modèles de Chr_Attach, qu'on reconnaît à leur maillage. Chaque pose donne le
    /// personnage, la place du pivot sur le personnage debout et l'os qui la porte.
    /// </summary>
    internal sealed class PackPoses
    {

        private readonly List<Pose> _poses = new List<Pose>();

        public readonly struct Pose
        {
            public Pose(GameObject character, Mesh mesh, Vector3 pivot, string bone, Vector3 headBone, bool human)
            {
                Character = character;
                Mesh = mesh;
                Pivot = pivot;
                Bone = bone;
                HeadBone = headBone;
                Human = human;
            }

            public GameObject Character { get; }
            public Mesh Mesh { get; }
            public Vector3 Pivot { get; }
            public string Bone { get; }

            /// <summary>La place de l'os de la tête du personnage (Head chez les humains, head chez les animaux).</summary>
            public Vector3 HeadBone { get; }

            public bool Human { get; }
        }

        /// <summary>Les poses de tous les personnages du pack.</summary>
        public static PackPoses Read()
        {
            var poses = new PackPoses();
            foreach (string file in Directory.GetFiles(PackPaths.Characters, "*.prefab"))
            {
                string path = file.Replace('\\', '/');
                var character = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Transform head = null;
                    bool human = false;
                    foreach (Transform bone in root.GetComponentsInChildren<Transform>(true))
                    {
                        if (bone.name == "head" || bone.name == "Head")
                        {
                            head = bone;
                            human = bone.name == "Head";
                        }
                    }
                    foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(false))
                    {
                        if (filter.sharedMesh == null || head == null)
                            continue;
                        poses._poses.Add(new Pose(character, filter.sharedMesh, filter.transform.position,
                            filter.transform.parent.name, head.position, human));
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            return poses;
        }

        /// <summary>La première pose de cette pièce du pack, sur un humain ou sur un animal ; faux si le pack ne la pose pas.</summary>
        public bool TryFind(GameObject piece, bool human, out Pose pose)
        {
            MeshFilter own = piece != null ? piece.GetComponentInChildren<MeshFilter>(true) : null;
            foreach (Pose candidate in _poses)
            {
                if (own != null && candidate.Mesh == own.sharedMesh && candidate.Human == human)
                {
                    pose = candidate;
                    return true;
                }
            }
            pose = default;
            return false;
        }

        /// <summary>Les maillages que ce personnage porte d'office, avec l'os qui porte chacun.</summary>
        public List<Pose> On(GameObject character)
        {
            return _poses.FindAll(pose => pose.Character == character);
        }
    }
}
