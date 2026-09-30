using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Peint le cadre de bois de l'interface (décision 145), en triple résolution, pour une découpe en neuf répétée : de
    /// l'extérieur vers l'intérieur, le liseré, le biseau (clair en haut et à gauche, sombre à droite, nuit en bas, les
    /// pans entre les deux), le bois du pack dont le fil suit chaque traverse (coupe d'onglet aux coins), puis le lin,
    /// son ombre sous la traverse du haut et sa trame. Chaque point se juge à sa profondeur sous le bord aux coins coupés.
    /// </summary>
    internal sealed class FramePainter
    {
        // Les mesures, en pixels de l'interface (maquette « fusion »).
        public const int Resolution = 3;
        public const int Rim = 2;
        public const int Bevel = 3;
        public const int WoodWidth = 7;
        public const int Border = Rim + Bevel + WoodWidth;
        public const float OuterCut = 19f;
        // Un coin de la découpe contient le pan du lin (12 px sous le bord, 12 px de pan) ; le pas des bords et du milieu
        // est un multiple des pas de la trame, pour qu'elle se raccorde en se répétant.
        public const int Corner = 28;
        public const int Tile = 150;
        public const int Size = 2 * Corner + Tile;
        private const int ShadowBand = 3;
        private const int WeftStep = 3;
        private const int WarpStep = 5;
        private const int SlubLength = 7;
        private const float SlubFloor = 0.2f;
        private const int Samples = 4;
        private static readonly float Sqrt2 = Mathf.Sqrt(2f);

        // Les distances d'un point aux huit bords, dans l'ordre des biseaux ; réutilisées d'un point à l'autre.
        private readonly float[] _edges = new float[8];
        private readonly PackWood _wood;
        private readonly Color _rim;
        private readonly Color[] _bevels;
        private readonly Color _linen;
        private readonly Color _linenShadow;
        private readonly Color _weft;
        private readonly Color _warp;

        public FramePainter(UssPalette palette, PackWood wood)
        {
            _wood = wood;
            _rim = palette.Get("bois-nuit");
            Color light = palette.Get("bois-clair");
            Color dark = palette.Get("bois-sombre");
            Color night = palette.Get("bois-nuit");
            // Dans l'ordre des bords : haut, gauche, droite, bas, puis les pans haut-gauche, haut-droit, bas-droit,
            // bas-gauche, chacun entre ses deux voisins.
            _bevels = new[]
            {
                light, light, dark, night, light, Color.Lerp(light, dark, 0.5f), Color.Lerp(dark, night, 0.5f),
                Color.Lerp(light, night, 0.5f)
            };
            _linen = palette.Get("lin");
            _linenShadow = palette.Get("lin-ombre");
            _weft = palette.Get("trame-trait");
            _warp = palette.Get("trame-chaine");
        }

        /// <summary>L'image entière, lissée (plusieurs points par pixel, moyennés en couleurs prémultipliées).</summary>
        public Color[] Paint()
        {
            int pixels = Size * Resolution;
            var image = new Color[pixels * pixels];
            float step = 1f / (Resolution * Samples);
            for (int row = 0; row < pixels; row++)
            {
                for (int column = 0; column < pixels; column++)
                {
                    var sum = Vector4.zero;
                    for (int sy = 0; sy < Samples; sy++)
                    {
                        for (int sx = 0; sx < Samples; sx++)
                        {
                            float x = (column * Samples + sx + 0.5f) * step;
                            float y = (row * Samples + sy + 0.5f) * step;
                            Color c = At(x, y);
                            sum += new Vector4(c.r * c.a, c.g * c.a, c.b * c.a, c.a);
                        }
                    }
                    sum /= Samples * Samples;
                    Color pixel = sum.w > 0f ? new Color(sum.x / sum.w, sum.y / sum.w, sum.z / sum.w, sum.w) : Color.clear;
                    // Les lignes d'une texture Unity comptent depuis le bas.
                    image[(pixels - 1 - row) * pixels + column] = pixel;
                }
            }
            return image;
        }

        // La couleur au point (x, y), en pixels de l'interface depuis le coin haut-gauche.
        private Color At(float x, float y)
        {
            float s = Size;
            float c = OuterCut;
            float top = y, left = x, right = s - x, bottom = s - y;
            float[] edges = _edges;
            edges[0] = top;
            edges[1] = left;
            edges[2] = right;
            edges[3] = bottom;
            edges[4] = (x + y - c) / Sqrt2;
            edges[5] = (s - x + y - c) / Sqrt2;
            edges[6] = (2f * s - x - y - c) / Sqrt2;
            edges[7] = (x + s - y - c) / Sqrt2;
            int nearest = 0;
            for (int i = 1; i < edges.Length; i++)
            {
                if (edges[i] < edges[nearest])
                    nearest = i;
            }
            float depth = edges[nearest];
            if (depth < 0f)
                return Color.clear;
            if (depth < Rim)
                return _rim;
            if (depth < Rim + Bevel)
                return _bevels[nearest];
            if (depth < Border)
                return Wood(x, y, top, left, right, bottom);
            return Linen(x, y);
        }

        // Le fil du bois suit la traverse la plus proche ; la coupe d'onglet des coins tombe sur leur diagonale.
        private Color Wood(float x, float y, float top, float left, float right, float bottom)
        {
            float inner = Rim + Bevel;
            if (Mathf.Min(top, bottom) <= Mathf.Min(left, right))
                return top <= bottom ? _wood.At(0, Along(x), top - inner) : _wood.At(1, Along(x), bottom - inner);
            return left <= right ? _wood.At(2, Along(y), left - inner) : _wood.At(3, Along(y), right - inner);
        }

        // La position le long d'une traverse, ramenée au pas des bords : les coins continuent les bords.
        private static float Along(float position)
        {
            return Mathf.Repeat(position - Corner, Tile);
        }

        private Color Linen(float x, float y)
        {
            Color color = _linen;
            if (y < Border + ShadowBand)
                color = Over(color, _linenShadow);
            int row = Mathf.FloorToInt(y);
            int column = Mathf.FloorToInt(x);
            if (row % WeftStep == 0)
                color = Over(color, Slub(_weft, row, column / SlubLength));
            if (column % WarpStep == 0)
                color = Over(color, Slub(_warp, column, row / SlubLength + 1000));
            return color;
        }

        // Un lin, pas un quadrillage : chaque fil a sa teinte, qui varie le long du fil par tronçons (les flammes du lin).
        private static Color Slub(Color thread, int line, int stretch)
        {
            unchecked
            {
                uint hash = (uint)(line * 73856093) ^ (uint)(stretch * 19349663);
                hash ^= hash >> 13;
                hash *= 0x5bd1e995;
                hash ^= hash >> 15;
                thread.a *= SlubFloor + (1f - SlubFloor) * (hash & 0xFFFF) / 65535f;
            }
            return thread;
        }

        private static Color Over(Color under, Color over)
        {
            Color mixed = Color.Lerp(under, over, over.a);
            mixed.a = under.a;
            return mixed;
        }
    }
}
