// Le fantôme d'un chantier qui monte (décision 166) : translucide au-dessus de la ligne, il s'efface dans la bande sous
// elle, où la vraie matière s'effrite (Diciela/Chantier) : on voit l'une à travers l'autre. Sur la ligne qui ondule,
// une lueur douce (le flou), un trait net juste au-dessus (le côté cartoon) et des étincelles qui clignotent. Sans ombre.
// Le fantôme d'un matériau qui détoure (des feuilles, le bord d'un chaume) a sa copie (SiteGhosts) : sa texture ne sert
// qu'à sa forme, tiges comprises (ChantierDetourage). Sans ligne (sa hauteur tout en bas, SiteLine.Nowhere), c'est un
// simple fantôme teinté : l'aperçu de pose s'en sert (décision 177). Le SRP Batcher le regroupe (ChantierLigne).
Shader "Diciela/ChantierFantome"
{
    Properties
    {
        _BaseColor("Teinte du fantôme", Color) = (0.7, 0.85, 1, 0.45)
        _BaseMap("Forme (détourage)", 2D) = "white" {}
        [NoScaleOffset] _TrunkMap("Forme des tiges (feuillage du pack)", 2D) = "white" {}
        _Cutoff("Seuil du détourage", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _Cull("__cull", Float) = 2.0
        _GlowColor("Lueur", Color) = (1, 0.85, 0.5, 0.9)
        _GlowWidth("Largeur de la lueur (m)", Float) = 0.14
        _StrokeColor("Trait", Color) = (1, 0.97, 0.85, 1)
        _StrokeWidth("Épaisseur du trait (m)", Float) = 0.03
        _SparkleColor("Étincelles", Color) = (1, 0.95, 0.75, 1)
        _SparkleSize("Pas des étincelles (m)", Float) = 0.22
        _SparkleShare("Part des étincelles", Range(0, 1)) = 0.12
        _SparkleReach("Hauteur des étincelles (m)", Float) = 0.45
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ChantierVertex
            #pragma fragment ChantierGhostFragment
            #pragma multi_compile_local_fragment _ _ALPHATEST_ON
            #pragma dynamic_branch_local _TRUNKMAP
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Art/Shaders/ChantierLigne.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half _Cutoff;
                half4 _GlowColor;
                float _GlowWidth;
                half4 _StrokeColor;
                float _StrokeWidth;
                half4 _SparkleColor;
                float _SparkleSize;
                float _SparkleShare;
                float _SparkleReach;
            CBUFFER_END

            #include "Assets/Art/Shaders/ChantierDetourage.hlsl"
            #include "Assets/Art/Shaders/ChantierSommets.hlsl"

            // Un nombre de 0 à 1 tiré d'une case de l'espace : toujours le même pour la même case.
            float ChantierHash(float3 cell)
            {
                return frac(sin(dot(cell, float3(127.1, 311.7, 74.7))) * 43758.5453);
            }

            half4 ChantierGhostFragment(ChantierVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                ChantierClipAlpha(input.uv, input.leaf);
                float h = input.positionWS.y - ChantierLine(input.positionWS);
                // Sous la bande, la vraie matière est entière : le fantôme n'y est pas.
                clip(h + _Softness);

                Light mainLight = GetMainLight();
                half lit = saturate(dot(normalize(float3(input.normalWS)), mainLight.direction));
                half3 color = _BaseColor.rgb * (0.65 + 0.35 * lit);
                // Dans la bande, le fantôme s'efface vers le bas : on voit la vraie matière à travers.
                half alpha = _BaseColor.a * saturate((h + _Softness) / max(_Softness, 0.001));

                // La lueur : douce, de part et d'autre de la ligne.
                half glow = exp(-(h * h) / max(_GlowWidth * _GlowWidth, 1e-5)) * _GlowColor.a;
                color = lerp(color, _GlowColor.rgb, glow);
                alpha = max(alpha, glow);

                // Le trait : net, juste au-dessus de la ligne, qui ondule avec elle.
                half stroke = step(0.0, h) * step(h, _StrokeWidth) * _StrokeColor.a;
                color = lerp(color, _StrokeColor.rgb, stroke);
                alpha = max(alpha, stroke);

                // Les étincelles : quelques cases de l'espace, au-dessus de la ligne, chacune à son rythme.
                float3 position = input.positionWS / max(_SparkleSize, 0.01);
                float3 cell = floor(position);
                float pick = ChantierHash(cell);
                half spot = 1.0 - smoothstep(0.08, 0.16, length(frac(position) - 0.5));
                half twinkle = 0.5 + 0.5 * sin(_Time.y * 4.0 + pick * 40.0);
                half reach = step(0.0, h) * (1.0 - saturate(h / max(_SparkleReach, 0.01)));
                half sparkle = step(1.0 - _SparkleShare, pick) * spot * twinkle * reach * _SparkleColor.a;
                color = lerp(color, _SparkleColor.rgb, sparkle);
                alpha = max(alpha, sparkle);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
