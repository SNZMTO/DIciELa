using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Ce qu'on lit d'un matériau d'origine pour le recopier sur un chantier ou dans l'aperçu (décisions 166 et 177) : s'il
    /// détoure (des feuilles), et à quel seuil, ou s'il est transparent par mélange (le bord d'un chaume), qu'on détoure
    /// au seuil des réglages ; les faces qu'il dessine ; sa texture de couleur, sous le nom de son shader (celui de Synty :
    /// _Albedo_Map ; ses feuillages : _Leaf_Texture, et _Trunk_Texture pour leurs tiges ; un Lit d'URP : _BaseMap). La
    /// vraie matière (SiteMaterials) et le fantôme (SiteGhosts) le lisent de la même façon.
    /// </summary>
    internal static class OriginalMaterials
    {
        private static readonly string[] AlbedoNames = { "_Albedo_Map", "_Leaf_Texture", "_BaseMap" };

        /// <summary>
        /// Vrai si ce matériau détoure : là où sa texture est plus transparente que ce seuil, rien ne se dessine. Un seuil
        /// nul ne détoure rien, même si le détourage est coché. Un matériau transparent par mélange, dont la texture est
        /// pleine ou vide (le bord d'un chaume), se détoure au seuil donné : les copies sont opaques.
        /// </summary>
        public static bool Clips(Material original, float blendedCutoff, out float threshold)
        {
            threshold = original.HasProperty("_Alpha_Clip_Threshold") ? original.GetFloat("_Alpha_Clip_Threshold")
                : original.HasProperty("_Cutoff") ? original.GetFloat("_Cutoff") : 0f;
            if (IsOn(original, "_AlphaClip") && threshold > 0f)
                return true;
            threshold = blendedCutoff;
            return IsOn(original, "_Surface") && threshold > 0f;
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

        /// <summary>
        /// Un feuillage du pack qui a des tiges : la copie reçoit leur texture et la branche qui la lit là où la couleur
        /// du sommet dit une tige (ChantierDetourage) ; sinon, rien.
        /// </summary>
        public static void CopyTrunk(Material original, Material to)
        {
            if (!original.HasProperty("_Trunk_Texture") || original.GetTexture("_Trunk_Texture") == null)
                return;
            to.SetTexture("_TrunkMap", original.GetTexture("_Trunk_Texture"));
            to.EnableKeyword("_TRUNKMAP");
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

        /// <summary>Vrai si ce réglage du matériau, une case à cocher, est coché.</summary>
        public static bool IsOn(Material material, string toggle)
        {
            return material.HasProperty(toggle) && material.GetFloat(toggle) > 0.5f;
        }
    }
}
