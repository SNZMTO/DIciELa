using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'une tête (GDD, section 6) : le crâne d'un animal du pack, ses yeux et ses sourcils compris. Ce qui s'en
    /// détache (ses oreilles, même cousues au crâne, ses touffes, le museau du renard) devient des pièces qu'elle porte
    /// d'office et qu'on peut changer. Le menu DICIELA/Données/Créer les fiches manquantes la découpe dans le modèle et mesure ses points, là où
    /// se posent les pièces de la tête (SAVOIR, 2.11). Elle va sur n'importe quel corps, tous ayant le même squelette.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Tête", fileName = "Tete")]
    public sealed class HeadDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Son crâne, découpé par le menu (Assets/Art/Personnages), sur le squelette commun. La vue s'en sert ; la " +
                 "simulation, jamais.")]
        [SerializeField] private Mesh _mesh;

        [Tooltip("Sa couleur d'origine, celle que le pack lui donne : l'écran de création la remet quand on choisit cette " +
                 "tête.")]
        [SerializeField] private PaletteDefinition _palette;

        [Tooltip("Les pièces qu'elle porte d'office : ce qui s'est détaché d'elle (oreilles, touffes) et ce que le " +
                 "pack pose sur sa tête (la huppe de l'oiseau). Rempli par le menu.")]
        [SerializeField] private List<AttachmentDefinition> _worn = new List<AttachmentDefinition>();

        [Tooltip("Les emplacements qu'elle ne porte pas (la grenouille : les oreilles) : l'écran de création ne les propose " +
                 "pas avec elle, et rien ne s'y pose. Rempli par le menu.")]
        [SerializeField] private List<AttachmentSlotDefinition> _refuses = new List<AttachmentSlotDefinition>();

        [Tooltip("Le haut de son crâne, en mètres depuis les pieds du personnage debout (x à sa droite, y en haut, z " +
                 "devant). Mesuré par le menu, jamais réglé à la main : une pièce se règle dans sa fiche.")]
        [SerializeField] private Vector3 _crown;

        [Tooltip("Le milieu de ses yeux, à leur avant. Mesuré par le menu.")]
        [SerializeField] private Vector3 _face;

        [Tooltip("La place de son oreille droite sur le crâne (x positif) ; la gauche en est le miroir. Mesurée par le " +
                 "menu.")]
        [SerializeField] private Vector3 _earRight;

        [Tooltip("Le bas de sa tête, sur la ligne du milieu. Mesuré par le menu.")]
        [SerializeField] private Vector3 _neck;

        [Tooltip("Le dessus de son crâne (hauteur selon x et z) : ce qui s'y pose descend jusqu'à le toucher. Mesuré par " +
                 "le menu.")]
        [SerializeField] private SurfaceMap _top = new SurfaceMap();

        [Tooltip("L'avant de sa face, yeux compris (avancée selon x et y) : ce qui s'y pose recule jusqu'à le toucher. " +
                 "Mesuré par le menu.")]
        [SerializeField] private SurfaceMap _front = new SurfaceMap();

        public string Id => _id;
        public string NameKey => _nameKey;
        public Mesh Mesh => _mesh;
        public PaletteDefinition Palette => _palette;
        public IReadOnlyList<AttachmentDefinition> Worn => _worn;

        /// <summary>Vrai si elle ne porte pas ce qui va à cet emplacement.</summary>
        public bool Refuses(AttachmentSlotDefinition slot)
        {
            foreach (AttachmentSlotDefinition refused in _refuses)
            {
                if (ReferenceEquals(refused, slot))
                    return true;
            }
            return false;
        }

        /// <summary>La place de son oreille droite sur le crâne ; la gauche en est le miroir.</summary>
        public Vector3 EarRight => _earRight;

        /// <summary>Sa surface pour ce contact : le dessus du crâne, l'avant de la face ; null pour les autres.</summary>
        public SurfaceMap SurfaceOf(ContactAxis axis)
        {
            return axis == ContactAxis.Down ? _top : axis == ContactAxis.Back ? _front : null;
        }

        /// <summary>
        /// Le point d'où se mesurent les pièces de ce repère, sur cette tête ; pour les oreilles, le milieu de leurs deux
        /// places ; les pieds pour le corps.
        /// </summary>
        public Vector3 PointOf(AttachmentFrame frame)
        {
            switch (frame)
            {
                case AttachmentFrame.Crown:
                    return _crown;
                case AttachmentFrame.Face:
                    return _face;
                case AttachmentFrame.Ears:
                    return new Vector3(0f, _earRight.y, _earRight.z);
                case AttachmentFrame.Neck:
                    return _neck;
                default:
                    return Vector3.zero;
            }
        }
    }
}
