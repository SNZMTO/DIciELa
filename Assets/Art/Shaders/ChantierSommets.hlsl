// Le sommet commun des passes du chantier qui ne demandent que la ligne et le détourage : la profondeur, les normales
// (Diciela/Chantier) et le fantôme (Diciela/ChantierFantome). Il porte la position dans le monde, la normale, l'UV de la
// texture de la matière et le bleu de la couleur du sommet (feuille ou tige d'un feuillage du pack, ChantierDetourage).
// Celui qui l'inclut a déclaré _BaseMap_ST. L'ombre a son propre sommet, qu'elle décale vers la lumière.
#ifndef DICIELA_CHANTIER_SOMMETS
#define DICIELA_CHANTIER_SOMMETS

struct ChantierAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct ChantierVaryings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    half3 normalWS : TEXCOORD1;
    float2 uv : TEXCOORD2;
    half leaf : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

ChantierVaryings ChantierVertex(ChantierAttributes input)
{
    ChantierVaryings output = (ChantierVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.normalWS = half3(TransformObjectToWorldNormal(input.normalOS));
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.leaf = input.color.b;
    return output;
}

#endif
