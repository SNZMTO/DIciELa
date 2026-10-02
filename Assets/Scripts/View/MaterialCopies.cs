using System.Collections.Generic;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Des copies de matériaux faites à l'exécution, une par matériau d'origine, gardées pour resservir et toutes
    /// détruites ensemble : celles du chantier (SiteMaterials) comme celles de son fantôme (SiteGhosts).
    /// </summary>
    internal sealed class MaterialCopies
    {
        private readonly Dictionary<Material, Material> _copies = new Dictionary<Material, Material>();

        /// <summary>La copie déjà faite pour cet original ; faux s'il n'en a pas encore.</summary>
        public bool TryGet(Material original, out Material copy)
        {
            return _copies.TryGetValue(original, out copy);
        }

        /// <summary>Garde cette copie de cet original, et la rend.</summary>
        public Material Keep(Material original, Material copy)
        {
            _copies.Add(original, copy);
            return copy;
        }

        /// <summary>Détruit toutes les copies.</summary>
        public void Clear()
        {
            foreach (Material copy in _copies.Values)
                Object.Destroy(copy);
            _copies.Clear();
        }
    }
}
