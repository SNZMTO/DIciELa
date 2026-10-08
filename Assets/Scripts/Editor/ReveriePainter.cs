using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Peint le voile de rêverie de la prévision (décisions 228 et 230), une image posée sur tout l'écran, sous le
    /// HUD : la lavande pâle de la feuille de style (--reverie), presque transparente au milieu, de plus en plus dense
    /// vers les bords, en rectangle aux coins ronds. L'image s'étire à l'écran : sa forme suit ses proportions. Un
    /// dégradé ne demande pas de finesse.
    /// </summary>
    internal sealed class ReveriePainter
    {
        public const int Size = 512;

        // L'opacité du voile au milieu de l'écran, puis à ses bords ; le fondu de l'un à l'autre commence à cette part
        // du chemin du milieu au bord et finit un peu au-delà, dans les coins. La puissance arrondit les coins du
        // rectangle.
        private const float MiddleOpacity = 0.06f;
        private const float EdgeOpacity = 0.5f;
        private const float FadeStart = 0.62f;
        private const float FadeEnd = 1.1f;
        private const float Roundness = 4f;

        private readonly Color _color;

        public ReveriePainter(UssPalette palette)
        {
            _color = palette.Get("reverie");
        }

        public Color[] Paint()
        {
            var image = new Color[Size * Size];
            for (int row = 0; row < Size; row++)
            {
                float v = (row + 0.5f) / Size * 2f - 1f;
                for (int column = 0; column < Size; column++)
                {
                    float u = (column + 0.5f) / Size * 2f - 1f;
                    float distance = Mathf.Pow(Mathf.Pow(Mathf.Abs(u), Roundness) + Mathf.Pow(Mathf.Abs(v), Roundness),
                        1f / Roundness);
                    float fade = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(FadeStart, FadeEnd, distance));
                    Color pixel = _color;
                    pixel.a = Mathf.Lerp(MiddleOpacity, EdgeOpacity, fade);
                    image[row * Size + column] = pixel;
                }
            }
            return image;
        }
    }
}
