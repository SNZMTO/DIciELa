using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'une pièce (GDD, section 6) : un morceau détaché d'une tête (oreilles, touffes, museau) ou un
    /// accessoire du pack (chapeau, cape…), qui prend l'une des couleurs du pack, comme la tête et le corps. Elle est
    /// modelée debout, dans les axes du personnage : la vue pose son pivot au point du repère de son emplacement, sur la
    /// tête choisie, plus son décalage, la tourne et la met à sa taille, puis l'accroche à l'os de l'emplacement sans
    /// la bouger (SAVOIR, 2.11). Des oreilles (et une boucle d'oreille) suivent l'écartement des oreilles de la tête
    /// choisie, comparé à celui de la tête où elles ont été réglées. Décalage, rotation, taille et réglages par tête ou par
    /// corps se règlent ici, à l'œil, sur la planche de l'emplacement.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Pièce", fileName = "Piece")]
    public sealed class AttachmentDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Son emplacement : un personnage n'y porte qu'une pièce, et elle s'accroche à son os.")]
        [SerializeField] private AttachmentSlotDefinition _slot;

        [Tooltip("Son modèle : une pièce du pack (Prefabs/Characters/Chr_Attach) ou un morceau détaché d'une tête " +
                 "(Assets/Art/Personnages/Pieces). La vue s'en sert ; la simulation, jamais.")]
        [SerializeField] private GameObject _model;

        [Tooltip("Sa couleur d'origine, celle de son modèle : l'écran de création la remet quand on la choisit.")]
        [SerializeField] private PaletteDefinition _palette;

        [Tooltip("Où se pose son pivot, en mètres depuis le point du repère de son emplacement sur la tête choisie (ou " +
                 "depuis les pieds pour le corps) : x vers la droite du personnage, y vers le haut, z vers l'avant. Sur " +
                 "l'axe de contact de l'emplacement, depuis la place où elle touche : négatif, elle s'enfonce. Pour une paire " +
                 "(des oreilles), x écarte ses moitiés, chacune de son côté.")]
        [SerializeField] private Vector3 _offset;

        [Tooltip("Sa rotation, en degrés autour de x, y et z, autour de son pivot : pour redresser une pièce que le pack " +
                 "dessine couchée ou penchée. Relancer le menu des fiches après l'avoir changée : la face qui touche se " +
                 "remesure avec elle.")]
        [SerializeField] private Vector3 _rotation;

        [Tooltip("Sa taille selon ses trois axes, en fois celle du modèle (1, 1, 1 : telle quelle). Les pièces faites pour " +
                 "les humains sont agrandies pour une tête d'animal, d'un seul facteur, ou d'un facteur par axe pour celles " +
                 "qui enveloppent le crâne (cheveux, barbe). Zéro : pas encore réglée, le menu la remplit.")]
        [SerializeField] private Vector3 _scale;

        [Tooltip("Les emplacements qu'elle cache quand on la porte (un chapeau cache les oreilles et les cheveux) : leurs " +
                 "pièces restent choisies, mais ne se montrent pas.")]
        [SerializeField] private List<AttachmentSlotDefinition> _hides = new List<AttachmentSlotDefinition>();

        [Tooltip("Des décalages de plus, chacun sur ses têtes (une pièce de la tête) ou ses corps (une pièce du " +
                 "corps), pour les porteurs qui ne ressemblent pas aux autres.")]
        [SerializeField] private List<WearerAdjustment> _adjustments = new List<WearerAdjustment>();

        [Tooltip("La tête sur laquelle son décalage et sa taille ont été réglés : des oreilles suivent l'écartement des " +
                 "oreilles de la tête choisie, comparé à celui de celle-ci. Déduit du pack par le menu.")]
        [SerializeField] private HeadDefinition _reference;

        [Tooltip("Sa face qui touche le personnage (le dessous d'un chapeau, le dos de lunettes, le devant d'un sac), " +
                 "dans ses axes, à sa taille d'origine. Mesurée par le menu.")]
        [SerializeField] private SurfaceMap _shape = new SurfaceMap();

        public string Id => _id;
        public string NameKey => _nameKey;
        public AttachmentSlotDefinition Slot => _slot;
        public GameObject Model => _model;
        public PaletteDefinition Palette => _palette;
        public Vector3 Offset => _offset;
        public Vector3 Scale => _scale;
        public Vector3 Rotation => _rotation;
        public HeadDefinition Reference => _reference;

        /// <summary>Vrai si sa taille est réglée : positive selon ses trois axes.</summary>
        public bool HasScale => _scale.x > 0f && _scale.y > 0f && _scale.z > 0f;
        public IReadOnlyList<AttachmentSlotDefinition> Hides => _hides;

        /// <summary>
        /// Son décalage de plus sur ce porteur (zéro s'il n'y en a pas) : selon le corps pour une pièce du corps, selon
        /// la tête sinon.
        /// </summary>
        public Vector3 AdjustmentOn(HeadDefinition head, BodyDefinition body)
        {
            return TryAdjustment(head, body, out WearerAdjustment adjustment) ? adjustment.Offset : Vector3.zero;
        }

        /// <summary>Sa taille sur ce porteur : celle de son réglage, s'il en donne une ; sinon la sienne.</summary>
        public Vector3 ScaleOn(HeadDefinition head, BodyDefinition body)
        {
            return TryAdjustment(head, body, out WearerAdjustment adjustment) && adjustment.Scale != Vector3.zero
                ? adjustment.Scale
                : _scale;
        }

        /// <summary>Sa rotation sur ce porteur : la sienne, plus celle de son réglage.</summary>
        public Vector3 RotationOn(HeadDefinition head, BodyDefinition body)
        {
            return TryAdjustment(head, body, out WearerAdjustment adjustment) ? _rotation + adjustment.Rotation : _rotation;
        }

        /// <summary>Le modèle qu'elle prend sur ce porteur : celui de son réglage, s'il en donne un ; sinon le sien.</summary>
        public GameObject ModelOn(HeadDefinition head, BodyDefinition body)
        {
            return TryAdjustment(head, body, out WearerAdjustment adjustment) && adjustment.Model != null
                ? adjustment.Model
                : _model;
        }

        // Son réglage sur ce porteur : selon le corps pour une pièce du corps, selon la tête sinon.
        private bool TryAdjustment(HeadDefinition head, BodyDefinition body, out WearerAdjustment found)
        {
            bool onBody = _slot != null && _slot.TouchesBody;
            foreach (WearerAdjustment adjustment in _adjustments)
            {
                if (!adjustment.AppliesTo(head, body, onBody))
                    continue;
                found = adjustment;
                return true;
            }
            found = default;
            return false;
        }
        public SurfaceMap Shape => _shape;
    }
}
