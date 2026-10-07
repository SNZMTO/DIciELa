using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Le catalogue : la fiche racine, qui mène à toutes les autres (ARCHITECTURE, section 9) ; le jeu ne charge que
    /// lui. Les familles de constructions, dans l'ordre des onglets de la barre ; la sorte de la Chimère ; les sortes
    /// d'arbres et de rochers de la carte ; ce que l'écran de création propose, dans son ordre : les têtes, les corps, les
    /// couleurs, les emplacements et leurs pièces. La première tête et le premier corps, dans leurs couleurs d'origine, et
    /// les pièces que ce corps porte dans le pack, de sa couleur, font l'apparence de départ.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Catalogue", fileName = "Catalogue")]
    public sealed class Catalog : ScriptableObject
    {
        [Tooltip("Les familles de constructions, dans l'ordre des onglets de la barre.")]
        [SerializeField] private List<StructureFamilyDefinition> _families = new List<StructureFamilyDefinition>();

        [Tooltip("La sorte de la Chimère, que le joueur crée au lancement d'une partie (GDD, section 6).")]
        [SerializeField] private VillagerKindDefinition _chimeraKind;

        [Tooltip("La sorte des habitants ordinaires : les humains du pack (GDD, sections 5 et 16).")]
        [SerializeField] private VillagerKindDefinition _villagerKind;

        [Tooltip("Le déroulé du départ : ce que la Chimère sait bâtir, le stock de départ, les premiers habitants (GDD, " +
                 "section 6).")]
        [SerializeField] private OpeningDefinition _opening;

        [Tooltip("La charrette de l'étal, celle de chaque entrepôt (GDD, section 11 ; décision 194).")]
        [SerializeField] private CartDefinition _cart;

        [Tooltip("La Place, la zone que le joueur trace, et ce que permet chacun de ses niveaux (GDD, section 11 ; " +
                 "décision 204).")]
        [SerializeField] private PlazaDefinition _plaza;

        [Tooltip("Les sortes de sol, dans l'ordre de l'éventail des sols de la roue des outils Chemins et Place " +
                 "(GDD, section 9 ; décisions 210 et 221).")]
        [SerializeField] private List<FloorDefinition> _floors = new List<FloorDefinition>();

        [Tooltip("Les sortes d'arbres et de rochers que le développeur peint sur la carte (GDD, section 2).")]
        [SerializeField] private List<NatureDefinition> _nature = new List<NatureDefinition>();

        [Tooltip("Les marchandises, dans l'ordre des fanions du stock (GDD, section 16).")]
        [SerializeField] private List<GoodDefinition> _goods = new List<GoodDefinition>();

        [Tooltip("Les statistiques et leurs sous-statistiques (GDD, section 5).")]
        [SerializeField] private List<StatisticDefinition> _statistics = new List<StatisticDefinition>();

        [Tooltip("Les têtes que propose l'écran de création, dans son ordre ; la première est celle de départ.")]
        [SerializeField] private List<HeadDefinition> _heads = new List<HeadDefinition>();

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
        public VillagerKindDefinition VillagerKind => _villagerKind;
        public OpeningDefinition Opening => _opening;
        public CartDefinition Cart => _cart;
        public PlazaDefinition Plaza => _plaza;
        public IReadOnlyList<FloorDefinition> Floors => _floors;
        public IReadOnlyList<NatureDefinition> Nature => _nature;
        public IReadOnlyList<GoodDefinition> Goods => _goods;
        public IReadOnlyList<StatisticDefinition> Statistics => _statistics;
        public IReadOnlyList<HeadDefinition> Heads => _heads;
        public IReadOnlyList<BodyDefinition> Bodies => _bodies;
        public IReadOnlyList<PaletteDefinition> Palettes => _palettes;
        public IReadOnlyList<AttachmentSlotDefinition> AttachmentSlots => _attachmentSlots;
        public IReadOnlyList<AttachmentDefinition> Attachments => _attachments;

        /// <summary>
        /// Toutes les constructions du catalogue : celles des familles, dans leur ordre. Une liste neuve à chaque appel :
        /// pour les outils de l'éditeur et les tests de contenu, jamais à chaque pas.
        /// </summary>
        public List<StructureDefinition> AllStructures()
        {
            var structures = new List<StructureDefinition>();
            foreach (StructureFamilyDefinition family in _families)
                structures.AddRange(family.Structures);
            return structures;
        }
    }
}
