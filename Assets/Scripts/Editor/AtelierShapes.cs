using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les formes de l'atelier (Atelier), en facettes : un sol rond un peu bosselé, en damier de deux tons ; un socle à
    /// dix pans, biseauté, où se tient la Chimère ; un fond en alcôve derrière elle, qui se penche vers elle en montant,
    /// une plinthe puis des pans de deux tons. Tout est centré sur le pied du socle ; la caméra regarde vers -z. Les
    /// bosses viennent d'un tirage à graine fixe : chaque passage refait les mêmes formes.
    /// </summary>
    internal static class AtelierShapes
    {
        /// <summary>La hauteur du dessus du socle, où se tient la Chimère, en mètres.</summary>
        public const float PedestalHeight = 0.4f;

        private const float FloorRadius = 9f;
        private const int FloorRings = 6;
        private const int FloorSegments = 18;
        private const float FloorBump = 0.03f;

        private const int PedestalSides = 10;
        private const float PedestalFootRadius = 1.4f;
        private const float PedestalSideRadius = 1.32f;
        private const float PedestalTopRadius = 1.18f;
        private const float PedestalSideHeight = 0.3f;

        private const int WallSegments = 16;
        private const float WallHalfArcDegrees = 115f;
        private static readonly float[] WallHeights = { 0f, 1.1f, 3f, 5f, 7.5f };
        private static readonly float[] WallRadii = { 8f, 8f, 7.8f, 7.2f, 6.2f };
        private const float WallBump = 0.12f;

        /// <summary>Le sol : deux matériaux, en damier de triangles.</summary>
        public static FacetMesh Floor()
        {
            var mesh = new FacetMesh(2);
            var random = new System.Random(1);
            var rings = new Vector3[FloorRings + 1][];
            for (int k = 1; k <= FloorRings; k++)
            {
                rings[k] = new Vector3[FloorSegments];
                for (int i = 0; i < FloorSegments; i++)
                {
                    // Un anneau sur deux décalé d'un demi-pan : des triangles plutôt que des trapèzes.
                    float angle = (i + 0.5f * (k % 2)) * 2f * Mathf.PI / FloorSegments;
                    rings[k][i] = Around(angle) * (FloorRadius * k / FloorRings) + Vector3.up * Bump(random, FloorBump);
                }
            }
            for (int i = 0; i < FloorSegments; i++)
                mesh.Triangle(i % 2, Vector3.zero, rings[1][i], rings[1][(i + 1) % FloorSegments], Vector3.up);
            for (int k = 2; k <= FloorRings; k++)
            {
                Vector3[] inner = rings[k - 1], outer = rings[k];
                for (int i = 0; i < FloorSegments; i++)
                {
                    int next = (i + 1) % FloorSegments;
                    int tone = (i + k) % 2;
                    if (k % 2 == 1)
                    {
                        mesh.Triangle(tone, inner[i], outer[i], inner[next], Vector3.up);
                        mesh.Triangle(1 - tone, outer[i], outer[next], inner[next], Vector3.up);
                    }
                    else
                    {
                        mesh.Triangle(tone, outer[i], inner[i], outer[next], Vector3.up);
                        mesh.Triangle(1 - tone, inner[i], inner[next], outer[next], Vector3.up);
                    }
                }
            }
            return mesh;
        }

        /// <summary>Le socle : ses pans (matériau 0), son biseau et son dessus (matériau 1).</summary>
        public static FacetMesh Pedestal()
        {
            var mesh = new FacetMesh(2);
            Vector3 top = Vector3.up * PedestalHeight;
            for (int i = 0; i < PedestalSides; i++)
            {
                float from = i * 2f * Mathf.PI / PedestalSides, to = (i + 1) * 2f * Mathf.PI / PedestalSides;
                Vector3 outward = Around((from + to) * 0.5f);
                Vector3 foot0 = Around(from) * PedestalFootRadius, foot1 = Around(to) * PedestalFootRadius;
                Vector3 side0 = Around(from) * PedestalSideRadius + Vector3.up * PedestalSideHeight;
                Vector3 side1 = Around(to) * PedestalSideRadius + Vector3.up * PedestalSideHeight;
                Vector3 top0 = Around(from) * PedestalTopRadius + top, top1 = Around(to) * PedestalTopRadius + top;
                mesh.Quad(0, foot0, foot1, side1, side0, outward);
                mesh.Quad(1, side0, side1, top1, top0, outward + Vector3.up);
                mesh.Triangle(1, top, top0, top1, Vector3.up);
            }
            return mesh;
        }

        /// <summary>Le fond : la plinthe (matériau 0), puis des pans de deux tons (matériaux 1 et 2), tournés vers le socle.</summary>
        public static FacetMesh Backdrop()
        {
            var mesh = new FacetMesh(3);
            var random = new System.Random(2);
            int rows = WallHeights.Length;
            var points = new Vector3[rows, WallSegments + 1];
            for (int r = 0; r < rows; r++)
            {
                for (int i = 0; i <= WallSegments; i++)
                {
                    float degrees = Mathf.Lerp(-WallHalfArcDegrees, WallHalfArcDegrees, (float)i / WallSegments);
                    // L'arc tourne autour de -z : derrière la Chimère, vue de la caméra.
                    Vector3 direction = Around((180f + degrees) * Mathf.Deg2Rad);
                    points[r, i] = direction * (WallRadii[r] + Bump(random, WallBump)) + Vector3.up * WallHeights[r];
                }
            }
            for (int r = 0; r < rows - 1; r++)
            {
                for (int i = 0; i < WallSegments; i++)
                {
                    Vector3 middle = (points[r, i] + points[r + 1, i + 1]) * 0.5f;
                    var inward = new Vector3(-middle.x, 0f, -middle.z);
                    int material = r == 0 ? 0 : 1 + i % 2;
                    mesh.Quad(material, points[r, i], points[r, i + 1], points[r + 1, i + 1], points[r + 1, i], inward);
                }
            }
            return mesh;
        }

        // La direction de cet angle (en radians) autour de l'axe vertical, depuis +z vers +x.
        private static Vector3 Around(float radians)
        {
            return new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
        }

        private static float Bump(System.Random random, float amplitude)
        {
            return ((float)random.NextDouble() * 2f - 1f) * amplitude;
        }
    }
}
