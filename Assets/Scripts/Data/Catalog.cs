using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Le catalogue : la fiche racine, qui mène à toutes les autres (ARCHITECTURE, section 9) ; le jeu ne charge que
    /// lui. Pour l'instant, les familles de constructions, dans l'ordre des onglets de la barre, et la sorte de la
    /// Chimère.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Catalogue", fileName = "Catalogue")]
    public sealed class Catalog : ScriptableObject
    {
        [Tooltip("Les familles de constructions, dans l'ordre des onglets de la barre.")]
        [SerializeField] private List<StructureFamilyDefinition> _families = new List<StructureFamilyDefinition>();

        [Tooltip("La sorte de la Chimère, que le joueur crée au lancement d'une partie (GDD, section 6).")]
        [SerializeField] private VillagerKindDefinition _chimeraKind;

        public IReadOnlyList<StructureFamilyDefinition> Families => _families;
        public VillagerKindDefinition ChimeraKind => _chimeraKind;
    }
}
