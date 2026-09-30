using System.Collections.Generic;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Le fanion d'un arbre ou d'un rocher marqué à retirer : un mât et une flamme, deux boîtes, le pied du mât à
    /// l'origine. Un remplaçant simple, en attendant le choix du développeur ; un seul maillage pour tous les fanions.
    /// </summary>
    internal static class MarkMesh
    {
        private const float PoleWidth = 0.06f;
        private const float FlagWidth = 0.5f;
        private const float FlagHeight = 0.3f;
        private const float FlagDepth = 0.03f;

        public static Mesh Build(float height)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            AddBox(new Vector3(0f, height * 0.5f, 0f), new Vector3(PoleWidth, height, PoleWidth), vertices, normals,
                triangles);
            AddBox(new Vector3(PoleWidth * 0.5f + FlagWidth * 0.5f, height - FlagHeight * 0.5f, 0f),
                new Vector3(FlagWidth, FlagHeight, FlagDepth), vertices, normals, triangles);
            var mesh = new Mesh { name = "Fanion" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Une boîte de cette taille, à ce centre : quatre coins par face, pour que chaque face ait sa normale.</summary>
        private static void AddBox(Vector3 center, Vector3 size, List<Vector3> vertices, List<Vector3> normals,
            List<int> triangles)
        {
            Vector3 half = size * 0.5f;
            Vector3[] axes = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            foreach (Vector3 normal in axes)
            {
                // Deux axes le long de la face (v = normale × u) ; vus de dehors, les coins tournent dans le sens des
                // aiguilles d'une montre, le sens des faces visibles pour Unity.
                Vector3 u = Mathf.Abs(normal.y) > 0.5f ? Vector3.right : Vector3.up;
                Vector3 v = Vector3.Cross(normal, u);
                int first = vertices.Count;
                Vector3 face = center + Vector3.Scale(normal, half);
                Vector3 du = Vector3.Scale(u, half);
                Vector3 dv = Vector3.Scale(v, half);
                vertices.Add(face - du - dv);
                vertices.Add(face + du - dv);
                vertices.Add(face + du + dv);
                vertices.Add(face - du + dv);
                for (int i = 0; i < 4; i++)
                    normals.Add(normal);
                triangles.AddRange(new[] { first, first + 1, first + 2, first, first + 2, first + 3 });
            }
        }
    }
}
