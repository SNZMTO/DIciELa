using System.Collections.Generic;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les copies des matériaux d'un modèle au shader du chantier qui monte (décisions 166 et 177) : une par matériau
    /// d'origine, faite à la première demande et partagée par tous les chantiers. Ses textures et réglages viennent de
    /// l'original : ceux du pack (le shader de Synty : _Albedo_Map, _Normal_Map… ; ses feuillages : _Leaf_Texture, et
    /// _Trunk_Texture pour les tiges…) ou ceux d'un Lit d'URP (les boîtes de remplacement), recopiés un à un sous les
    /// noms du Lit ; elle détoure comme lui et dessine les mêmes faces (OriginalMaterials). Un shader qu'on ne connaît
    /// pas est dit dans la console : sa copie monterait blanche. Toutes sont détruites ensemble quand la vue se cache.
    /// </summary>
    internal sealed class SiteMaterials
    {
        private readonly Material _template;
        private readonly float _blendedCutoff;
        private readonly Dictionary<Material, Material> _copies = new Dictionary<Material, Material>();

        /// <summary>Les copies faites sur ce modèle ; un matériau transparent par mélange s'y détoure à ce seuil.</summary>
        public SiteMaterials(Material template, float blendedCutoff)
        {
            _template = template;
            _blendedCutoff = blendedCutoff;
        }

        /// <summary>Le nombre de copies vivantes.</summary>
        public int Count => _copies.Count;

        /// <summary>La copie de ce matériau au shader du chantier ; le modèle de copie s'il n'y en a pas.</summary>
        public Material For(Material original)
        {
            if (original == null)
                return _template;
            if (!_copies.TryGetValue(original, out Material copy))
            {
                copy = Copy(original);
                _copies.Add(original, copy);
            }
            return copy;
        }

        public void Clear()
        {
            foreach (Material copy in _copies.Values)
                Object.Destroy(copy);
            _copies.Clear();
        }

        private Material Copy(Material original)
        {
            var copy = new Material(_template) { name = original.name + " (chantier)" };
            OriginalMaterials.CopyNumber(original, "_Smoothness", copy, "_Smoothness");
            OriginalMaterials.CopyNumber(original, "_Metallic", copy, "_Metallic");
            if (original.HasProperty("_Albedo_Map"))
            {
                OriginalMaterials.CopyTexture(original, "_Albedo_Map", copy, "_BaseMap");
                OriginalMaterials.CopyTexture(original, "_Normal_Map", copy, "_BumpMap");
                OriginalMaterials.CopyNumber(original, "_Normal_Amount", copy, "_BumpScale");
                copy.SetColor("_BaseColor", Color.white);
                if (OriginalMaterials.IsOn(original, "_Enable_Emission"))
                {
                    OriginalMaterials.CopyTexture(original, "_Emission_Map", copy, "_EmissionMap");
                    if (original.HasProperty("_Emission_Color"))
                        copy.SetColor("_EmissionColor", original.GetColor("_Emission_Color"));
                    copy.EnableKeyword("_EMISSION");
                }
            }
            else if (original.HasProperty("_Leaf_Texture"))
            {
                // Les feuillages du pack : la texture des feuilles fait la matière, et leur détourage ; celle des tiges,
                // là où la couleur du sommet le dit.
                OriginalMaterials.CopyTexture(original, "_Leaf_Texture", copy, "_BaseMap");
                OriginalMaterials.CopyTrunk(original, copy);
                if (OriginalMaterials.IsOn(original, "_Enable_Leaf_Normal"))
                    OriginalMaterials.CopyTexture(original, "_Leaf_Normal", copy, "_BumpMap");
                OriginalMaterials.CopyNumber(original, "_Leaf_Normal_Strength", copy, "_BumpScale");
                OriginalMaterials.CopyNumber(original, "_Leaf_Smoothness", copy, "_Smoothness");
                OriginalMaterials.CopyNumber(original, "_Leaf_Metallic", copy, "_Metallic");
                copy.SetColor("_BaseColor", Color.white);
            }
            else if (original.HasProperty("_BaseMap") || original.HasProperty("_BaseColor"))
            {
                OriginalMaterials.CopyTexture(original, "_BaseMap", copy, "_BaseMap");
                OriginalMaterials.CopyTexture(original, "_BumpMap", copy, "_BumpMap");
                OriginalMaterials.CopyNumber(original, "_BumpScale", copy, "_BumpScale");
                if (original.HasProperty("_BaseColor"))
                    copy.SetColor("_BaseColor", original.GetColor("_BaseColor"));
            }
            else
            {
                Debug.LogWarning("DICIELA : le matériau « " + original.name + " » (shader « " + original.shader.name +
                                 " ») n'est pas reconnu sur un chantier : sa copie monte blanche. Apprendre ses " +
                                 "propriétés à SiteMaterials.", original);
            }
            if (copy.GetTexture("_BumpMap") != null)
                copy.EnableKeyword("_NORMALMAP");
            if (OriginalMaterials.Clips(original, _blendedCutoff, out float threshold))
            {
                copy.SetFloat("_Cutoff", threshold);
                copy.EnableKeyword("_ALPHATEST_ON");
            }
            copy.SetFloat("_Cull", OriginalMaterials.CullOf(original));
            return copy;
        }
    }
}
