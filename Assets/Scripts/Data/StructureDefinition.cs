using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'une construction : bâtiment, déco, la Place. Son modèle du pack (la source) et le modèle préparé
    /// depuis lui ; son empreinte au sol et sa porte, mesurées sur le modèle par le menu DICIELA/Contenu/Préparer les
    /// modèles (SAVOIR, 2.9). Sans modèle du pack, le même menu lui fait une boîte à la taille de sa fiche, sa porte
    /// marquée (GDD, section 16). À l'angle 0, la construction regarde vers le nord (+z), où s'ouvre sa porte.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Construction", fileName = "Construction")]
    public sealed class StructureDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Largeur de l'empreinte au sol, en mètres (d'ouest en est à l'angle 0).")]
        [SerializeField, Min(0.25f)] private float _widthMeters = 4f;

        [Tooltip("Profondeur de l'empreinte au sol, en mètres (du sud au nord à l'angle 0).")]
        [SerializeField, Min(0.25f)] private float _depthMeters = 4f;

        [Tooltip("Vrai si la construction a une porte, devant laquelle rien ne se pose.")]
        [SerializeField] private bool _hasDoor = true;

        [Tooltip("Décalage de la porte depuis le milieu de la façade nord, en mètres (vers l'est si positif).")]
        [SerializeField] private float _doorOffsetMeters;

        [Tooltip("Largeur de la porte, en mètres.")]
        [SerializeField, Min(0.25f)] private float _doorWidthMeters = 1.5f;

        [Tooltip("Une seule dans la ville : la tente puis la maison de la Chimère, la Place, les grands projets (GDD, " +
                 "section 9).")]
        [SerializeField] private bool _unique;

        [Tooltip("Le logis de la Chimère : elle y dort quand le joueur le touche (GDD, section 6). La tente, puis sa " +
                 "maison ; il lui faut une porte.")]
        [SerializeField] private bool _chimeraHome;

        [Tooltip("Dressée dès qu'on la pose, sans chantier : la tente, que la Chimère plante d'un coup (GDD, section 9).")]
        [SerializeField] private bool _builtWhenPlaced;

        [Tooltip("Le modèle du pack, tel quel : la source que le menu DICIELA/Contenu/Préparer les modèles mesure et " +
                 "fusionne. Vide tant que le modèle n'est pas choisi : le menu fait alors une boîte à la taille de la fiche.")]
        [SerializeField] private GameObject _sourceModel;

        [Tooltip("Le modèle préparé par ce menu, depuis la source ou en boîte : fusionné, porte au nord, centré sur " +
                 "l'empreinte, son origine au sol. La vue le montre ; la simulation ne s'en sert pas.")]
        [SerializeField] private GameObject _model;

        public string Id => _id;
        public string NameKey => _nameKey;
        public float WidthMeters => _widthMeters;
        public float DepthMeters => _depthMeters;
        public bool HasDoor => _hasDoor;
        public float DoorOffsetMeters => _doorOffsetMeters;
        public float DoorWidthMeters => _doorWidthMeters;
        public bool IsUnique => _unique;
        public bool IsChimeraHome => _chimeraHome;
        public bool BuiltWhenPlaced => _builtWhenPlaced;
        public GameObject SourceModel => _sourceModel;
        public GameObject Model => _model;
    }
}
