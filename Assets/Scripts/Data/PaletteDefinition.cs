using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'une couleur (GDD, section 6) : l'une des douze textures de couleurs du pack, rangées de la même façon,
    /// par ses deux matériaux, tels quels : celui des personnages (têtes, corps, queues) et celui des objets (chapeaux,
    /// capes, lunettes…), qui ne diffèrent que par leur shader. N'importe quelle partie prend n'importe quelle couleur :
    /// on remplace chacun de ses matériaux par celui de même sorte.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Couleur", fileName = "Couleur")]
    public sealed class PaletteDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Le matériau des personnages du pack de cette couleur (Materials/Alts/Animal_Alts). La vue s'en sert ; " +
                 "la simulation, jamais.")]
        [SerializeField] private Material _characterMaterial;

        [Tooltip("Le matériau des objets du pack de cette couleur (Materials/Alts), celui des pièces à accrocher.")]
        [SerializeField] private Material _objectMaterial;

        public string Id => _id;
        public string NameKey => _nameKey;
        public Material CharacterMaterial => _characterMaterial;
        public Material ObjectMaterial => _objectMaterial;
    }
}
