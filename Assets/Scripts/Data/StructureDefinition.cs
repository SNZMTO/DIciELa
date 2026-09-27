using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'une construction : bâtiment, déco, la Place. Son empreinte au sol et sa porte (mesurées sur le modèle
    /// par un menu, SAVOIR 2.9) ; à l'angle 0, la construction regarde vers le nord (+z), où s'ouvre sa porte.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Construction", fileName = "Construction")]
    public sealed class StructureDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

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

        [Tooltip("Le modèle, pour la vue ; la simulation ne s'en sert pas.")]
        [SerializeField] private GameObject _model;

        public string Id => _id;
        public float WidthMeters => _widthMeters;
        public float DepthMeters => _depthMeters;
        public bool HasDoor => _hasDoor;
        public float DoorOffsetMeters => _doorOffsetMeters;
        public float DoorWidthMeters => _doorWidthMeters;
        public GameObject Model => _model;
    }
}
