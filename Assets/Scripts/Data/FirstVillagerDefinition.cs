using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Un premier habitant (GDD, section 6) : il arrive dès qu'un lit est libre, fort dans une discipline, la
    /// sous-statistique de son métier, apporte l'idée de son lieu de travail, et reste en ville comme habitant ordinaire.
    /// Son allure est un humain du pack. Le déroulé du départ les fait arriver dans l'ordre de sa liste.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Premier habitant", fileName = "PremierHabitant")]
    public sealed class FirstVillagerDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom, dans la table des textes ; en attendant les noms du développeur, son métier (GDD, " +
                 "section 6).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("La sous-statistique de son métier, où il est fort en arrivant (GDD, section 6).")]
        [SerializeField] private SubStatisticDefinition _strength;

        [Tooltip("Son allure : un humain du pack, tel quel (GDD, section 16).")]
        [SerializeField] private CharacterDefinition _character;

        [Tooltip("Son lieu de travail, dont il apporte l'idée en arrivant : la ville sait aussitôt le bâtir (GDD, " +
                 "sections 6 et 13). Vide tant que ce lieu n'existe pas encore dans le jeu.")]
        [SerializeField] private StructureDefinition _workplace;

        public string Id => _id;
        public string NameKey => _nameKey;
        public SubStatisticDefinition Strength => _strength;
        public CharacterDefinition Character => _character;
        public StructureDefinition Workplace => _workplace;
    }
}
