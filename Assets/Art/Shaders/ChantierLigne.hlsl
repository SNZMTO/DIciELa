#ifndef DICIELA_CHANTIER_LIGNE_INCLUDED
#define DICIELA_CHANTIER_LIGNE_INCLUDED

// La ligne d'un chantier qui monte (décisions 166 et 181) : sa hauteur, son ondulation lente, et la bande sous elle où
// la vraie matière s'effrite en points. Sa hauteur est la valeur propre de chaque rendu (la Renderer Shader User Value
// d'Unity, les bits d'un flottant, SiteLine) ; son ondulation et son effritement, des réglages de la pose, sont des
// variables globales. Aucune n'est une propriété de matériau, aucun bloc de propriétés ne les porte : le SRP Batcher
// regroupe les chantiers.
float _WaveAmplitude;
float _WaveLength;
float _WaveSeconds;
float _Softness;

// La hauteur de la ligne de ce rendu, en mètres.
float ChantierFillHeight()
{
    return asfloat(unity_RendererUserValue);
}

static const float ChantierBayerTable[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

// La hauteur de la ligne au-dessus de ce point : deux sinus lents qui se croisent, pour une vague qui ne se répète pas.
float ChantierLine(float3 positionWS)
{
    float t = _Time.y * 6.2831853 / max(_WaveSeconds, 0.01);
    float k = 6.2831853 / max(_WaveLength, 0.01);
    float wave = 0.6 * sin(k * (positionWS.x + 0.7 * positionWS.z) + t)
               + 0.4 * sin(k * 1.37 * (0.6 * positionWS.x - positionWS.z) - 1.3 * t);
    return ChantierFillHeight() + _WaveAmplitude * wave;
}

// Le seuil d'une trame de Bayer 4 × 4, de 0 à 1, à ce pixel.
float ChantierBayer(float2 pixel)
{
    uint2 p = uint2(pixel) % 4;
    return (ChantierBayerTable[p.y * 4 + p.x] + 0.5) / 16.0;
}

// La vraie matière : rien au-dessus de la ligne ; dans la bande sous elle, de plus en plus de trous en montant.
void ChantierClipBuilt(float3 positionWS, float4 positionCS)
{
    float h = positionWS.y - ChantierLine(positionWS);
    float crumble = saturate((h + _Softness) / max(_Softness, 0.001));
    clip(ChantierBayer(positionCS.xy) - crumble);
}

#endif
