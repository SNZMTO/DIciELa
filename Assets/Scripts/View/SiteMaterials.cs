using System.Collections.Generic;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les copies des matériaux d'un modèle au shader du chantier qui monte (décision 166) : une par matériau d'origine,
    /// faite à la première demande et partagée par tous les chantiers. Ses textures et réglages viennent de l'original :
    /// ceux du pack (le shader de Synty : _Albedo_Map, _Normal_Map…) ou ceux d'un Lit d'URP (les boîtes de remplacement),
    /// recopiés un à un sous les noms du Lit. Toutes sont détruites ensemble quand la vue se cache.
    /// </summary>
    internal sealed class SiteMaterials
    {
        private readonly Material _template;
        private readonly Dictionary<Material, Material> _copies = new Dictionary<Material, Material>();

        public SiteMaterials(Material template)
        {
            _template = template;
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
            if (original.HasProperty("_Albedo_Map"))
            {
                Texture(original, "_Albedo_Map", copy, "_BaseMap");
                Texture(original, "_Normal_Map", copy, "_BumpMap");
                Number(original, "_Normal_Amount", copy, "_BumpScale");
                copy.SetColor("_BaseColor", Color.white);
                if (original.HasProperty("_Enable_Emission") && original.GetFloat("_Enable_Emission") > 0.5f)
                {
                    Texture(original, "_Emission_Map", copy, "_EmissionMap");
                    if (original.HasProperty("_Emission_Color"))
                        copy.SetColor("_EmissionColor", original.GetColor("_Emission_Color"));
                    copy.EnableKeyword("_EMISSION");
                }
            }
            else
            {
                Texture(original, "_BaseMap", copy, "_BaseMap");
                Texture(original, "_BumpMap", copy, "_BumpMap");
                Number(original, "_BumpScale", copy, "_BumpScale");
                if (original.HasProperty("_BaseColor"))
                    copy.SetColor("_BaseColor", original.GetColor("_BaseColor"));
            }
            Number(original, "_Smoothness", copy, "_Smoothness");
            Number(original, "_Metallic", copy, "_Metallic");
            if (copy.GetTexture("_BumpMap") != null)
                copy.EnableKeyword("_NORMALMAP");
            return copy;
        }

        private static void Texture(Material from, string fromName, Material to, string toName)
        {
            if (!from.HasProperty(fromName))
                return;
            to.SetTexture(toName, from.GetTexture(fromName));
            to.SetTextureScale(toName, from.GetTextureScale(fromName));
            to.SetTextureOffset(toName, from.GetTextureOffset(fromName));
        }

        private static void Number(Material from, string fromName, Material to, string toName)
        {
            if (from.HasProperty(fromName))
                to.SetFloat(toName, from.GetFloat(fromName));
        }
    }
}
