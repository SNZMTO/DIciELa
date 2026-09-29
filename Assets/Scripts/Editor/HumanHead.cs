using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La tête d'un humain du pack, mesurée comme celle d'un animal (HeadMeasures, SurfaceBaker) : les pièces faites pour
    /// les humains (cheveux, chapeaux 1 à 5, barbe…) sont posées par le pack à l'origine de l'os Head de leur porteur ;
    /// mesurer la tête de ce porteur dit où elles se tiennent par rapport à son crâne, de combien elles l'enveloppent (un
    /// chapeau low poly a un fond fermé que la tête traverse), et à quelle taille (SAVOIR, 2.11). Les humains n'ont pas de
    /// morceaux à détacher : toute la tête compte.
    /// </summary>
    internal sealed class HumanHead
    {
        private static readonly string[] HeadBones = { "Head", "Eyes", "Eyebrows" };
        private const float Share = 0.5f;
        private static readonly Rect TopArea = Rect.MinMaxRect(-0.3f, -0.3f, 0.3f, 0.3f);
        private static readonly Rect FrontArea = Rect.MinMaxRect(-0.3f, 1.3f, 0.3f, 2f);
        private const float Cell = 0.015f;

        private HumanHead(HeadMeasures measures, Vector3 headBone, SurfaceMap top, SurfaceMap front)
        {
            Measures = measures;
            HeadBone = headBone;
            Top = top;
            Front = front;
        }

        public HeadMeasures Measures { get; }

        /// <summary>La place de son os Head, où le pack pose ses pièces de tête.</summary>
        public Vector3 HeadBone { get; }

        public SurfaceMap Top { get; }
        public SurfaceMap Front { get; }

        /// <summary>La tête de ce personnage humain du pack.</summary>
        public static HumanHead Of(GameObject character)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(character));
            try
            {
                SkinnedMeshRenderer skin = PackSkin.VisibleBody(root);
                (Vector3[] positions, _) = PackSkin.Standing(skin);
                float[] head = PackSkin.Share(skin, HeadBones);
                float[] eyes = PackSkin.Share(skin, new[] { "Eyes" });
                float[] brows = PackSkin.Share(skin, new[] { "Eyebrows" });
                int[] triangles = skin.sharedMesh.triangles;
                var skull = new List<int>();
                for (int t = 0; t < triangles.Length; t += 3)
                {
                    if ((head[triangles[t]] + head[triangles[t + 1]] + head[triangles[t + 2]]) / 3f >= Share)
                        skull.AddRange(new[] { triangles[t], triangles[t + 1], triangles[t + 2] });
                }
                var eyeVertices = new List<Vector3>();
                float browSum = 0f;
                int browCount = 0;
                for (int v = 0; v < eyes.Length; v++)
                {
                    if (eyes[v] >= Share)
                        eyeVertices.Add(positions[v]);
                    if (brows[v] >= Share)
                    {
                        browSum += positions[v].y;
                        browCount++;
                    }
                }
                // Les sourcils se mesurent sur leurs sommets, comme chez les animaux.
                HeadMeasures measures = HeadMeasures.Of(positions, skull, eyeVertices, browSum / Mathf.Max(1, browCount));
                return new HumanHead(measures, PackSkin.Bone(skin, "Head"),
                    SurfaceBaker.Surface(positions, skull, ContactAxis.Down, TopArea, Cell),
                    SurfaceBaker.Surface(positions, skull, ContactAxis.Back, FrontArea, Cell));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>Sa surface pour ce contact : le dessus du crâne, l'avant de la face ; null pour les autres.</summary>
        public SurfaceMap SurfaceOf(ContactAxis axis)
        {
            return axis == ContactAxis.Down ? Top : axis == ContactAxis.Back ? Front : null;
        }
    }
}
