using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Referme les trous d'un crâne découpé (là où des oreilles cousues ont été détachées, FusedEars) : chaque bord ouvert
    /// devient un éventail de triangles vers son centre, en facettes comme le reste, d'une seule couleur (celle du bord) et
    /// mené par les mêmes os. Les oreilles propres de la tête, posées dessus, le cachent. Sert au maillage enregistré comme
    /// aux mesures de la tête. Un crâne fermé ne change pas.
    /// </summary>
    internal static class SkullCap
    {
        /// <summary>Referme les bords ouverts de ce maillage à peau, en place.</summary>
        public static void Close(Mesh mesh)
        {
            var positions = new List<Vector3>(mesh.vertices);
            var all = new List<int>();
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
                all.AddRange(mesh.GetTriangles(sub));
            List<List<int>> loops = Loops(positions, all);
            if (loops.Count == 0)
                return;

            var normals = new List<Vector3>(mesh.normals);
            var uvs = new List<Vector2>();
            mesh.GetUVs(0, uvs);
            var weights = new List<BoneWeight>(mesh.boneWeights);
            var triangles = new List<int>(mesh.GetTriangles(0));
            foreach (List<int> loop in loops)
            {
                // Le couvercle a ses propres sommets : une seule couleur et une seule orientation, celles du bord.
                Vector3 normal = Normal(positions, loop);
                Vector2 uv = uvs.Count > loop[0] ? uvs[loop[0]] : Vector2.zero;
                BoneWeight weight = weights[loop[0]];
                var corners = new List<Vector3>();
                foreach (int v in loop)
                    corners.Add(positions[v]);
                corners.Add(Center(positions, loop));
                var ring = new List<int>();
                foreach (Vector3 corner in corners)
                {
                    ring.Add(positions.Count);
                    positions.Add(corner);
                    normals.Add(normal);
                    if (uvs.Count > 0)
                        uvs.Add(uv);
                    weights.Add(weight);
                }
                Fan(ring.GetRange(0, loop.Count), ring[loop.Count], triangles);
            }
            mesh.SetVertices(positions);
            if (normals.Count == positions.Count)
                mesh.SetNormals(normals);
            if (uvs.Count == positions.Count)
                mesh.SetUVs(0, uvs);
            mesh.boneWeights = weights.ToArray();
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        /// <summary>
        /// Ces triangles, refermés : positions reçoit le centre de chaque trou, et le résultat, les éventails en plus.
        /// </summary>
        public static List<int> Closed(List<Vector3> positions, List<int> triangles)
        {
            var closed = new List<int>(triangles);
            foreach (List<int> loop in Loops(positions, triangles))
            {
                positions.Add(Center(positions, loop));
                Fan(loop, positions.Count - 1, closed);
            }
            return closed;
        }

        /// <summary>Les bords ouverts, chacun la suite de ses sommets dans le sens où les triangles le parcourent.</summary>
        private static List<List<int>> Loops(List<Vector3> positions, List<int> triangles)
        {
            int[] weld = MeshShells.Weld(positions);
            // Un bord ouvert est une arête parcourue dans un sens par un triangle, jamais dans l'autre.
            var directed = new HashSet<(int, int)>();
            for (int t = 0; t < triangles.Count; t += 3)
            {
                for (int k = 0; k < 3; k++)
                    directed.Add((weld[triangles[t + k]], weld[triangles[t + (k + 1) % 3]]));
            }
            // Un sommet peut porter plusieurs bords (un trou qui se touche lui-même) : on suit chaque arête une fois, en
            // prenant à chaque sommet une arête pas encore suivie, jusqu'à revenir au départ.
            var outgoing = new Dictionary<int, List<int>>();
            foreach ((int a, int b) in directed)
            {
                if (directed.Contains((b, a)))
                    continue;
                if (!outgoing.TryGetValue(a, out List<int> targets))
                    outgoing[a] = targets = new List<int>();
                targets.Add(b);
            }
            var loops = new List<List<int>>();
            foreach (int start in new List<int>(outgoing.Keys))
            {
                while (outgoing[start].Count > 0)
                {
                    var loop = new List<int>();
                    int v = start;
                    // Borné : un bord ne compte pas plus d'arêtes qu'il n'y a de sommets.
                    for (int step = 0; step <= positions.Count; step++)
                    {
                        loop.Add(v);
                        List<int> targets = outgoing[v];
                        int to = targets[targets.Count - 1];
                        targets.RemoveAt(targets.Count - 1);
                        v = to;
                        if (v == start || !outgoing.ContainsKey(v) || outgoing[v].Count == 0)
                            break;
                    }
                    if (loop.Count >= 3)
                        loops.Add(loop);
                }
            }
            return loops;
        }

        // Un éventail du bord vers le centre, parcouru à l'envers du bord pour regarder vers l'extérieur.
        private static void Fan(List<int> loop, int center, List<int> triangles)
        {
            for (int i = 0; i < loop.Count; i++)
            {
                triangles.Add(loop[(i + 1) % loop.Count]);
                triangles.Add(loop[i]);
                triangles.Add(center);
            }
        }

        // Le centre du bord, à la hauteur moyenne de sa moitié basse : un bord qui remonte le long d'une oreille ne fait pas
        // pointer le couvercle.
        private static Vector3 Center(List<Vector3> positions, List<int> loop)
        {
            Vector3 center = Vector3.zero;
            var heights = new List<float>();
            foreach (int v in loop)
            {
                center += positions[v];
                heights.Add(positions[v].y);
            }
            heights.Sort();
            float low = 0f;
            int half = Mathf.Max(1, heights.Count / 2);
            for (int i = 0; i < half; i++)
                low += heights[i];
            center /= loop.Count;
            center.y = low / half;
            return center;
        }

        private static Vector3 Normal(List<Vector3> positions, List<int> loop)
        {
            Vector3 center = Center(positions, loop), normal = Vector3.zero;
            for (int i = 0; i < loop.Count; i++)
            {
                Vector3 a = positions[loop[i]], b = positions[loop[(i + 1) % loop.Count]];
                normal += Vector3.Cross(a - b, center - b);
            }
            return normal.normalized;
        }
    }
}
