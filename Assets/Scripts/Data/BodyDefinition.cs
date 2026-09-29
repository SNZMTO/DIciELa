using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'un corps (GDD, section 6) : le corps d'un animal du pack et ses habits, sans sa tête, séparé du modèle
    /// par le menu DICIELA/Données/Créer les fiches manquantes (SAVOIR, 2.11). Le personnage du pack d'où il vient sert
    /// de squelette : tous les animaux ont le même. Les pièces que ce personnage porte dans le pack ailleurs que sur la
    /// tête (la queue du chat) ne lui restent pas attachées : ce sont des choix comme les autres, qui habillent la Chimère
    /// de départ ; celles de la tête vont à la fiche de la tête.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Corps", fileName = "Corps")]
    public sealed class BodyDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Le personnage du pack d'où il vient, tel quel, l'origine à ses pieds et regardant vers +z : son squelette " +
                 "porte le corps et la tête. La vue s'en sert ; la simulation, jamais.")]
        [SerializeField] private GameObject _model;

        [Tooltip("Son maillage, séparé du modèle du pack par le menu (Assets/Art/Personnages), sans la tête.")]
        [SerializeField] private Mesh _mesh;

        [Tooltip("Sa couleur d'origine, celle que le pack lui donne : l'écran de création la remet quand on choisit ce " +
                 "corps.")]
        [SerializeField] private PaletteDefinition _palette;

        [Tooltip("Les pièces que son personnage porte dans le pack ailleurs que sur la tête (la queue du chat) : la " +
                 "Chimère de départ les porte, de la couleur du corps. Rempli par le menu.")]
        [SerializeField] private List<AttachmentDefinition> _worn = new List<AttachmentDefinition>();

        [Tooltip("Son dos (recul selon x et y) : ce qui s'y porte avance jusqu'à le toucher. Mesuré par le menu.")]
        [SerializeField] private SurfaceMap _back = new SurfaceMap();

        [Tooltip("Son ventre (avancée selon x et y) : ce qui se porte à la ceinture recule jusqu'à le toucher. Mesuré " +
                 "par le menu.")]
        [SerializeField] private SurfaceMap _front = new SurfaceMap();

        public string Id => _id;
        public string NameKey => _nameKey;
        public GameObject Model => _model;
        public Mesh Mesh => _mesh;
        public PaletteDefinition Palette => _palette;
        public IReadOnlyList<AttachmentDefinition> Worn => _worn;

        /// <summary>Sa surface pour ce contact : le dos, le ventre ; null pour les autres.</summary>
        public SurfaceMap SurfaceOf(ContactAxis axis)
        {
            return axis == ContactAxis.Forward ? _back : axis == ContactAxis.Back ? _front : null;
        }
    }
}
