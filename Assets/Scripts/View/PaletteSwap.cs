using Diciela.Data;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Change la couleur de rendus du pack (SAVOIR, 2.11) : chaque matériau d'une couleur est remplacé par celui de même
    /// sorte (personnage, objet) d'une autre, là seulement où il est ; une lentille de verre reste en verre. Les
    /// matériaux sont partagés : une copie par personnage empêcherait le rendu de les regrouper.
    /// </summary>
    public static class PaletteSwap
    {
        /// <summary>Remplace la couleur own par chosen sur ces rendus ; rien si c'est la même, ou sans couleur d'origine.</summary>
        public static void Apply(Renderer[] renderers, PaletteDefinition own, PaletteDefinition chosen)
        {
            if (own == null || chosen == null || own == chosen)
                return;
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material to = materials[i] == null ? null
                        : materials[i] == own.CharacterMaterial ? chosen.CharacterMaterial
                        : materials[i] == own.ObjectMaterial ? chosen.ObjectMaterial
                        : null;
                    if (to == null)
                        continue;
                    materials[i] = to;
                    changed = true;
                }
                if (changed)
                    renderer.sharedMaterials = materials;
            }
        }
    }
}
