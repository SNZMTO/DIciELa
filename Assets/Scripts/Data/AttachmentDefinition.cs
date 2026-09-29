using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'une pièce (GDD, section 6) : un morceau détaché d'une tête (oreilles, touffes, museau) ou un
    /// accessoire du pack (chapeau, cape…), qui prend l'une des couleurs du pack, comme la tête et le corps. Elle est
    /// modelée debout, dans les axes du personnage : la vue pose son pivot au point du repère de son emplacement, sur la
    /// tête choisie, plus son décalage, la met à sa taille, puis l'accroche à l'os de l'emplacement sans la bouger
    /// (SAVOIR, 2.11). Décalage et taille se règlent ici, à l'œil, sur la planche de l'emplacement.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Pièce", fileName = "Piece")]
    public sealed class AttachmentDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Son emplacement : un personnage n'y porte qu'une pièce, et elle s'accroche à son os.")]
        [SerializeField] private AttachmentSlotDefinition _slot;

        [Tooltip("Son modèle : une pièce du pack (Prefabs/Characters/Chr_Attach) ou un morceau détaché d'une tête " +
                 "(Assets/Art/Personnages/Pieces). La vue s'en sert ; la simulation, jamais.")]
        [SerializeField] private GameObject _model;

        [Tooltip("Sa couleur d'origine, celle de son modèle : l'écran de création la remet quand on la choisit.")]
        [SerializeField] private PaletteDefinition _palette;

        [Tooltip("Où se pose son pivot, en mètres depuis le point du repère de son emplacement sur la tête choisie (ou " +
                 "depuis les pieds pour le corps) : x vers la droite du personnage, y vers le haut, z vers l'avant. Sur " +
                 "l'axe de contact de l'emplacement, depuis la place où elle touche : négatif, elle s'enfonce.")]
        [SerializeField] private Vector3 _offset;

        [Tooltip("Sa taille selon ses trois axes, en fois celle du modèle (1, 1, 1 : telle quelle). Les pièces faites pour " +
                 "les humains sont agrandies pour une tête d'animal, d'un seul facteur, ou d'un facteur par axe pour celles " +
                 "qui enveloppent le crâne (cheveux, barbe). Zéro : pas encore réglée, le menu la remplit.")]
        [SerializeField] private Vector3 _scale;

        [Tooltip("Les emplacements qu'elle cache quand on la porte (un chapeau cache les oreilles) : leurs pièces restent " +
                 "choisies, mais ne se montrent pas.")]
        [SerializeField] private List<AttachmentSlotDefinition> _hides = new List<AttachmentSlotDefinition>();

        [Tooltip("Sa face qui touche le personnage (le dessous d'un chapeau, le dos de lunettes, le devant d'un sac), " +
                 "dans ses axes, à sa taille d'origine. Mesurée par le menu.")]
        [SerializeField] private SurfaceMap _shape = new SurfaceMap();

        public string Id => _id;
        public string NameKey => _nameKey;
        public AttachmentSlotDefinition Slot => _slot;
        public GameObject Model => _model;
        public PaletteDefinition Palette => _palette;
        public Vector3 Offset => _offset;
        public Vector3 Scale => _scale;

        /// <summary>Vrai si sa taille est réglée : positive selon ses trois axes.</summary>
        public bool HasScale => _scale.x > 0f && _scale.y > 0f && _scale.z > 0f;
        public IReadOnlyList<AttachmentSlotDefinition> Hides => _hides;
        public SurfaceMap Shape => _shape;
    }
}
