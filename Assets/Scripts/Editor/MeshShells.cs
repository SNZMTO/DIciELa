using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les morceaux d'un seul tenant (« shells ») d'un ensemble de triangles : deux triangles sont du même morceau s'ils
    /// partagent un sommet, les sommets étant soudés par leur position, car le pack double un sommet à chaque couture de
    /// texture et à chaque arête vive.
    /// </summary>
    internal static class MeshShells
    {
        // Soudure : deux sommets à moins d'un dixième de millimètre sont le même.
        private const float WeldPerMeter = 10000f;

        /// <summary>Ces triangles (sous-maillage, premier indice) groupés par morceau.</summary>
        public static List<List<(int Sub, int First)>> Group(Vector3[] positions, int[][] triangles,
            List<(int Sub, int First)> members)
        {
            int[] weld = Weld(positions);
            var groups = new UnionFind(positions.Length);
            foreach ((int sub, int first) in members)
            {
                groups.Join(weld[triangles[sub][first]], weld[triangles[sub][first + 1]]);
                groups.Join(weld[triangles[sub][first]], weld[triangles[sub][first + 2]]);
            }

            var shells = new Dictionary<int, List<(int Sub, int First)>>();
            foreach ((int sub, int first) in members)
            {
                int root = groups.Root(weld[triangles[sub][first]]);
                if (!shells.TryGetValue(root, out List<(int, int)> shell))
                    shells[root] = shell = new List<(int, int)>();
                shell.Add((sub, first));
            }
            return new List<List<(int Sub, int First)>>(shells.Values);
        }

        /// <summary>Pour chaque sommet, le premier sommet à sa place : les copies d'une couture deviennent un seul.</summary>
        public static int[] Weld(IReadOnlyList<Vector3> positions)
        {
            var first = new Dictionary<Vector3Int, int>();
            var weld = new int[positions.Count];
            for (int v = 0; v < positions.Count; v++)
            {
                Vector3Int key = Vector3Int.RoundToInt(positions[v] * WeldPerMeter);
                if (!first.TryGetValue(key, out int kept))
                    first[key] = kept = v;
                weld[v] = kept;
            }
            return weld;
        }
    }
}
