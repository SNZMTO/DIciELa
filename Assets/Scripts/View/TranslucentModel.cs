using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.View
{
    /// <summary>
    /// Un modèle vu en fantôme (décisions 166 et 177), pour l'aperçu de la pose comme pour le fantôme d'un chantier :
    /// chaque matériau de ses maillages devient son fantôme (SiteGhosts : la forme des feuilles et de leurs tiges, du bord
    /// d'un chaume), sans ombre, et ses couleurs (sur un chantier, sa ligne) se règlent par un bloc de propriétés, sans
    /// créer de matériau à chaque changement. Sans ligne, le fantôme du chantier est un simple fantôme teinté. Ce qui
    /// n'est pas un maillage (la fumée des cheminées du pack, un effet) n'est pas dessiné : la construction n'est pas
    /// encore bâtie.
    /// </summary>
    internal sealed class TranslucentModel
    {
        private readonly Renderer[] _renderers;

        public TranslucentModel(GameObject root, SiteGhosts ghosts)
        {
            var meshes = new List<Renderer>();
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!KeepMesh(renderer))
                    continue;
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                    materials[i] = ghosts.For(materials[i]);
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                meshes.Add(renderer);
            }
            _renderers = meshes.ToArray();
        }

        /// <summary>Règle ses couleurs, et sur un chantier sa ligne, par ce bloc, sur tous ses maillages.</summary>
        public void Apply(MaterialPropertyBlock block)
        {
            foreach (Renderer renderer in _renderers)
                renderer.SetPropertyBlock(block);
        }

        /// <summary>
        /// Vrai si ce rendu dessine un maillage ; sinon (une fumée, un effet du pack), il est éteint : une construction
        /// pas encore bâtie ne fume pas.
        /// </summary>
        public static bool KeepMesh(Renderer renderer)
        {
            if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                return true;
            renderer.enabled = false;
            return false;
        }
    }
}
