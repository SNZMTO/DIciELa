// La vraie matière d'un chantier qui monte (décision 166) : le Lit d'URP (sa passe avant reprise telle quelle, avec la
// même entrée LitInput), coupé au-dessus de la ligne du chantier et effrité dans la bande sous elle. Les ombres, la
// profondeur et les normales coupent au même endroit : la moitié qui n'est pas bâtie ne fait ni ombre ni occlusion. Un
// matériau qui détoure (des feuilles) détoure partout, au seuil de l'original (_ALPHATEST_ON, ChantierDetourage).
// Les matériaux du pack y sont recopiés à l'exécution (SiteMaterials) ; la ligne se règle par chantier (SiteFill).
Shader "Diciela/Chantier"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.2
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        [ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0
        _Cull("__cull", Float) = 2.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _FillHeight("Fill Height", Float) = -10000
        [HideInInspector] _WaveAmplitude("Wave Amplitude", Float) = 0.08
        [HideInInspector] _WaveLength("Wave Length", Float) = 1.6
        [HideInInspector] _WaveSeconds("Wave Seconds", Float) = 3.0
        [HideInInspector] _Softness("Softness", Float) = 0.25
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex LitPassVertex
            #pragma fragment ChantierForwardFragment

            // Ces trois mots-clés ne sont posés qu'à l'exécution, sur les copies des matériaux d'origine (SiteMaterials) :
            // en shader_feature, un jeu compilé n'aurait que les variantes de Chantier.mat, qui n'en a aucun.
            #pragma multi_compile_local _ _NORMALMAP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma multi_compile_local_fragment _ _EMISSION
            #pragma multi_compile_local_fragment _ _ALPHATEST_ON

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"

            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer

            // La passe avant d'URP garde la position dans le monde : la ligne en a besoin.
            #define REQUIRES_WORLD_SPACE_POS_INTERPOLATOR
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
            #include "Assets/Art/Shaders/ChantierLigne.hlsl"

            void ChantierForwardFragment(
                Varyings input
                , out half4 outColor : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
            #endif
            )
            {
                ChantierClipBuilt(input.positionWS, input.positionCS);
                LitPassFragment(input, outColor
            #ifdef _WRITE_RENDERING_LAYERS
                    , outRenderingLayers
            #endif
                );
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ChantierShadowVertex
            #pragma fragment ChantierShadowFragment
            #pragma multi_compile_local_fragment _ _ALPHATEST_ON
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Assets/Art/Shaders/ChantierOmbre.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ChantierDepthVertex
            #pragma fragment ChantierDepthFragment
            #pragma multi_compile_local_fragment _ _ALPHATEST_ON
            #pragma multi_compile_instancing

            #include "Assets/Art/Shaders/ChantierProfondeur.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ChantierNormalsVertex
            #pragma fragment ChantierNormalsFragment
            #pragma multi_compile_local_fragment _ _ALPHATEST_ON
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #include "Assets/Art/Shaders/ChantierNormales.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
