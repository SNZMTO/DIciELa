using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.View
{
    /// <summary>
    /// Le fantôme de chaque matériau d'origine, sur un chantier et dans l'aperçu de pose (décisions 166, 177 et 181) : le
    /// même pour tous, sauf pour ceux qui détourent (des feuilles et leurs tiges, le bord d'un chaume) ou qui dessinent
    /// leurs deux faces, qui ont leur copie du fantôme, avec leur texture, leur seuil et leurs faces : leur fantôme a leur
    /// forme, pas celle d'un carré plein. Tous portent la teinte du fantôme : un chantier n'a pas de bloc de propriétés.
    /// Faites à la première demande, partagées par tous les modèles, toutes détruites ensemble quand celui qui les tient
    /// les libère.
    /// </summary>
    internal sealed class SiteGhosts
    {
        // L'envers caché, l'endroit dessiné : les faces du fantôme de tous.
        private const float FrontFaces = (float)CullMode.Back;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly Material _ghost;
        private readonly Color _tint;
        private readonly float _blendedCutoff;
        private readonly Dictionary<Material, Material> _copies = new Dictionary<Material, Material>();
        // Le fantôme de tous, à la teinte du fantôme.
        private Material _common;

        /// <summary>
        /// Les fantômes faits sur celui-ci, de cette teinte ; un matériau transparent par mélange s'y détoure à ce seuil.
        /// </summary>
        public SiteGhosts(Material ghost, Color tint, float blendedCutoff)
        {
            _ghost = ghost;
            _tint = tint;
            _blendedCutoff = blendedCutoff;
        }

        /// <summary>Le nombre de copies vivantes faites pour un matériau d'origine.</summary>
        public int Count => _copies.Count;

        /// <summary>Le fantôme de ce matériau : sa copie s'il détoure ou dessine ses deux faces, sinon le fantôme de tous.</summary>
        public Material For(Material original)
        {
            if (_common == null)
            {
                _common = new Material(_ghost) { name = _ghost.name + " (teinté)" };
                _common.SetColor(BaseColorId, _tint);
            }
            if (original == null)
                return _common;
            bool clips = OriginalMaterials.Clips(original, _blendedCutoff, out float threshold);
            float cull = OriginalMaterials.CullOf(original);
            if (!clips && cull == FrontFaces)
                return _common;
            if (!_copies.TryGetValue(original, out Material copy))
            {
                copy = new Material(_common) { name = original.name + " (fantôme)" };
                if (clips)
                {
                    OriginalMaterials.CopyAlbedo(original, copy, "_BaseMap");
                    OriginalMaterials.CopyTrunk(original, copy);
                    copy.SetFloat("_Cutoff", threshold);
                    copy.EnableKeyword("_ALPHATEST_ON");
                }
                copy.SetFloat("_Cull", cull);
                _copies.Add(original, copy);
            }
            return copy;
        }

        public void Clear()
        {
            foreach (Material copy in _copies.Values)
                Object.Destroy(copy);
            _copies.Clear();
            if (_common != null)
                Object.Destroy(_common);
            _common = null;
        }
    }
}
