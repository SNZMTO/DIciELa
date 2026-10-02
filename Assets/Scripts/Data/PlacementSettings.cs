using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Les réglages de la pose (GDD, sections 2 et 9) : tourner, les couleurs de l'aperçu et du fantôme, le fanion des
    /// arbres et des rochers marqués à retirer, et le chantier qui monte (décision 166 : ses deux matériaux, sa ligne qui
    /// ondule). Les valeurs écrites ici ne servent qu'à une fiche neuve : la fiche fait foi.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Réglages de la pose", fileName = "PlacementSettings")]
    public sealed class PlacementSettings : ScriptableObject
    {
        [Header("Tourner")]
        [Tooltip("Le cran d'une touche brève, en degrés (GDD, section 9).")]
        [SerializeField, Range(1f, 90f)] private float _rotationStepDegrees = 15f;

        [Tooltip("La touche tenue, en bougeant la souris de gauche à droite : degrés par pixel.")]
        [SerializeField, Range(0.01f, 2f)] private float _rotationDegreesPerPixel = 0.5f;

        [Header("Couleurs")]
        [Tooltip("Le matériau translucide des fanions des arbres marqués ; l'aperçu de pose a le fantôme du chantier.")]
        [SerializeField] private Material _translucentMaterial;

        [Tooltip("L'aperçu, quand la pose est permise.")]
        [SerializeField] private Color _allowedColor = new Color(0.35f, 0.85f, 0.4f, 0.5f);

        [Tooltip("L'aperçu, quand la pose est refusée.")]
        [SerializeField] private Color _refusedColor = new Color(0.95f, 0.3f, 0.25f, 0.5f);

        [Tooltip("Le fantôme : une construction posée qui attend d'être bâtie.")]
        [SerializeField] private Color _plannedColor = new Color(0.7f, 0.85f, 1f, 0.45f);

        [Tooltip("La vraie matière d'un chantier, coupée à la ligne qui monte (shader Diciela/Chantier).")]
        [SerializeField] private Material _siteMaterial;

        [Tooltip("Le fantôme d'un chantier, au-dessus de la ligne, sa lueur et ses étincelles (Diciela/ChantierFantome) ; " +
                 "l'aperçu de pose aussi, de ses couleurs.")]
        [SerializeField] private Material _siteGhostMaterial;

        [Tooltip("Sur un chantier et dans l'aperçu, le seuil où se détoure un matériau du pack transparent par mélange " +
                 "(le bord d'un chaume) : sa texture y est pleine ou vide.")]
        [SerializeField, Range(0.05f, 0.95f)] private float _blendedCutoff = 0.5f;

        [Tooltip("La ligne qui monte : la hauteur de son ondulation, en mètres.")]
        [SerializeField, Range(0f, 0.5f)] private float _waveAmplitude = 0.08f;

        [Tooltip("La ligne qui monte : la longueur d'une ondulation, en mètres.")]
        [SerializeField, Range(0.2f, 10f)] private float _waveLength = 1.6f;

        [Tooltip("La ligne qui monte : la durée d'une ondulation, en secondes.")]
        [SerializeField, Range(0.2f, 20f)] private float _waveSeconds = 3f;

        [Tooltip("La bande sous la ligne où la vraie matière s'effrite et où le fantôme s'efface, en mètres.")]
        [SerializeField, Range(0f, 1f)] private float _siteSoftness = 0.25f;

        [Tooltip("Le fanion d'un arbre ou d'un rocher marqué à retirer, du même matériau translucide (remplaçant simple).")]
        [SerializeField] private Color _markedColor = new Color(0.9f, 0.35f, 0.3f, 0.85f);

        [Tooltip("La hauteur du fanion, en mètres, planté au pied de l'arbre ou du rocher marqué.")]
        [SerializeField, Range(0.5f, 5f)] private float _markHeightMeters = 1.8f;

        public float RotationStepDegrees => _rotationStepDegrees;
        public float RotationDegreesPerPixel => _rotationDegreesPerPixel;
        public Material TranslucentMaterial => _translucentMaterial;
        public Color AllowedColor => _allowedColor;
        public Color RefusedColor => _refusedColor;
        public Color PlannedColor => _plannedColor;
        public Material SiteMaterial => _siteMaterial;
        public Material SiteGhostMaterial => _siteGhostMaterial;
        public float BlendedCutoff => _blendedCutoff;
        public float WaveAmplitude => _waveAmplitude;
        public float WaveLength => _waveLength;
        public float WaveSeconds => _waveSeconds;
        public float SiteSoftness => _siteSoftness;
        public Color MarkedColor => _markedColor;
        public float MarkHeightMeters => _markHeightMeters;
    }
}
