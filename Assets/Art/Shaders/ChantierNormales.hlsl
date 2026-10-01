// La passe des normales du chantier qui monte (Diciela/Chantier, décision 166), coupée à la même ligne et détourée :
// l'occlusion ambiante ne voit ni la moitié pas bâtie, ni ce que le matériau détoure.
#ifndef DICIELA_CHANTIER_NORMALES
#define DICIELA_CHANTIER_NORMALES

#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets/Art/Shaders/ChantierLigne.hlsl"
#include "Assets/Art/Shaders/ChantierDetourage.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    half3 normalWS : TEXCOORD1;
    float2 uv : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings ChantierNormalsVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.normalWS = half3(TransformObjectToWorldNormal(input.normalOS));
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    return output;
}

void ChantierNormalsFragment(
    Varyings input
    , out half4 outNormalWS : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out uint outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    ChantierClipBuilt(input.positionWS, input.positionCS);
    ChantierClipAlpha(input.uv);
    float3 normalWS = normalize(float3(input.normalWS));
#if defined(_GBUFFER_NORMALS_OCT)
    float2 octNormalWS = PackNormalOctQuadEncode(normalWS);
    float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
    outNormalWS = half4(PackFloat2To888(remappedOctNormalWS), 0.0);
#else
    outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
#endif
#ifdef _WRITE_RENDERING_LAYERS
    outRenderingLayers = EncodeMeshRenderingLayer();
#endif
}

#endif
