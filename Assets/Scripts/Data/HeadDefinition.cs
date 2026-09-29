using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'une tête (GDD, section 6) : la tête d'un animal du pack, ses oreilles et son museau compris. Dans le
    /// pack, elle est déjà une pièce à part du modèle, qu'aucun triangle ne relie au corps : le menu
    /// DICIELA/Données/Créer les fiches manquantes l'en sépare, telle quelle (SAVOIR, 2.11). Elle se pose sur n'importe
    /// quel corps, tous les animaux ayant le même squelette.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Tête", fileName = "Tete")]
    public sealed class HeadDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Son maillage, séparé du modèle du pack par le menu (Assets/Art/Personnages), sur le squelette commun. " +
                 "La vue s'en sert ; la simulation, jamais.")]
        [SerializeField] private Mesh _mesh;

        [Tooltip("Sa couleur d'origine, celle que le pack lui donne : l'écran de création la remet quand on choisit cette " +
                 "tête.")]
        [SerializeField] private PaletteDefinition _palette;

        public string Id => _id;
        public string NameKey => _nameKey;
        public Mesh Mesh => _mesh;
        public PaletteDefinition Palette => _palette;
    }
}
