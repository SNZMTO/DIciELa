using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.View
{
    /// <summary>
    /// Un modèle vu en translucide, pour l'aperçu et le fantôme : les matériaux de ses maillages sont remplacés par un
    /// seul, sans ombre, et sa couleur se règle par un bloc de propriétés, sans créer de matériau à chaque changement.
    /// Ce qui n'est pas un maillage (la fumée des cheminées du pack, un effet) n'est pas dessiné : la construction n'est
    /// pas encore bâtie.
    /// </summary>
    public sealed class TranslucentModel
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly Renderer[] _renderers;
        private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();

        public TranslucentModel(GameObject root, Material translucent)
        {
            var meshes = new List<Renderer>();
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer))
                {
                    renderer.enabled = false;
                    continue;
                }
                var materials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                    materials[i] = translucent;
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                meshes.Add(renderer);
            }
            _renderers = meshes.ToArray();
        }

        public void SetColor(Color color)
        {
            _block.SetColor(BaseColorId, color);
            foreach (Renderer renderer in _renderers)
                renderer.SetPropertyBlock(_block);
        }
    }
}
