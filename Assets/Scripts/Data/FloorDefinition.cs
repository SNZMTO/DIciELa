using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Une sorte de sol (GDD, section 9 ; décisions 209, 210, 227 et 232) : ce qu'on trace à l'outil Chemins, partout
    /// où l'on marche, ou à l'outil de la Place, dans sa zone. La terre, gratuite, dès le départ ; puis les pavés et
    /// les autres, chacun avec son idée. La fiche dit son nom, son aspect (le calque où il se peint dans le sol de la
    /// carte, son bord qui se fond et s'effiloche, son liant tout autour, qui se fond à son tour) et le pas d'une
    /// charrette tirée sur lui ; dans l'herbe, c'est la fiche de la charrette qui le dit. Son prix viendra avec les
    /// sols qui se paient.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Sol", fileName = "Sol")]
    public sealed class FloorDefinition : ScriptableObject
    {
        /// <summary>
        /// La plus grande largeur d'un liant, en mètres : la peinture d'un sol ne porte pas plus loin.
        /// </summary>
        public const float MaxBorderMeters = 3f;

        /// <summary>De combien un bord s'effiloche au plus, en mètres.</summary>
        public const float MaxFrayMeters = 1f;

        /// <summary>La plus grande largeur d'un fondu, en mètres, celui du sol comme celui de son liant.</summary>
        public const float MaxFadeMeters = 2f;

        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes.")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Son aspect : le calque du sol de la carte où il se peint quand il sert (GDD, section 16) ; celui " +
                 "du pack, ou une copie rangée hors du pack.")]
        [SerializeField] private TerrainLayer _layer;

        [Tooltip("Le fondu de son bord, en mètres (décision 232) : la largeur, centrée sur son bord, où il se mêle à " +
                 "ce qui l'entoure, son liant ou le sol de la carte, comme peint au pinceau doux dans la démo du " +
                 "pack.")]
        [SerializeField, Range(0.05f, MaxFadeMeters)] private float _fadeMeters = 1f;

        [Tooltip("Le liant : le calque qui se peint tout autour du sol, entre lui et l'herbe, comme la terre " +
                 "autour des pavés dans la démo du pack (décision 227) ; aucun pour un sol qui se fond tout seul " +
                 "dans l'herbe.")]
        [SerializeField] private TerrainLayer _borderLayer;

        [Tooltip("La largeur du liant, en mètres, depuis le bord du sol.")]
        [SerializeField, Range(0f, MaxBorderMeters)] private float _borderMeters = 0.8f;

        [Tooltip("Le fondu du bout de son liant dans le sol de la carte, en mètres, centré sur le bout du liant.")]
        [SerializeField, Range(0.05f, MaxFadeMeters)] private float _borderFadeMeters = 1f;

        [Tooltip("De combien son bord s'effiloche, en mètres : celui du sol et celui de son liant ; 0, un bord lisse.")]
        [SerializeField, Range(0f, MaxFrayMeters)] private float _frayMeters = 0.35f;

        [Tooltip("La taille des festons de son bord, en mètres : l'échelle du bruit qui l'effiloche.")]
        [SerializeField, Range(0.25f, 10f)] private float _frayScaleMeters = 1.5f;

        [Tooltip("La vitesse d'une charrette tirée sur ce sol, en part du pas de qui la tire : normale sur la terre, une " +
                 "fois et demie sur un sol dur (GDD, section 9).")]
        [SerializeField, Range(0.1f, 3f)] private float _cartSpeedFactor = 1f;

        public string Id => _id;
        public string NameKey => _nameKey;
        public TerrainLayer Layer => _layer;
        public TerrainLayer BorderLayer => _borderLayer;
        public float FadeMeters => _fadeMeters;
        public float BorderMeters => _borderLayer != null ? _borderMeters : 0f;
        public float BorderFadeMeters => _borderFadeMeters;
        public float FrayMeters => _frayMeters;
        public float FrayScaleMeters => _frayScaleMeters;
        public float CartSpeedFactor => _cartSpeedFactor;
    }
}
