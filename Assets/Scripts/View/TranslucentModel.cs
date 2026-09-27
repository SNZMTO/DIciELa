using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.View
{
    /// <summary>
    /// Un modèle vu en translucide, pour l'aperçu et le fantôme : tous ses matériaux sont remplacés par un seul, sans
    /// ombre, et sa couleur se règle par un bloc de propriétés, sans créer de matériau à chaque changement.
    /// </summary>
    public sealed class TranslucentModel
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly Renderer[] _renderers;
        private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();

        public TranslucentModel(GameObject root, Material translucent)
        {
            _renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in _renderers)
            {
                var materials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                    materials[i] = translucent;
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        public void SetColor(Color color)
        {
            _block.SetColor(BaseColorId, color);
            foreach (Renderer renderer in _renderers)
                renderer.SetPropertyBlock(_block);
        }
    }
}
