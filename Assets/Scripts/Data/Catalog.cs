using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Le catalogue : la fiche racine, qui mène à toutes les autres (ARCHITECTURE, section 9) ; le jeu ne charge que
    /// lui. Les familles de constructions, dans l'ordre des onglets de la barre ; la sorte de la Chimère ; ce que
    /// l'écran de création propose, dans son ordre : les corps (le premier, avec sa couleur d'origine et sans pièce, est
    /// l'apparence de départ), les couleurs, les emplacements et leurs pièces.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Catalogue", fileName = "Catalogue")]
    public sealed class Catalog : ScriptableObject
    {
        [Tooltip("Les familles de constructions, dans l'ordre des onglets de la barre.")]
        [SerializeField] private List<StructureFamilyDefinition> _families = new List<StructureFamilyDefinition>();

        [Tooltip("La sorte de la Chimère, que le joueur crée au lancement d'une partie (GDD, section 6).")]
        [SerializeField] private VillagerKindDefinition _chimeraKind;

        [Tooltip("Les corps que propose l'écran de création, dans son ordre ; le premier est celui de départ.")]
        [SerializeField] private List<BodyDefinition> _bodies = new List<BodyDefinition>();

        [Tooltip("Les couleurs que propose l'écran de création, dans son ordre.")]
        [SerializeField] private List<PaletteDefinition> _palettes = new List<PaletteDefinition>();

        [Tooltip("Les emplacements, dans l'ordre des lignes de l'écran de création.")]
        [SerializeField] private List<AttachmentSlotDefinition> _attachmentSlots = new List<AttachmentSlotDefinition>();

        [Tooltip("Les pièces que propose l'écran de création ; chacune sur la ligne de son emplacement, dans cet ordre.")]
        [SerializeField] private List<AttachmentDefinition> _attachments = new List<AttachmentDefinition>();

        public IReadOnlyList<StructureFamilyDefinition> Families => _families;
        public VillagerKindDefinition ChimeraKind => _chimeraKind;
        public IReadOnlyList<BodyDefinition> Bodies => _bodies;
        public IReadOnlyList<PaletteDefinition> Palettes => _palettes;
        public IReadOnlyList<AttachmentSlotDefinition> AttachmentSlots => _attachmentSlots;
        public IReadOnlyList<AttachmentDefinition> Attachments => _attachments;
    }
}
