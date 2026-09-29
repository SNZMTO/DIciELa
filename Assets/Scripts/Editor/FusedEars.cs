using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les oreilles cousues au crâne (le raton laveur) : dans le crâne d'une tête qui n'a pas d'oreilles à part, ce qui
    /// dépasse au-dessus des yeux, sur les côtés. Le noyau d'une oreille est une feuille : un rayon lancé vers l'intérieur
    /// depuis sa face ressort par l'autre face à quelques centimètres ; le crâne, lui, est épais. Les triangles minces se
    /// groupent en morceaux ; il en faut un grand de chaque côté pour faire une paire. Le dos d'une oreille, épais à sa
    /// base, se reconnaît à sa peinture : on ajoute au noyau, de proche en proche, les triangles voisins qui ne sont pas de
    /// la teinte du crâne (la plus répandue). La base, là où l'oreille rejoint le dôme, laisse un trou que SkullCap referme.
    /// </summary>
    internal static class FusedEars
    {
        // Mince : l'autre face est à moins de ce nombre de mètres (le dôme en fait plus de 60 cm). Une oreille est au-dessus
        // des yeux d'au moins la marge, à plus de la demi-largeur donnée du milieu, et son noyau compte au moins tant de
        // triangles. Deux triangles sont de la même teinte si leurs coordonnées de texture sont à moins de l'écart donné.
        private const float ThinMeters = 0.3f;
        private const float AboveEyes = 0.1f;
        private const float SideMeters = 0.15f;
        private const int MinTriangles = 30;
        private const float Nudge = 0.002f;
        private const float SamePaint = 0.006f;

        /// <summary>Les triangles des oreilles cousues à ce crâne (premiers indices), vide s'il n'y en a pas une paire.</summary>
        public static HashSet<(int Sub, int First)> Find(Vector3[] positions, Vector2[] uvs, int[][] triangles,
            List<(int Sub, int First)> skull, float eyesHeight)
        {
            var flat = new List<int>();
            foreach ((int sub, int first) in skull)
            {
                flat.Add(triangles[sub][first]);
                flat.Add(triangles[sub][first + 1]);
                flat.Add(triangles[sub][first + 2]);
            }
            var ray = new MeshRay(positions, flat);
            var sides = new List<(int Sub, int First)>();
            var thin = new List<(int Sub, int First)>();
            foreach ((int sub, int first) in skull)
            {
                Vector3 a = positions[triangles[sub][first]], b = positions[triangles[sub][first + 1]];
                Vector3 c = positions[triangles[sub][first + 2]];
                Vector3 center = (a + b + c) / 3f;
                if (center.y < eyesHeight + AboveEyes || Mathf.Abs(center.x) < SideMeters)
                    continue;
                sides.Add((sub, first));
                Vector3 inward = -Vector3.Cross(b - a, c - a).normalized;
                // Un bord de feuille ne voit rien derrière lui : il est mince aussi.
                if (!ray.TryHit(center + inward * Nudge, inward, out float depth) || depth < ThinMeters)
                    thin.Add((sub, first));
            }
            HashSet<(int Sub, int First)> ears = Pair(positions, triangles, thin);
            if (ears.Count > 0)
                Grow(ears, sides, positions, uvs, triangles, SkullPaint(uvs, triangles, skull));
            return ears;
        }

        // Le plus grand morceau mince de chaque côté ; une paire, ou rien.
        private static HashSet<(int Sub, int First)> Pair(Vector3[] positions, int[][] triangles,
            List<(int Sub, int First)> thin)
        {
            (int Sub, int First)[] left = null, right = null;
            foreach (List<(int Sub, int First)> shell in MeshShells.Group(positions, triangles, thin))
            {
                if (shell.Count < MinTriangles)
                    continue;
                float x = positions[triangles[shell[0].Sub][shell[0].First]].x;
                if (x < 0f && (left == null || shell.Count > left.Length))
                    left = shell.ToArray();
                if (x > 0f && (right == null || shell.Count > right.Length))
                    right = shell.ToArray();
            }
            var ears = new HashSet<(int Sub, int First)>();
            if (left == null || right == null)
                return ears;
            ears.UnionWith(left);
            ears.UnionWith(right);
            return ears;
        }

        // Ajoute aux oreilles, de proche en proche, les triangles de ces candidats qui touchent une oreille par un sommet
        // et ne sont pas de la teinte du crâne. Borné : chaque tour en ajoute au moins un, ou s'arrête.
        private static void Grow(HashSet<(int Sub, int First)> ears, List<(int Sub, int First)> candidates,
            Vector3[] positions, Vector2[] uvs, int[][] triangles, Vector2 skullPaint)
        {
            int[] weld = MeshShells.Weld(positions);
            var touched = new HashSet<int>();
            foreach ((int sub, int first) in ears)
            {
                for (int k = 0; k < 3; k++)
                    touched.Add(weld[triangles[sub][first + k]]);
            }
            var rest = new List<(int Sub, int First)>();
            foreach ((int Sub, int First) triangle in candidates)
            {
                if (!ears.Contains(triangle) && (Paint(uvs, triangles, triangle) - skullPaint).magnitude >= SamePaint)
                    rest.Add(triangle);
            }
            bool grew = true;
            while (grew)
            {
                grew = false;
                for (int i = rest.Count - 1; i >= 0; i--)
                {
                    (int sub, int first) = rest[i];
                    int[] corners = { weld[triangles[sub][first]], weld[triangles[sub][first + 1]],
                        weld[triangles[sub][first + 2]] };
                    if (!touched.Contains(corners[0]) && !touched.Contains(corners[1]) && !touched.Contains(corners[2]))
                        continue;
                    ears.Add(rest[i]);
                    touched.UnionWith(corners);
                    rest.RemoveAt(i);
                    grew = true;
                }
            }
        }

        // La teinte la plus répandue du crâne : la moyenne des triangles de la case la plus remplie.
        private static Vector2 SkullPaint(Vector2[] uvs, int[][] triangles, List<(int Sub, int First)> skull)
        {
            var sums = new Dictionary<Vector2Int, (Vector2 Sum, int Count)>();
            Vector2Int best = Vector2Int.zero;
            foreach ((int Sub, int First) triangle in skull)
            {
                Vector2 paint = Paint(uvs, triangles, triangle);
                Vector2Int key = Vector2Int.FloorToInt(paint / SamePaint);
                (Vector2 sum, int count) = sums.TryGetValue(key, out var cell) ? cell : (Vector2.zero, 0);
                sums[key] = (sum + paint, count + 1);
                if (count + 1 > (sums.TryGetValue(best, out var most) ? most.Count : 0))
                    best = key;
            }
            return sums[best].Sum / sums[best].Count;
        }

        private static Vector2 Paint(Vector2[] uvs, int[][] triangles, (int Sub, int First) triangle)
        {
            (int sub, int first) = triangle;
            return (uvs[triangles[sub][first]] + uvs[triangles[sub][first + 1]] + uvs[triangles[sub][first + 2]]) / 3f;
        }
    }
}
