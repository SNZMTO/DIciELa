using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La ferrure de laiton d'un coin du cadre de bois (décision 163) : une plaque en travers de la coupe d'onglet, au
    /// milieu du bois, son biseau clair du côté de la lumière, et un clou en son milieu. Toute entière dans la case du
    /// coin du cadre (FramePainter.Corner) : la découpe en neuf ne la répète pas le long des bords. Ses couleurs sont les
    /// jetons du laiton de Fusion.uss.
    /// </summary>
    internal sealed class BrassFitting
    {
        // La ferrure : sa demi-longueur le long du biseau, sa demi-largeur en travers, au milieu du bois ; son biseau et
        // son clou.
        private const float PlateHalfLength = 8f;
        private const float PlateHalfWidth = 4.5f;
        private const float PlateBevel = 1f;
        private const float NailRadius = 2f;
        private const float PlateDepth = FramePainter.Rim + FramePainter.Bevel + FramePainter.WoodWidth * 0.5f;
        private static readonly float Sqrt2 = Mathf.Sqrt(2f);

        private readonly Color _brass;
        private readonly Color _brassLight;
        private readonly Color _brassShadow;

        public BrassFitting(UssPalette palette)
        {
            _brass = palette.Get("laiton");
            _brassLight = palette.Get("laiton-clair");
            _brassShadow = palette.Get("laiton-ombre");
        }

        /// <summary>
        /// La couleur de la ferrure au point (x, y), en pixels de l'interface depuis le coin haut-gauche du cadre ; faux
        /// hors d'elle. Ramenée au coin haut-gauche ; la lumière vient d'en haut à gauche de l'écran, quel que soit le
        /// coin : un côté de plaque ou de clou tourné vers elle est clair, l'autre sombre.
        /// </summary>
        public bool TryPaint(float x, float y, out Color color)
        {
            color = default;
            float s = FramePainter.Size;
            float sx = x > s * 0.5f ? -1f : 1f;
            float sy = y > s * 0.5f ? -1f : 1f;
            float fx = sx > 0f ? x : s - x;
            float fy = sy > 0f ? y : s - y;
            if (fx > FramePainter.Corner || fy > FramePainter.Corner)
                return false;
            float middle = (FramePainter.OuterCut + PlateDepth * Sqrt2) * 0.5f;
            var offset = new Vector2(fx - middle, fy - middle);
            float along = (offset.x - offset.y) / Sqrt2;
            float across = (offset.x + offset.y) / Sqrt2;
            if (Mathf.Abs(along) > PlateHalfLength || Mathf.Abs(across) > PlateHalfWidth)
                return false;
            // Dans l'écran, la lumière vient de (-1, -1) : un côté qui regarde par là est clair.
            var toLight = new Vector2(-1f, -1f).normalized;
            if (offset.magnitude < NailRadius)
            {
                var nail = new Vector2(offset.x * sx, offset.y * sy);
                float lit = 0.5f + 0.5f * Vector2.Dot(nail.normalized, toLight) * Mathf.Clamp01(nail.magnitude);
                color = Color.Lerp(_brassShadow, _brassLight, lit);
                return true;
            }
            float alongEdge = PlateHalfLength - Mathf.Abs(along);
            float acrossEdge = PlateHalfWidth - Mathf.Abs(across);
            if (Mathf.Min(alongEdge, acrossEdge) >= PlateBevel)
            {
                color = _brass;
                return true;
            }
            Vector2 side = alongEdge < acrossEdge
                ? new Vector2(1f, -1f) * Mathf.Sign(along)
                : new Vector2(1f, 1f) * Mathf.Sign(across);
            var onScreen = new Vector2(side.x * sx, side.y * sy);
            color = Vector2.Dot(onScreen, toLight) > 0f ? _brassLight : _brassShadow;
            return true;
        }
    }
}
