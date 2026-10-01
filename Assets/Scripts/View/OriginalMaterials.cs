using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Ce qu'on lit d'un matériau d'origine pour le recopier sur un chantier (décision 166) : s'il détoure (des feuilles,
    /// le bord d'un chaume), et à quel seuil ; les faces qu'il dessine ; sa texture de couleur, sous le nom de son shader
    /// (celui de Synty : _Albedo_Map ; ses feuillages : _Leaf_Texture ; un Lit d'URP : _BaseMap). La vraie matière
    /// (SiteMaterials) et le fantôme (SiteGhosts) le lisent de la même façon.
    /// </summary>
    internal static class OriginalMaterials
    {
        private static readonly string[] AlbedoNames = { "_Albedo_Map", "_Leaf_Texture", "_BaseMap" };

        /// <summary>
        /// Vrai si ce matériau détoure : là où sa texture est plus transparente que ce seuil, rien ne se dessine. Un seuil
        /// nul ne détoure rien, même si le détourage est coché.
        /// </summary>
        public static bool Clips(Material original, out float threshold)
        {
            threshold = original.HasProperty("_Alpha_Clip_Threshold") ? original.GetFloat("_Alpha_Clip_Threshold")
                : original.HasProperty("_Cutoff") ? original.GetFloat("_Cutoff") : 0f;
            return original.HasProperty("_AlphaClip") && original.GetFloat("_AlphaClip") > 0.5f && threshold > 0f;
        }

        /// <summary>Les faces qu'il dessine, comme le _Cull des shaders : 0 les deux, 1 l'envers, 2 l'endroit.</summary>
        public static float CullOf(Material original)
        {
            return original.HasProperty("_Cull") ? original.GetFloat("_Cull") : 2f;
        }

        /// <summary>Recopie sa texture de couleur sous ce nom, avec son échelle et son décalage ; rien s'il n'en a pas.</summary>
        public static void CopyAlbedo(Material original, Material to, string toName)
        {
            foreach (string name in AlbedoNames)
            {
                if (original.HasProperty(name))
                {
                    CopyTexture(original, name, to, toName);
                    return;
                }
            }
        }

        /// <summary>Recopie une texture de l'original sous un autre nom, avec son échelle et son décalage.</summary>
        public static void CopyTexture(Material from, string fromName, Material to, string toName)
        {
            if (!from.HasProperty(fromName))
                return;
            to.SetTexture(toName, from.GetTexture(fromName));
            to.SetTextureScale(toName, from.GetTextureScale(fromName));
            to.SetTextureOffset(toName, from.GetTextureOffset(fromName));
        }

        /// <summary>Recopie un nombre de l'original sous un autre nom, s'il l'a.</summary>
        public static void CopyNumber(Material from, string fromName, Material to, string toName)
        {
            if (from.HasProperty(fromName))
                to.SetFloat(toName, from.GetFloat(fromName));
        }
    }
}
