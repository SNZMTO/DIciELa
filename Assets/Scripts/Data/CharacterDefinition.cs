using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Un personnage du pack, montré tel que le pack le fait (GDD, section 16 : les habitants ordinaires sont les humains
    /// du pack) : son modèle, sans découpe ni couleur changée. La simulation le garde avec l'habitant sans le lire ; la
    /// vue l'habille (Dresser).
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Personnage du pack", fileName = "Personnage")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("Le personnage du pack, tel quel (Assets/Synty/PolygonFantasyVillage/Prefabs/Characters).")]
        [SerializeField] private GameObject _model;

        public string Id => _id;
        public GameObject Model => _model;
    }
}
