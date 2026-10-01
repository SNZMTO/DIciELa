using System.Collections.Generic;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Le fantôme d'un chantier, pour chaque matériau d'origine (décision 166) : le même pour tous, sauf pour ceux qui
    /// détourent (des feuilles, le bord d'un chaume), qui ont leur copie du fantôme, avec leur texture, leur seuil et
    /// leurs faces : leur fantôme a leur forme, pas celle d'un carré plein. Faites à la première demande, partagées par
    /// tous les chantiers, toutes détruites ensemble quand la vue se cache.
    /// </summary>
    internal sealed class SiteGhosts
    {
        private readonly Material _ghost;
        private readonly Dictionary<Material, Material> _copies = new Dictionary<Material, Material>();

        public SiteGhosts(Material ghost)
        {
            _ghost = ghost;
        }

        /// <summary>Le nombre de copies vivantes.</summary>
        public int Count => _copies.Count;

        /// <summary>Le fantôme de ce matériau : sa copie s'il détoure, sinon le fantôme de tous.</summary>
        public Material For(Material original)
        {
            if (original == null || !OriginalMaterials.Clips(original, out float threshold))
                return _ghost;
            if (!_copies.TryGetValue(original, out Material copy))
            {
                copy = new Material(_ghost) { name = original.name + " (fantôme)" };
                OriginalMaterials.CopyAlbedo(original, copy, "_BaseMap");
                copy.SetFloat("_Cutoff", threshold);
                copy.SetFloat("_Cull", OriginalMaterials.CullOf(original));
                copy.EnableKeyword("_ALPHATEST_ON");
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
    }
}
