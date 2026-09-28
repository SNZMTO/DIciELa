using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'un emplacement (GDD, section 6) : la queue, les oreilles, la tête… Un personnage porte au plus une
    /// pièce par emplacement ; l'écran de création en fait une ligne, dans l'ordre du catalogue. Ses pièces s'accrochent
    /// à un même os du squelette des personnages, qu'elles suivront quand ils seront animés.
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

        public string Id => _id;
        public string NameKey => _nameKey;
        public string Bone => _bone;
    }
}
