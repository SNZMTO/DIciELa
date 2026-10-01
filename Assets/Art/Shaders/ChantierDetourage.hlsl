#ifndef DICIELA_CHANTIER_DETOURAGE_INCLUDED
#define DICIELA_CHANTIER_DETOURAGE_INCLUDED

// Le détourage d'un matériau du pack sur un chantier qui monte (décision 166) : des feuilles, le bord d'un chaume. Là où
// sa texture est plus transparente que son seuil, rien ne se dessine, ni ombre ni profondeur. Seulement si le matériau
// d'origine détoure (_ALPHATEST_ON, posé par SiteMaterials et SiteGhosts). Celui qui l'inclut a déclaré _BaseMap, son
// échantillonneur et _Cutoff.
void ChantierClipAlpha(float2 uv)
{
#if defined(_ALPHATEST_ON)
    clip(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).a - _Cutoff);
#endif
}

#endif
