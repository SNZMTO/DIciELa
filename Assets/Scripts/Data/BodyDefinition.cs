using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'un corps (GDD, section 6) : un personnage du pack, tel quel, d'un seul tenant (GDD, section 16), et
    /// sa couleur d'origine, celle que le pack lui donne. Chaque prefab d'animal du pack porte seize corps sur un même
    /// squelette, un seul allumé : le corps est celui qui est allumé ; les autres ne se montrent jamais.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Corps", fileName = "Corps")]
    public sealed class BodyDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Le personnage du pack, tel quel, l'origine à ses pieds et regardant vers +z. La vue s'en sert ; la " +
                 "simulation, jamais.")]
        [SerializeField] private GameObject _model;

        [Tooltip("Sa couleur d'origine, celle que le pack lui donne : l'écran de création la remet quand on choisit ce " +
                 "corps, et une autre couleur prend sa place sur le personnage.")]
        [SerializeField] private PaletteDefinition _palette;

        public string Id => _id;
        public string NameKey => _nameKey;
        public GameObject Model => _model;
        public PaletteDefinition Palette => _palette;
    }
}
