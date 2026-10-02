// La passe de profondeur du chantier qui monte (Diciela/Chantier, décision 166), coupée à la même ligne et détourée.
#ifndef DICIELA_CHANTIER_PROFONDEUR
#define DICIELA_CHANTIER_PROFONDEUR

#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets/Art/Shaders/ChantierLigne.hlsl"
#include "Assets/Art/Shaders/ChantierDetourage.hlsl"
#include "Assets/Art/Shaders/ChantierSommets.hlsl"

half ChantierDepthFragment(ChantierVaryings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    ChantierClipBuilt(input.positionWS, input.positionCS);
    ChantierClipAlpha(input.uv, input.leaf);
    return input.positionCS.z;
}

#endif
