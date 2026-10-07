using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les mesures de la roue que donne la feuille de style (décisions 176 et 221), lues sur son fond : l'écart au bord de
    /// l'écran (--marge), le rayon du cercle des cases (--rayon), celui des éventails (--rayon-eventail) et l'écart de
    /// leurs options (--ecart), en pixels et en degrés ; et ce qui s'en déduit : le seuil au-delà duquel on vise une
    /// option, et la portée de la roue autour de son centre, éventails compris.
    /// </summary>
    internal sealed class WheelMeasures
    {
        private static readonly CustomStyleProperty<float> EdgeMargin = new CustomStyleProperty<float>("--marge");
        private static readonly CustomStyleProperty<float> SlotRadiusProperty = new CustomStyleProperty<float>("--rayon");
        private static readonly CustomStyleProperty<float> FanRadiusProperty =
            new CustomStyleProperty<float>("--rayon-eventail");
        private static readonly CustomStyleProperty<float> FanStepProperty = new CustomStyleProperty<float>("--ecart");
        // Le demi-feutre d'une case et d'un jeton, grossis quand on les vise : ce qui dépasse de leur cercle.
        private const float SlotOverhang = 46f;
        private const float TokenOverhang = 38f;

        private readonly VisualElement _wheel;

        public WheelMeasures(VisualElement wheel)
        {
            _wheel = wheel;
        }

        public float Margin => Length(EdgeMargin, 0f);
        public float SlotRadius => Length(SlotRadiusProperty, 132f);
        public float FanRadius => Length(FanRadiusProperty, 226f);
        public float FanStep => Length(FanStepProperty, 30f);

        /// <summary>Au-delà de cette distance du centre, on vise une option de l'éventail ouvert : à mi-chemin des cercles.</summary>
        public float FanThreshold => (SlotRadius + FanRadius) * 0.5f;

        /// <summary>La portée de ces cases autour du centre : leur cercle, et celui des éventails s'il y en a.</summary>
        public float Reach(IReadOnlyList<WheelSlot> slots)
        {
            float reach = SlotRadius + SlotOverhang;
            foreach (WheelSlot slot in slots)
            {
                if (slot.Options.Count > 0)
                    return Mathf.Max(reach, FanRadius + TokenOverhang);
            }
            return reach;
        }

        private float Length(CustomStyleProperty<float> property, float fallback)
        {
            return _wheel.customStyle.TryGetValue(property, out float value) ? value : fallback;
        }
    }
}
