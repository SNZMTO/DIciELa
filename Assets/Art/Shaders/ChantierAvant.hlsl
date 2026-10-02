// La passe avant du chantier qui monte (Diciela/Chantier, décisions 166 et 177) : celle du Lit d'URP (LitForwardPass),
// son sommet et son éclairage gardés tels quels, avec deux ajouts. La ligne du chantier coupe ce qui est au-dessus ; un
// feuillage du pack prend sa couleur et son alpha à la texture des tiges là où le bleu du sommet le dit
// (ChantierDetourage), avant que la matière ne soit détourée. Ce bleu va du sommet au fragment dans l'interpolateur
// viewDirTS d'URP, libre sans parallaxe (le chantier n'en a pas) : Chantier.shader le demande avant d'inclure
// LitForwardPass. Celui qui l'inclut a inclus LitInput, LitForwardPass, ChantierLigne et ChantierDetourage.
#ifndef DICIELA_CHANTIER_AVANT
#define DICIELA_CHANTIER_AVANT

// Les entrées du sommet d'URP (Attributes de LitForwardPass), et la couleur du sommet.
struct ChantierForwardAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 texcoord : TEXCOORD0;
    float2 staticLightmapUV : TEXCOORD1;
    float2 dynamicLightmapUV : TEXCOORD2;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ChantierForwardVertex(ChantierForwardAttributes input)
{
    Attributes lit = (Attributes)0;
    lit.positionOS = input.positionOS;
    lit.normalOS = input.normalOS;
    lit.tangentOS = input.tangentOS;
    lit.texcoord = input.texcoord;
    lit.staticLightmapUV = input.staticLightmapUV;
    lit.dynamicLightmapUV = input.dynamicLightmapUV;
    UNITY_TRANSFER_INSTANCE_ID(input, lit);
    Varyings output = LitPassVertex(lit);
    output.viewDirTS = half3(input.color.b, 0.0h, 0.0h);
    return output;
}

// InitializeStandardLitSurfaceData d'URP (LitInput), sans les couches que le chantier n'a pas (vernis, détail) : la
// couleur et l'alpha viennent de la texture des tiges sur une tige, qui n'a pas de carte de normales.
void ChantierSurfaceData(float2 uv, half leaf, out SurfaceData outSurfaceData)
{
    half4 albedoAlpha = ChantierAlbedo(uv, leaf);
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0h, 0.0h, 0.0h);
    outSurfaceData.smoothness = specGloss.a;
    half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.normalTS = ChantierIsTrunk(leaf) ? half3(0.0h, 0.0h, 1.0h) : normalTS;
    outSurfaceData.occlusion = SampleOcclusion(uv);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
    outSurfaceData.clearCoatMask = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
}

// LitPassFragment d'URP, sans la parallaxe, la matière tirée de ChantierSurfaceData, et coupée à la ligne d'abord.
void ChantierForwardFragment(
    Varyings input
    , out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out uint outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    ChantierClipBuilt(input.positionWS, input.positionCS);

    SurfaceData surfaceData;
    ChantierSurfaceData(input.uv, input.viewDirTS.x, surfaceData);

#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
#endif

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, UNDO_TRANSFORM_TEX(input.uv, _BaseMap));

#if defined(_DBUFFER)
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

    InitializeBakedGIData(input, inputData);

    half4 color = UniversalFragmentPBR(inputData, surfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent());

    outColor = color;
#ifdef _WRITE_RENDERING_LAYERS
    outRenderingLayers = EncodeMeshRenderingLayer();
#endif
}

#endif
