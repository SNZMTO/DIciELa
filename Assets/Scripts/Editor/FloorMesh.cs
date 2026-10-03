using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Le dallage d'un sol qu'on traverse (décision 193) : un maillage plat de largeur × profondeur mètres, centré sur son
    /// origine, son dessus un peu au-dessus du sol. Il est tramé en mailles d'un demi-mètre au plus, pour épouser le relief
    /// une fois posé (FloorDrape). Ses UV comptent en motifs de sa texture, mesurés en mètres, pour que les dalles gardent
    /// leur taille quelle que soit celle du sol ; chaque bloc d'un motif tourne sa texture d'un quart de tour tiré au
    /// hasard (un tirage fixe), pour que le motif ne se répète pas en grille : les joints des dalles restent alignés.
    /// Une jupe fait le tour en descendant sous le sol : un relief inégal ne laisse jamais voir de vide sous son bord.
    /// </summary>
    internal static class FloorMesh
    {
        private const float CellMeters = 0.5f;
        private const int Seed = 193;
        // Ce qui reste d'un bloc plus petit que ce seuil, en mètres, n'en fait pas un de plus.
        private const float Slack = 0.001f;

        /// <summary>
        /// Le dallage de cette taille, son dessus à top mètres, sa jupe jusqu'à skirt mètres sous le sol, un motif de sa
        /// texture tous les pattern mètres.
        /// </summary>
        public static Mesh Create(string name, float width, float depth, float top, float skirt, float pattern)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            var random = new System.Random(Seed);
            var corner = new Vector2(-width * 0.5f, -depth * 0.5f);
            for (float z = 0f; z < depth - Slack; z += pattern)
            {
                for (float x = 0f; x < width - Slack; x += pattern)
                {
                    var from = new Vector2(x, z);
                    var to = new Vector2(Mathf.Min(x + pattern, width), Mathf.Min(z + pattern, depth));
                    AddBlock(corner, from, to, top, pattern, random.Next(4), vertices, normals, uvs, triangles);
                }
            }

            // La jupe : les quatre bords, chacun de son coin à l'autre en tournant autour du sol.
            float w = width * 0.5f;
            float d = depth * 0.5f;
            var corners = new[] { new Vector2(-w, -d), new Vector2(w, -d), new Vector2(w, d), new Vector2(-w, d) };
            for (int i = 0; i < 4; i++)
                AddSkirt(corners[i], corners[(i + 1) % 4], top, -skirt, pattern, vertices, normals, uvs, triangles);

            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        // Un bloc d'un motif, de from à to (en mètres depuis le coin du sol) : sa trame, ses UV tournés de tant de quarts.
        private static void AddBlock(Vector2 corner, Vector2 from, Vector2 to, float top, float pattern, int turns,
            List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles)
        {
            int columns = Mathf.Max(1, Mathf.CeilToInt((to.x - from.x) / CellMeters - Slack));
            int rows = Mathf.Max(1, Mathf.CeilToInt((to.y - from.y) / CellMeters - Slack));
            int first = vertices.Count;
            for (int z = 0; z <= rows; z++)
            {
                for (int x = 0; x <= columns; x++)
                {
                    float u = Mathf.Lerp(from.x, to.x, (float)x / columns);
                    float v = Mathf.Lerp(from.y, to.y, (float)z / rows);
                    vertices.Add(new Vector3(corner.x + u, top, corner.y + v));
                    normals.Add(Vector3.up);
                    uvs.Add(Turn(new Vector2((u - from.x) / pattern, (v - from.y) / pattern), turns));
                }
            }
            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int below = first + z * (columns + 1) + x;
                    int above = below + columns + 1;
                    triangles.AddRange(new[] { below, above, above + 1, below, above + 1, below + 1 });
                }
            }
        }

        // Le point de texture tourné de tant de quarts de tour autour du milieu du motif.
        private static Vector2 Turn(Vector2 uv, int turns)
        {
            switch (turns)
            {
                case 1: return new Vector2(uv.y, 1f - uv.x);
                case 2: return new Vector2(1f - uv.x, 1f - uv.y);
                case 3: return new Vector2(1f - uv.y, uv.x);
                default: return uv;
            }
        }

        // Une bande verticale de from à to (vus d'en haut), du dessus jusqu'au bas, tournée vers l'extérieur du sol.
        private static void AddSkirt(Vector2 from, Vector2 to, float top, float bottom, float pattern,
            List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles)
        {
            Vector2 along = to - from;
            var outward = new Vector3(along.y, 0f, -along.x).normalized;
            int first = vertices.Count;
            vertices.Add(new Vector3(from.x, bottom, from.y));
            vertices.Add(new Vector3(from.x, top, from.y));
            vertices.Add(new Vector3(to.x, top, to.y));
            vertices.Add(new Vector3(to.x, bottom, to.y));
            float length = along.magnitude / pattern;
            float height = (top - bottom) / pattern;
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(0f, height));
            uvs.Add(new Vector2(length, height));
            uvs.Add(new Vector2(length, 0f));
            for (int i = 0; i < 4; i++)
                normals.Add(outward);
            // L'ordre des sommets fait face à l'extérieur : la normale d'un triangle d'Unity est le produit vectoriel de
            // ses deux premiers côtés ; s'il regarde vers l'intérieur, on renverse.
            Vector3 facing = Vector3.Cross(vertices[first + 1] - vertices[first], vertices[first + 2] - vertices[first]);
            if (Vector3.Dot(facing, outward) >= 0f)
                triangles.AddRange(new[] { first, first + 1, first + 2, first, first + 2, first + 3 });
            else
                triangles.AddRange(new[] { first, first + 2, first + 1, first, first + 3, first + 2 });
        }
    }
}
