using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'un emplacement (GDD, section 6) : la queue, les oreilles, le chapeau… Un personnage porte au plus une
    /// pièce par emplacement ; l'écran de création en fait une ligne, dans l'ordre du catalogue. Ses pièces se posent
    /// depuis un même repère (les pieds, ou un point mesuré sur la tête choisie) et s'accrochent à un même os du
    /// squelette des personnages, qu'elles suivront quand ils seront animés.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Emplacement", fileName = "Emplacement")]
    public sealed class AttachmentSlotDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Le nom de l'os du squelette des personnages auquel s'accrochent ses pièces (pelvis, head…).")]
        [SerializeField] private string _bone = "";

        [Tooltip("D'où se mesure la place de ses pièces : les pieds, ou un point de la tête choisie. Rempli par le menu.")]
        [SerializeField] private AttachmentFrame _frame;

        [Tooltip("Comment ses pièces viennent au contact du personnage : descendre sur le crâne, reculer contre la face, " +
                 "avancer contre le dos, ou rien. Rempli par le menu.")]
        [SerializeField] private ContactAxis _contact;

        [Tooltip("Vrai si ses pièces peuvent aller par paires (les oreilles) : chaque moitié suit l'oreille de son " +
                 "côté sur la tête choisie, et la paire se pose par sa base. Rempli par le menu.")]
        [SerializeField] private bool _pairs;

        [Tooltip("Vrai si un personnage ne peut pas s'en passer (la queue, les oreilles) : l'écran de création ne propose " +
                 "pas « aucun ». Rempli par le menu.")]
        [SerializeField] private bool _required;

        public string Id => _id;
        public string NameKey => _nameKey;
        public string Bone => _bone;
        public AttachmentFrame Frame => _frame;
        public ContactAxis Contact => _contact;
        public bool Pairs => _pairs;
        public bool Required => _required;

        /// <summary>
        /// Vrai si ses pièces touchent le corps (celles des pieds et du cou : ce qui les porte), faux si elles touchent la
        /// tête.
        /// </summary>
        public bool TouchesBody => _frame == AttachmentFrame.Body || _frame == AttachmentFrame.Neck;
    }
}
