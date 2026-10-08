using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La peinture de la carte, lue à la finesse de sa copie (décision 233 ; SAVOIR, 2.5) : la copie du sol, où se
    /// peignent les sols, est plus fine que la carte, pour que leurs bords s'arrondissent ; chaque texel de la copie
    /// prend la peinture de la carte en son milieu, entre les quatre texels de la carte autour de lui, comme le Terrain
    /// la montre. Elle affine la copie à l'ouverture, et rend la peinture de la carte là où un sol s'en va. La carte,
    /// elle, n'est jamais touchée.
    /// </summary>
    internal static class MapPaint
    {
        // Le côté des morceaux qu'on affine d'un coup, en texels de la copie : de quoi borner la mémoire prise.
        private const int Chunk = 256;

        // La plus grande peinture qu'un Terrain accepte.
        private const int MaxResolution = 4096;

        /// <summary>
        /// La résolution de peinture qui donne au plus cette taille de texel, en mètres, sur ce Terrain : une puissance
        /// de deux, jamais moins fine que celle qu'il a.
        /// </summary>
        public static int ResolutionFor(TerrainData data, float texelMeters)
        {
            float side = Mathf.Max(data.size.x, data.size.z);
            int wanted = Mathf.NextPowerOfTwo(Mathf.CeilToInt(side / Mathf.Max(texelMeters, 0.01f)));
            return Mathf.Clamp(wanted, data.alphamapResolution, MaxResolution);
        }

        /// <summary>
        /// Affine la copie à cette résolution, sa peinture reprise de la carte, morceau par morceau ; rien si elle l'a
        /// déjà.
        /// </summary>
        public static void Refine(TerrainData copy, TerrainData original, int resolution)
        {
            if (copy.alphamapResolution == resolution)
                return;
            copy.alphamapResolution = resolution;
            for (int y = 0; y < resolution; y += Chunk)
            {
                for (int x = 0; x < resolution; x += Chunk)
                {
                    var texels = new RectInt(x, y, Mathf.Min(Chunk, resolution - x), Mathf.Min(Chunk, resolution - y));
                    copy.SetAlphamaps(x, y, Read(original, resolution, texels));
                }
            }
        }

        /// <summary>
        /// Les poids de la peinture de la carte pour ces texels d'une copie de cette résolution, rangés comme ceux de
        /// GetAlphamaps (ligne, colonne, calque).
        /// </summary>
        public static float[,,] Read(TerrainData original, int resolution, RectInt texels)
        {
            int from = original.alphamapResolution;
            if (from == resolution)
                return original.GetAlphamaps(texels.x, texels.y, texels.width, texels.height);
            float scale = (float)from / resolution;
            int x0 = Mathf.Clamp(Mathf.FloorToInt(Source(texels.xMin, scale)), 0, from - 1);
            int x1 = Mathf.Clamp(Mathf.FloorToInt(Source(texels.xMax - 1, scale)) + 1, 0, from - 1);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(Source(texels.yMin, scale)), 0, from - 1);
            int y1 = Mathf.Clamp(Mathf.FloorToInt(Source(texels.yMax - 1, scale)) + 1, 0, from - 1);
            float[,,] map = original.GetAlphamaps(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
            int layers = map.GetLength(2);
            var weights = new float[texels.height, texels.width, layers];
            for (int j = 0; j < texels.height; j++)
            {
                Between(Source(texels.yMin + j, scale) - y0, y1 - y0, out int below, out int above, out float v);
                for (int i = 0; i < texels.width; i++)
                {
                    Between(Source(texels.xMin + i, scale) - x0, x1 - x0, out int left, out int right, out float u);
                    for (int layer = 0; layer < layers; layer++)
                    {
                        float bottom = Mathf.Lerp(map[below, left, layer], map[below, right, layer], u);
                        float top = Mathf.Lerp(map[above, left, layer], map[above, right, layer], u);
                        weights[j, i, layer] = Mathf.Lerp(bottom, top, v);
                    }
                }
            }
            return weights;
        }

        // Le texel de la carte, en continu, sous le milieu de ce texel de la copie.
        private static float Source(int texel, float scale)
        {
            return (texel + 0.5f) * scale - 0.5f;
        }

        // Les deux texels lus autour de cette place (de 0 à last), et la part du second, bornés aux bords.
        private static void Between(float at, int last, out int low, out int high, out float share)
        {
            at = Mathf.Clamp(at, 0f, last);
            low = Mathf.Min(Mathf.FloorToInt(at), last);
            high = Mathf.Min(low + 1, last);
            share = at - low;
        }
    }
}
