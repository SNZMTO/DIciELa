using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Le dallage d'un sol qu'on traverse (décisions 193 et 196) : un disque de ce diamètre, centré sur son origine, son
    /// dessus un peu au-dessus du sol. Il est tramé en anneaux d'un demi-mètre et en secteurs, pour épouser le relief une
    /// fois posé (FloorDrape). Ses UV se lisent à plat, en motifs de sa texture mesurés en mètres : les pierres gardent leur
    /// taille, et la texture du pack se raccorde d'elle-même. Une jupe fait le tour en descendant sous le sol : un relief
    /// inégal ne laisse jamais voir de vide sous son bord.
    /// </summary>
    internal static class FloorMesh
    {
        private const float RingMeters = 0.5f;
        private const int Sectors = 72;

        /// <summary>
        /// Le disque de ce diamètre, son dessus à top mètres, sa jupe jusqu'à skirt mètres sous le sol, un motif de sa
        /// texture tous les pattern mètres.
        /// </summary>
        public static Mesh Create(string name, float diameter, float top, float skirt, float pattern)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            float radius = diameter * 0.5f;
            int rings = Mathf.Max(1, Mathf.CeilToInt(radius / RingMeters));

            // Le milieu, puis chaque anneau, du dedans vers le dehors.
            Add(Vector2.zero, top, radius, pattern, vertices, normals, uvs);
            for (int ring = 1; ring <= rings; ring++)
            {
                float r = radius * ring / rings;
                for (int sector = 0; sector < Sectors; sector++)
                    Add(Around(sector, r), top, radius, pattern, vertices, normals, uvs);
            }
            for (int sector = 0; sector < Sectors; sector++)
            {
                int next = (sector + 1) % Sectors;
                // Le centre : un éventail vers le premier anneau ; l'ordre des sommets tourne pour faire face au ciel.
                triangles.AddRange(new[] { 0, 1 + next, 1 + sector });
                for (int ring = 1; ring < rings; ring++)
                {
                    int inner = 1 + (ring - 1) * Sectors;
                    int outer = inner + Sectors;
                    triangles.AddRange(new[] { inner + sector, inner + next, outer + next });
                    triangles.AddRange(new[] { inner + sector, outer + next, outer + sector });
                }
            }
            AddSkirt(radius, top, -skirt, pattern, vertices, normals, uvs, triangles);

            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        // Un sommet du dessus, ses UV à plat depuis le coin du carré qui tient le disque.
        private static void Add(Vector2 at, float top, float radius, float pattern, List<Vector3> vertices,
            List<Vector3> normals, List<Vector2> uvs)
        {
            vertices.Add(new Vector3(at.x, top, at.y));
            normals.Add(Vector3.up);
            uvs.Add(new Vector2((at.x + radius) / pattern, (at.y + radius) / pattern));
        }

        // Le point de ce secteur sur le cercle de ce rayon, en tournant vers l'est puis le nord.
        private static Vector2 Around(int sector, float r)
        {
            float angle = 2f * Mathf.PI * sector / Sectors;
            return new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
        }

        // La jupe : une bande verticale tout autour, du dessus jusqu'au bas, tournée vers l'extérieur.
        private static void AddSkirt(float radius, float top, float bottom, float pattern, List<Vector3> vertices,
            List<Vector3> normals, List<Vector2> uvs, List<int> triangles)
        {
            int first = vertices.Count;
            float height = (top - bottom) / pattern;
            for (int sector = 0; sector <= Sectors; sector++)
            {
                Vector2 at = Around(sector % Sectors, radius);
                var outward = new Vector3(at.x, 0f, at.y).normalized;
                float along = 2f * Mathf.PI * radius * sector / Sectors / pattern;
                vertices.Add(new Vector3(at.x, bottom, at.y));
                vertices.Add(new Vector3(at.x, top, at.y));
                normals.Add(outward);
                normals.Add(outward);
                uvs.Add(new Vector2(along, 0f));
                uvs.Add(new Vector2(along, height));
            }
            for (int sector = 0; sector < Sectors; sector++)
            {
                int low = first + sector * 2;
                // Bas, haut, haut suivant ; bas, haut suivant, bas suivant : la normale d'un triangle d'Unity, le produit
                // vectoriel de ses deux premiers côtés, regarde alors le dehors.
                triangles.AddRange(new[] { low, low + 1, low + 3, low, low + 3, low + 2 });
            }
        }
    }
}
