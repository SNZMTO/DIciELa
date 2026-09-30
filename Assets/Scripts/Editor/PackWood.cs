using System.IO;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Le bois du pack pour le cadre de l'interface : les planches de l'atlas, lues le long d'une traverse, une bande par
    /// traverse. Le motif se raccorde d'un pas au suivant par un fondu. L'atlas est lu comme un fichier : son asset, dans
    /// le pack, n'est jamais touché.
    /// </summary>
    internal sealed class PackWood
    {
        private const string AtlasPath =
            "Assets/Synty/PolygonFantasyVillage/Textures/Alts/PolygonFantasyVillage_01_A.png";

        // Les planches dans l'atlas, en pixels depuis son coin haut-gauche, sans les marques voisines.
        private static readonly RectInt Planks = new RectInt(2575, 2960, 1515, 340);
        // 380 px d'interface pour toute la largeur des planches, comme dans la maquette.
        private const float AtlasPerPixel = 4f;
        // Le fil droit du milieu des planches, loin de leurs bouts en crosse.
        private const float Start = 300f;
        // Une bande par traverse (haut, bas, gauche, droite), à des hauteurs différentes des planches.
        private static readonly float[] RailRows = { 50f, 150f, 100f, 260f };
        // Le raccord d'un pas au suivant, en pixels de l'interface.
        private const float Fade = 25f;

        private readonly Color[] _pixels;
        private readonly float _period;

        private PackWood(Color[] pixels, float period)
        {
            _pixels = pixels;
            _period = period;
        }

        /// <summary>Lit les planches dans l'atlas du pack ; period est le pas des traverses, en pixels d'interface.</summary>
        public static PackWood Load(float period)
        {
            if (!File.Exists(AtlasPath))
                throw new FileNotFoundException("L'atlas du pack manque (procédure pack-synty) : " + AtlasPath);
            var atlas = new Texture2D(2, 2);
            atlas.LoadImage(File.ReadAllBytes(AtlasPath));
            // Les lignes d'une texture comptent depuis le bas : on retourne pour lire depuis le haut.
            Color[] bottomUp = atlas.GetPixels(Planks.x, atlas.height - Planks.y - Planks.height, Planks.width,
                Planks.height);
            Object.DestroyImmediate(atlas);
            var pixels = new Color[bottomUp.Length];
            for (int row = 0; row < Planks.height; row++)
                System.Array.Copy(bottomUp, (Planks.height - 1 - row) * Planks.width, pixels, row * Planks.width,
                    Planks.width);
            return new PackWood(pixels, period);
        }

        /// <summary>
        /// Le bois d'une traverse : along, la position le long d'elle dans son pas ; across, la profondeur dans la
        /// bande de bois, en pixels d'interface.
        /// </summary>
        public Color At(int rail, float along, float across)
        {
            Color color = Sample(rail, along, across);
            float fadeStart = _period - Fade;
            if (along <= fadeStart)
                return color;
            return Color.Lerp(color, Sample(rail, along - _period, across), (along - fadeStart) / Fade);
        }

        private Color Sample(int rail, float along, float across)
        {
            float u = Start + along * AtlasPerPixel;
            float v = RailRows[rail] + across * AtlasPerPixel;
            int x = Mathf.Clamp(Mathf.FloorToInt(u), 0, Planks.width - 2);
            int y = Mathf.Clamp(Mathf.FloorToInt(v), 0, Planks.height - 2);
            float fx = Mathf.Clamp01(u - x);
            float fy = Mathf.Clamp01(v - y);
            Color top = Color.Lerp(_pixels[y * Planks.width + x], _pixels[y * Planks.width + x + 1], fx);
            Color low = Color.Lerp(_pixels[(y + 1) * Planks.width + x], _pixels[(y + 1) * Planks.width + x + 1], fx);
            return Color.Lerp(top, low, fy);
        }
    }
}
