using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Peint l'ombre portée du cadre de bois (décision 145), une image à part posée sous lui : une ombre dure décalée
    /// vers le bas, puis une ombre douce, floue, de l'ensemble, comme la maquette (deux ombres enchaînées). Un flou ne
    /// demande pas de finesse : l'image est à la taille de l'interface.
    /// </summary>
    internal sealed class ShadowPainter
    {
        // La marge autour du cadre : elle contient tout le flou et les décalages.
        public const int Margin = 44;
        public const int Size = FramePainter.Size + 2 * Margin;
        public const int Corner = FramePainter.Corner + Margin;
        private const float HardOffset = 5f;
        private const float SoftOffset = 12f;
        // Le flou de la maquette (16 px) est un rayon : son écart type en est la moitié.
        private const float SoftSigma = 8f;
        private const int Samples = 4;

        private readonly Color _hard;
        private readonly Color _soft;

        public ShadowPainter(UssPalette palette)
        {
            _hard = palette.Get("cadre-ombre-dure");
            _soft = palette.Get("cadre-ombre-douce");
        }

        public Color[] Paint()
        {
            float[] shape = Silhouette(0f);
            float[] lowered = Silhouette(HardOffset);
            var hard = new float[shape.Length];
            var joined = new float[shape.Length];
            for (int i = 0; i < shape.Length; i++)
            {
                hard[i] = lowered[i] * _hard.a;
                joined[i] = shape[i] + (1f - shape[i]) * hard[i];
            }
            float[] soft = Blur(Shift(joined, SoftOffset));
            var image = new Color[shape.Length];
            for (int row = 0; row < Size; row++)
            {
                for (int column = 0; column < Size; column++)
                {
                    int i = row * Size + column;
                    float a = hard[i] + soft[i] * _soft.a * (1f - hard[i]);
                    Color color = a > 0f ? Color.Lerp(_soft, _hard, hard[i] / a) : _hard;
                    color.a = a;
                    // Les lignes d'une texture Unity comptent depuis le bas.
                    image[(Size - 1 - row) * Size + column] = color;
                }
            }
            return image;
        }

        // La part de chaque pixel couverte par le cadre, descendu de offset pixels.
        private static float[] Silhouette(float offset)
        {
            var coverage = new float[Size * Size];
            float step = 1f / Samples;
            for (int row = 0; row < Size; row++)
            {
                for (int column = 0; column < Size; column++)
                {
                    int inside = 0;
                    for (int sy = 0; sy < Samples; sy++)
                    {
                        for (int sx = 0; sx < Samples; sx++)
                        {
                            float x = column + (sx + 0.5f) * step - Margin;
                            float y = row + (sy + 0.5f) * step - Margin - offset;
                            if (Inside(x, y))
                                inside++;
                        }
                    }
                    coverage[row * Size + column] = inside / (float)(Samples * Samples);
                }
            }
            return coverage;
        }

        private static bool Inside(float x, float y)
        {
            float s = FramePainter.Size;
            float c = FramePainter.OuterCut;
            return x >= 0f && y >= 0f && x <= s && y <= s && x + y >= c && s - x + y >= c && x + s - y >= c &&
                   2f * s - x - y >= c;
        }

        private static float[] Shift(float[] values, float offset)
        {
            var shifted = new float[values.Length];
            int rows = Mathf.RoundToInt(offset);
            for (int row = rows; row < Size; row++)
                System.Array.Copy(values, (row - rows) * Size, shifted, row * Size, Size);
            return shifted;
        }

        // Un flou gaussien, en deux passes (lignes puis colonnes), sur trois écarts types.
        private static float[] Blur(float[] values)
        {
            int radius = Mathf.CeilToInt(3f * SoftSigma);
            var kernel = new float[2 * radius + 1];
            float total = 0f;
            for (int i = -radius; i <= radius; i++)
            {
                kernel[i + radius] = Mathf.Exp(-i * i / (2f * SoftSigma * SoftSigma));
                total += kernel[i + radius];
            }
            for (int i = 0; i < kernel.Length; i++)
                kernel[i] /= total;
            return Pass(Pass(values, kernel, radius, 1, Size), kernel, radius, Size, 1);
        }

        private static float[] Pass(float[] values, float[] kernel, int radius, int along, int across)
        {
            var result = new float[values.Length];
            for (int line = 0; line < Size; line++)
            {
                for (int position = 0; position < Size; position++)
                {
                    float sum = 0f;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int p = position + k;
                        if (p >= 0 && p < Size)
                            sum += values[line * across + p * along] * kernel[k + radius];
                    }
                    result[line * across + position * along] = sum;
                }
            }
            return result;
        }
    }
}
