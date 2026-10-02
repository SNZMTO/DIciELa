// La passe d'ombre du chantier qui monte (Diciela/Chantier, décision 166) : la moitié pas bâtie ne fait pas d'ombre,
// ni ce que le matériau détoure. Celle d'URP n'a pas la position dans le monde : la ligne en a besoin.
#ifndef DICIELA_CHANTIER_OMBRE
#define DICIELA_CHANTIER_OMBRE

#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Assets/Art/Shaders/ChantierLigne.hlsl"
#include "Assets/Art/Shaders/ChantierDetourage.hlsl"

float3 _LightDirection;
float3 _LightPosition;

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    float2 uv : TEXCOORD1;
    half leaf : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ChantierShadowVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif
    output.positionCS = ApplyShadowClamping(
        TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS)));
    output.positionWS = positionWS;
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.leaf = input.color.b;
    return output;
}

half4 ChantierShadowFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    ChantierClipBuilt(input.positionWS, input.positionCS);
    ChantierClipAlpha(input.uv, input.leaf);
    return 0;
}

#endif
