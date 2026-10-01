using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Ce qu'une face (Facet) lit dans la feuille de style (Fusion.uss) : ses propriétés à elle, toutes en pixels ou en
    /// couleurs. Une propriété absente vaut rien : pas d'ombre, pas de liseré, pas de relief, pas de creux, pas de couture,
    /// pas de grain, pas de trame.
    /// </summary>
    public struct FacetLook
    {
        private static readonly CustomStyleProperty<float> CutProperty = new CustomStyleProperty<float>("--pan");
        private static readonly CustomStyleProperty<Color> ShadowProperty = new CustomStyleProperty<Color>("--ombre");
        private static readonly CustomStyleProperty<float> ShadowOffsetProperty =
            new CustomStyleProperty<float>("--ombre-y");
        private static readonly CustomStyleProperty<Color> FaceProperty = new CustomStyleProperty<Color>("--face");
        private static readonly CustomStyleProperty<Color> RimProperty = new CustomStyleProperty<Color>("--lisere");
        private static readonly CustomStyleProperty<float> RimWidthProperty =
            new CustomStyleProperty<float>("--lisere-largeur");
        private static readonly CustomStyleProperty<Color> LipProperty = new CustomStyleProperty<Color>("--levre");
        private static readonly CustomStyleProperty<float> LipHeightProperty =
            new CustomStyleProperty<float>("--levre-hauteur");
        private static readonly CustomStyleProperty<float> ShineHeightProperty =
            new CustomStyleProperty<float>("--reflet-hauteur");
        private static readonly CustomStyleProperty<Color> ShineTintProperty =
            new CustomStyleProperty<Color>("--reflet-couleur");
        private static readonly CustomStyleProperty<float> ShineShareProperty =
            new CustomStyleProperty<float>("--reflet-part");
        private static readonly CustomStyleProperty<float> PressedProperty = new CustomStyleProperty<float>("--enfonce");
        private static readonly CustomStyleProperty<Color> HollowShadowProperty =
            new CustomStyleProperty<Color>("--creux-ombre");
        private static readonly CustomStyleProperty<float> HollowOffsetProperty =
            new CustomStyleProperty<float>("--creux-decalage");
        private static readonly CustomStyleProperty<float> HollowBlurProperty =
            new CustomStyleProperty<float>("--creux-flou");
        private static readonly CustomStyleProperty<Color> HollowLightProperty =
            new CustomStyleProperty<Color>("--creux-lumiere");
        private static readonly CustomStyleProperty<float> StitchedProperty = new CustomStyleProperty<float>("--couture");
        private static readonly CustomStyleProperty<Color> StitchProperty = new CustomStyleProperty<Color>("--fil");
        private static readonly CustomStyleProperty<float> StitchInsetProperty =
            new CustomStyleProperty<float>("--fil-retrait");
        private static readonly CustomStyleProperty<float> StitchWidthProperty =
            new CustomStyleProperty<float>("--fil-epaisseur");
        private static readonly CustomStyleProperty<float> DashProperty = new CustomStyleProperty<float>("--fil-trait");
        private static readonly CustomStyleProperty<float> GapProperty = new CustomStyleProperty<float>("--fil-espace");
        private static readonly CustomStyleProperty<float> GrainProperty = new CustomStyleProperty<float>("--grain");
        private static readonly CustomStyleProperty<float> GrainSpacingProperty =
            new CustomStyleProperty<float>("--grain-pas");
        private static readonly CustomStyleProperty<Color> WeftProperty = new CustomStyleProperty<Color>("--tissage-trame");
        private static readonly CustomStyleProperty<Color> WarpProperty =
            new CustomStyleProperty<Color>("--tissage-chaine");
        private static readonly CustomStyleProperty<float> WeftStepProperty =
            new CustomStyleProperty<float>("--tissage-trame-pas");
        private static readonly CustomStyleProperty<float> WarpStepProperty =
            new CustomStyleProperty<float>("--tissage-chaine-pas");

        public float Cut;
        public Color Shadow;
        public float ShadowOffset;
        public Color Face;
        public Color Rim;
        public float RimWidth;
        public Color Lip;
        public float LipHeight;
        public Color Shine;
        public float ShineHeight;
        public bool Pressed;
        public Color HollowShadow;
        public float HollowOffset;
        public float HollowBlur;
        public Color HollowLight;
        public Color Stitch;
        public float StitchInset;
        public float StitchWidth;
        public float Dash;
        public float Gap;
        public float Grain;
        public float GrainSpacing;
        public Color Weft;
        public Color Warp;
        public float WeftStep;
        public float WarpStep;

        public bool HasShadow => Shadow.a > 0f && ShadowOffset > 0f;
        public bool HasRim => RimWidth > 0f && Rim.a > 0f;
        public bool HasRelief => Lip.a > 0f && LipHeight > 0f;
        public bool IsHollow => HollowShadow.a > 0f;
        public bool IsStitched => Stitch.a > 0f && StitchWidth > 0f && Dash > 0f;
        public bool HasGrain => Grain > 0f && GrainSpacing > 0f;
        public bool IsWoven => (Weft.a > 0f && WeftStep >= 1f) || (Warp.a > 0f && WarpStep >= 1f);

        /// <summary>Lit les propriétés résolues pour une face.</summary>
        public static FacetLook Read(ICustomStyle style)
        {
            var look = new FacetLook
            {
                Cut = Number(style, CutProperty),
                Shadow = Colour(style, ShadowProperty),
                ShadowOffset = Number(style, ShadowOffsetProperty),
                Face = Colour(style, FaceProperty),
                Rim = Colour(style, RimProperty),
                RimWidth = Number(style, RimWidthProperty),
                Lip = Colour(style, LipProperty),
                LipHeight = Number(style, LipHeightProperty),
                ShineHeight = Number(style, ShineHeightProperty),
                Pressed = Number(style, PressedProperty) > 0f,
                HollowShadow = Colour(style, HollowShadowProperty),
                HollowOffset = Number(style, HollowOffsetProperty),
                HollowBlur = Number(style, HollowBlurProperty),
                HollowLight = Colour(style, HollowLightProperty),
                StitchInset = Number(style, StitchInsetProperty),
                StitchWidth = Number(style, StitchWidthProperty),
                Dash = Number(style, DashProperty),
                Gap = Number(style, GapProperty),
                Grain = Number(style, GrainProperty),
                GrainSpacing = Number(style, GrainSpacingProperty),
                Weft = Colour(style, WeftProperty),
                Warp = Colour(style, WarpProperty),
                WeftStep = Number(style, WeftStepProperty),
                WarpStep = Number(style, WarpStepProperty)
            };
            // Le reflet se déduit de la face : il la suit quand le survol ou l'appui la change.
            look.Shine = Color.Lerp(look.Face, Colour(style, ShineTintProperty), Number(style, ShineShareProperty));
            // La couture ne se coud que sur les grands boutons (--couture), de la couleur de fil de leur pastel.
            look.Stitch = Number(style, StitchedProperty) > 0f ? Colour(style, StitchProperty) : Color.clear;
            return look;
        }

        private static float Number(ICustomStyle style, CustomStyleProperty<float> property)
        {
            return style.TryGetValue(property, out float value) ? value : 0f;
        }

        private static Color Colour(ICustomStyle style, CustomStyleProperty<Color> property)
        {
            return style.TryGetValue(property, out Color value) ? value : Color.clear;
        }
    }
}
