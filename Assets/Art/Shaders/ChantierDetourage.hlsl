#ifndef DICIELA_CHANTIER_DETOURAGE_INCLUDED
#define DICIELA_CHANTIER_DETOURAGE_INCLUDED

// La couleur et le détourage d'un matériau du pack sur un chantier qui monte (décisions 166 et 177). Un feuillage du
// pack (le shader Foliage de Synty) prend, sommet par sommet, la texture des feuilles là où le bleu de sa couleur
// dépasse 0,5, et celle des tiges ailleurs, lue en UV brut : la copie la reçoit (_TrunkMap), et la branche _TRUNKMAP la
// lit (une branche du shader, pas une variante de plus). Là où la texture lue est plus transparente que le seuil, rien
// ne se dessine, ni ombre ni profondeur : seulement si le matériau d'origine détoure (_ALPHATEST_ON, posé par
// SiteMaterials et SiteGhosts). Celui qui l'inclut a déclaré _BaseMap, son échantillonneur, _BaseMap_ST et _Cutoff, et
// la branche _TRUNKMAP (#pragma dynamic_branch_local).
TEXTURE2D(_TrunkMap);
SAMPLER(sampler_TrunkMap);

// Vrai là où le sommet dit une tige d'un feuillage du pack.
bool ChantierIsTrunk(half leaf)
{
    return _TRUNKMAP && leaf <= 0.5h;
}

// La couleur et l'alpha de la matière en ce point : la texture des tiges sur une tige, sinon celle de la matière. La
// branche ne dépend que du matériau : les deux lectures restent hors d'un flot qui varie d'un pixel à l'autre.
half4 ChantierAlbedo(float2 uv, half leaf)
{
    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    if (_TRUNKMAP)
    {
        half4 trunk = SAMPLE_TEXTURE2D(_TrunkMap, sampler_TrunkMap, (uv - _BaseMap_ST.zw) / _BaseMap_ST.xy);
        albedo = leaf > 0.5h ? albedo : trunk;
    }
    return albedo;
}

void ChantierClipAlpha(float2 uv, half leaf)
{
#if defined(_ALPHATEST_ON)
    clip(ChantierAlbedo(uv, leaf).a - _Cutoff);
#endif
}

#endif
