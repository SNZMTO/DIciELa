using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'une pièce (GDD, section 6) : un morceau d'un autre animal (queue, oreilles, museau) ou un accessoire
    /// (chapeau, cape…), pris dans le pack tel quel, qui prend l'une des couleurs du pack, comme la tête et le corps. Le
    /// pack la modèle debout, dans les axes du personnage, son pivot à son point d'accroche : la vue pose ce pivot sur ce
    /// point du personnage, droite, puis l'accroche à l'os de son emplacement sans la bouger (SAVOIR, 2.11).
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

        [Tooltip("La pièce du pack, telle quelle (Prefabs/Characters/Chr_Attach). La vue s'en sert ; la simulation, " +
                 "jamais.")]
        [SerializeField] private GameObject _model;

        [Tooltip("Sa couleur d'origine, celle de son modèle dans le pack : l'écran de création la remet quand on la " +
                 "choisit.")]
        [SerializeField] private PaletteDefinition _palette;

        [Tooltip("Où se pose son pivot sur le personnage debout, en mètres depuis ses pieds : x vers sa droite, y vers le " +
                 "haut, z vers l'avant. Tous les animaux du pack ont le même squelette : un seul point pour tous. Se règle " +
                 "en regardant la planche de la Chimère (DICIELA/Album).")]
        [SerializeField] private Vector3 _anchor;

        public string Id => _id;
        public string NameKey => _nameKey;
        public AttachmentSlotDefinition Slot => _slot;
        public GameObject Model => _model;
        public PaletteDefinition Palette => _palette;
        public Vector3 Anchor => _anchor;
    }
}
