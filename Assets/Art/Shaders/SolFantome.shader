// Le fantôme d'un sol en chantier (décision 216) : la texture du calque de sa fiche, posée en coordonnées du monde comme
// le Terrain pose ses calques (la même tuile au même endroit : l'échelle et le décalage de _BaseMap, que la vue calcule
// depuis l'origine du Terrain), mêlée à la teinte des fantômes et translucide : on voit quel sol il sera. Éclairé comme
// le fantôme d'un chantier (Diciela/ChantierFantome), d'en haut puisqu'il est à plat. La part de texture et l'opacité
// sont des réglages du matériau. Sans ombre ; le SRP Batcher le regroupe.
Shader "Diciela/SolFantome"
{
    Properties
    {
        _BaseColor("Teinte du fantôme", Color) = (0.7, 0.85, 1, 1)
        _BaseMap("Texture du sol (tuile en mètres du monde)", 2D) = "white" {}
        _TextureShare("Part de la texture", Range(0.0, 1.0)) = 0.6
        _Opacity("Opacité", Range(0.0, 1.0)) = 0.7
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
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex SolVertex
            #pragma fragment SolFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half _TextureShare;
                half _Opacity;
            CBUFFER_END

            struct SolAttributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct SolVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            SolVaryings SolVertex(SolAttributes input)
            {
                SolVaryings output = (SolVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                // La tuile du calque, en mètres du monde : celle que le Terrain posera au même endroit une fois bâti.
                output.uv = positionWS.xz * _BaseMap_ST.xy + _BaseMap_ST.zw;
                return output;
            }

            half4 SolFragment(SolVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half3 texel = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                half lit = saturate(GetMainLight().direction.y);
                half3 color = lerp(_BaseColor.rgb, texel, _TextureShare) * (0.65 + 0.35 * lit);
                return half4(color, _Opacity);
            }
            ENDHLSL
        }
    }
}
