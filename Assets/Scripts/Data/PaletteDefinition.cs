using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'une couleur de corps (GDD, section 6) : un matériau d'animal du pack, tel quel. Tous lisent des
    /// textures de couleurs rangées de la même façon : chacun va sur n'importe quel corps, pelage et habits compris.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Couleur", fileName = "Couleur")]
    public sealed class PaletteDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Le matériau du pack, tel quel. La vue le pose sur le corps ; la simulation ne s'en sert pas.")]
        [SerializeField] private Material _material;

        public string Id => _id;
        public string NameKey => _nameKey;
        public Material Material => _material;
    }
}
