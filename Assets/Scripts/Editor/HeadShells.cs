using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les morceaux de la tête d'un animal du pack (SAVOIR, 2.11) : les triangles dont les sommets suivent pour moitié au
    /// moins les os de la tête, en morceaux d'un seul tenant (sommets soudés par leur position), chacun fermé. Le plus
    /// grand est le crâne ; une paire de grands morceaux hors du milieu, au-dessus des yeux, les oreilles ; de petits
    /// morceaux en haut, au milieu, les touffes ; un morceau au milieu, devant, sous les yeux, le museau (le renard). Des
    /// oreilles cousues au crâne (FusedEars) et un museau cousu à la face (FusedSnout) s'en détachent aussi ; le reste
    /// (yeux, sourcils, moustaches) va avec le crâne. Positions du personnage debout : pieds à l'origine, face vers +z.
    /// </summary>
    internal sealed class HeadShells
    {
        public enum Kind
        {
            Body,
            Skull,
            Ears,
            Tufts,
            Snout
        }

        private const float HeadShare = 0.5f;
        // Une oreille compte au moins ce nombre de triangles (la plus petite du pack, celle du renard, en a 98 ; une
        // touffe, 40 au plus) et s'écarte de la ligne du milieu d'au moins ce nombre de mètres.
        private const int EarTriangles = 60;
        private const float MiddleMeters = 0.1f;
        // Une touffe est au moins à cette hauteur au-dessus des yeux ; un museau, devant l'os des yeux.
        private const float TuftAboveEyes = 0.2f;
        private const int MaxTufts = 3;
        private static readonly HashSet<string> HeadBones = new HashSet<string> { "head", "eyes", "eyebrows" };

        private readonly Kind[] _kindOfTriangle;
        private readonly int[][] _triangles;

        private HeadShells(Vector3[] positions, Vector3[] normals, List<Vector3> eyes, float browHeight, int[][] triangles,
            Kind[] kinds)
        {
            Positions = positions;
            Normals = normals;
            Eyes = eyes;
            BrowHeight = browHeight;
            _triangles = triangles;
            _kindOfTriangle = kinds;
        }

        /// <summary>La place de chaque sommet sur le personnage debout.</summary>
        public Vector3[] Positions { get; }

        /// <summary>L'orientation de chaque sommet sur le personnage debout.</summary>
        public Vector3[] Normals { get; }

        /// <summary>La place des sommets de ses yeux, qui suivent l'os eyes pour moitié au moins.</summary>
        public List<Vector3> Eyes { get; }

        /// <summary>La hauteur moyenne des sommets de ses sourcils (l'os eyebrows des animaux est plus bas, aux yeux).</summary>
        public float BrowHeight { get; }

        /// <summary>
        /// Les morceaux de ce corps allumé d'un personnage du pack ; snoutFront : la profondeur au-delà de laquelle sa face
        /// porte un museau cousu (FusedSnout), ou l'infini s'il n'en a pas.
        /// </summary>
        public static HeadShells Of(SkinnedMeshRenderer skin, float snoutFront)
        {
            Mesh source = skin.sharedMesh;
            (Vector3[] positions, Vector3[] normals) = PackSkin.Standing(skin);
            float[] head = PackSkin.Share(skin, HeadBones);
            float[] eyes = PackSkin.Share(skin, new[] { "eyes" });
            float[] brows = PackSkin.Share(skin, new[] { "eyebrows" });
            float eyesHeight = PackSkin.Bone(skin, "eyes").y;

            var triangles = new int[source.subMeshCount][];
            var heads = new List<(int Sub, int First)>();
            for (int sub = 0; sub < source.subMeshCount; sub++)
            {
                triangles[sub] = source.GetTriangles(sub);
                for (int t = 0; t < triangles[sub].Length; t += 3)
                {
                    if (Mean(head, triangles[sub], t) >= HeadShare)
                        heads.Add((sub, t));
                }
            }

            List<List<(int Sub, int First)>> shells = MeshShells.Group(positions, triangles, heads);
            List<(int Sub, int First)> skull = shells[0];
            foreach (List<(int Sub, int First)> shell in shells)
            {
                if (shell.Count > skull.Count)
                    skull = shell;
            }

            var kinds = new Kind[source.triangles.Length / 3];
            var firstOfSub = new int[source.subMeshCount];
            for (int sub = 1; sub < source.subMeshCount; sub++)
                firstOfSub[sub] = firstOfSub[sub - 1] + triangles[sub - 1].Length / 3;
            var counts = new Dictionary<Kind, int>();
            foreach (List<(int Sub, int First)> shell in shells)
            {
                Kind kind = shell == skull ? Kind.Skull : Classify(shell, triangles, positions, eyes, brows, eyesHeight);
                counts[kind] = (counts.TryGetValue(kind, out int count) ? count : 0) + 1;
                foreach ((int sub, int first) in shell)
                    kinds[firstOfSub[sub] + first / 3] = kind;
            }
            Check(skin, counts);
            // Une tête sans oreilles à part peut les avoir cousues au crâne (le raton laveur) : on les détache aussi.
            if (!counts.ContainsKey(Kind.Ears))
            {
                foreach ((int sub, int first) in FusedEars.Find(positions, source.uv, triangles, skull, eyesHeight))
                    kinds[firstOfSub[sub] + first / 3] = Kind.Ears;
            }
            (List<Vector3> eyeVertices, float browHeight) = PackSkin.Features(positions, eyes, brows);
            // Un museau cousu à la face aussi, s'il n'y en a pas un à part (le renard) ; il est sous les oreilles cousues.
            if (!counts.ContainsKey(Kind.Snout) && !float.IsInfinity(snoutFront))
            {
                foreach ((int sub, int first) in FusedSnout.Find(positions, triangles, skull, eyeVertices, snoutFront))
                    kinds[firstOfSub[sub] + first / 3] = Kind.Snout;
            }
            return new HeadShells(positions, normals, eyeVertices, browHeight, triangles, kinds);
        }

        /// <summary>Les triangles de cette sorte, tous sous-maillages confondus.</summary>
        public List<int> AllTrianglesOf(Kind kind)
        {
            var all = new List<int>();
            foreach (List<int> part in TrianglesOf(kind))
                all.AddRange(part);
            return all;
        }

        /// <summary>Les triangles de cette sorte, sous-maillage par sous-maillage, en indices de sommets de la source.</summary>
        public List<int>[] TrianglesOf(Kind kind)
        {
            var parts = new List<int>[_triangles.Length];
            int index = 0;
            for (int sub = 0; sub < _triangles.Length; sub++)
            {
                parts[sub] = new List<int>();
                for (int t = 0; t < _triangles[sub].Length; t += 3, index++)
                {
                    if (_kindOfTriangle[index] != kind)
                        continue;
                    parts[sub].Add(_triangles[sub][t]);
                    parts[sub].Add(_triangles[sub][t + 1]);
                    parts[sub].Add(_triangles[sub][t + 2]);
                }
            }
            return parts;
        }

        private static Kind Classify(List<(int Sub, int First)> shell, int[][] triangles, Vector3[] positions,
            float[] eyes, float[] brows, float eyesHeight)
        {
            Vector3 sum = Vector3.zero;
            float eyeShare = 0f, browShare = 0f;
            int count = 0;
            foreach ((int sub, int first) in shell)
            {
                for (int k = 0; k < 3; k++)
                {
                    int v = triangles[sub][first + k];
                    sum += positions[v];
                    eyeShare += eyes[v];
                    browShare += brows[v];
                    count++;
                }
            }
            Vector3 mean = sum / count;
            if (eyeShare / count >= HeadShare || browShare / count >= HeadShare)
                return Kind.Skull;
            bool middle = Mathf.Abs(mean.x) < MiddleMeters;
            if (!middle && shell.Count >= EarTriangles && mean.y > eyesHeight)
                return Kind.Ears;
            if (middle && mean.y > eyesHeight + TuftAboveEyes)
                return Kind.Tufts;
            if (middle && shell.Count >= EarTriangles / 2 && mean.y < eyesHeight)
                return Kind.Snout;
            return Kind.Skull;
        }

        private static float Mean(float[] values, int[] triangles, int first)
        {
            return (values[triangles[first]] + values[triangles[first + 1]] + values[triangles[first + 2]]) / 3f;
        }

        // Une tête du pack a au plus une paire d'oreilles, trois touffes, un museau : une tête qui sort de ces formes
        // arrête le menu plutôt que d'être mal coupée.
        private static void Check(SkinnedMeshRenderer skin, Dictionary<Kind, int> counts)
        {
            int ears = counts.TryGetValue(Kind.Ears, out int e) ? e : 0;
            int tufts = counts.TryGetValue(Kind.Tufts, out int t) ? t : 0;
            int snouts = counts.TryGetValue(Kind.Snout, out int n) ? n : 0;
            if ((ears != 0 && ears != 2) || tufts > MaxTufts || snouts > 1)
                throw new System.InvalidOperationException("DICIELA : la tête de " + skin.name + " ne se découpe pas comme " +
                                                           "prévu (" + ears + " oreilles, " + tufts + " touffes, " + snouts +
                                                           " museaux).");
        }
    }
}
