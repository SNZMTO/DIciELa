using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Les réglages de la pose (GDD, sections 2 et 9) : tourner, les couleurs de l'aperçu et du fantôme, le fanion des
    /// arbres et des rochers marqués à retirer, le chantier qui monte (décision 166 : ses deux matériaux, sa ligne qui
    /// ondule), et l'outil de la Place (décision 206 : son pinceau, son voile, ses lignes). Les valeurs écrites ici ne
    /// servent qu'à une fiche neuve : la fiche fait foi.
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

        [Tooltip("Le fantôme d'un sol en chantier (Diciela/SolFantome) : sa part de texture et son opacité s'y règlent ; " +
                 "chaque sorte de sol en a une copie, à la texture de son calque.")]
        [SerializeField] private Material _floorGhostMaterial;

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

        [Header("Les outils de sol (décisions 206 et 216)")]
        [Tooltip("Le rayon du pinceau et de la gomme quand on prend l'outil de la Place, en mètres.")]
        [SerializeField, Range(0.25f, 10f)] private float _brushRadiusMeters = 2f;

        [Tooltip("Le rayon du pinceau et de la gomme quand on prend l'outil Chemins, en mètres : un chemin de 2 m de " +
                 "large (GDD, section 9).")]
        [SerializeField, Range(0.25f, 10f)] private float _pathBrushRadiusMeters = 1f;

        [Tooltip("Le plus petit et le plus grand rayon du pinceau, en mètres.")]
        [SerializeField] private Vector2 _brushRadiusRangeMeters = new Vector2(0.5f, 6f);

        [Tooltip("Ce que change un cran de Ctrl + molette au rayon du pinceau, en mètres.")]
        [SerializeField, Range(0.05f, 2f)] private float _brushRadiusStepMeters = 0.5f;

        [Tooltip("L'écart entre deux points d'un trait de pinceau, en mètres (SAVOIR, 2.6 : quelques décimètres).")]
        [SerializeField, Range(0.05f, 2f)] private float _strokeSpacingMeters = 0.3f;

        [Tooltip("À l'écran, à combien de pixels d'un point de tracé le pointeur le prend : un clic sur le dernier " +
                 "point fait partir le tracé.")]
        [SerializeField, Range(2f, 40f)] private float _handlePickPixels = 14f;

        [Tooltip("Le rayon du rond qui montre un point posé d'un tracé ou d'un polygone, en mètres.")]
        [SerializeField, Range(0.05f, 2f)] private float _handleRadiusMeters = 0.35f;

        [Tooltip("La plus grande longueur des petits segments d'une courbe (un tracé, un polygone arrondi), en mètres : " +
                 "une demi-maille suffit.")]
        [SerializeField, Range(0.05f, 2f)] private float _curveStepMeters = 0.25f;

        [Tooltip("Les branches de l'étoile (la forme comprise), au départ, puis au moins et au plus.")]
        [SerializeField, Range(2, 12)] private int _startStarBranches = 4;

        [SerializeField] private Vector2Int _starBranchesRange = new Vector2Int(3, 8);

        [Tooltip("La longueur des axes que l'épingle de la symétrie montre au sol, de part et d'autre, en mètres.")]
        [SerializeField, Range(0.5f, 20f)] private float _pinAxisMeters = 3f;

        [Tooltip("À combien de mètres de la place libre à côté d'une place de charrette posée le pointeur s'y " +
                 "aimante.")]
        [SerializeField, Range(0f, 10f)] private float _cartMagnetMeters = 1.5f;

        [Tooltip("L'écart entre deux places de charrette aimantées, en mètres : au moins la diagonale d'une maille (0,71 m " +
                 "pour des mailles de 50 cm), sinon elles se toucheraient.")]
        [SerializeField, Range(0f, 5f)] private float _cartSpotGapMeters = 0.8f;

        [Tooltip("Le voile de la Place sur le sol, en mode construction, du matériau translucide.")]
        [SerializeField] private Color _zoneVeilColor = new Color(1f, 0.97f, 0.9f, 0.22f);

        [Tooltip("Le contour de la Place et ses places de charrette, comme les fantômes.")]
        [SerializeField] private Color _outlineColor = new Color(1f, 1f, 1f, 0.85f);

        [Tooltip("La largeur du contour et du cadre d'une place de charrette, en mètres.")]
        [SerializeField, Range(0.02f, 0.5f)] private float _outlineWidthMeters = 0.12f;

        [Tooltip("La hauteur du voile et des lignes au-dessus du relief, en mètres : assez pour ne pas se mêler au sol.")]
        [SerializeField, Range(0f, 0.3f)] private float _overlayLiftMeters = 0.05f;

        [Tooltip("L'arrondi des formes tracées sur le sol, en mètres (décision 227) : la Place, les sols, leurs " +
                 "fantômes et l'aperçu d'un geste suivent leurs mailles, leurs coins arrondis à peu près de ce " +
                 "rayon, sans escalier.")]
        [SerializeField, Range(0.25f, 2f)] private float _groundRoundingMeters = 0.5f;

        [Tooltip("La douceur du bord d'un sol peint et de son liant, en mètres : sur cette largeur, il se fond " +
                 "dans ce qui l'entoure ; le Terrain l'adoucit encore d'un texel.")]
        [SerializeField, Range(0.02f, 1f)] private float _floorEdgeSoftnessMeters = 0.15f;

        public float RotationStepDegrees => _rotationStepDegrees;
        public float RotationDegreesPerPixel => _rotationDegreesPerPixel;
        public Material TranslucentMaterial => _translucentMaterial;
        public Color AllowedColor => _allowedColor;
        public Color RefusedColor => _refusedColor;
        public Color PlannedColor => _plannedColor;
        public Material SiteMaterial => _siteMaterial;
        public Material SiteGhostMaterial => _siteGhostMaterial;
        public Material FloorGhostMaterial => _floorGhostMaterial;
        public float BlendedCutoff => _blendedCutoff;
        public float WaveAmplitude => _waveAmplitude;
        public float WaveLength => _waveLength;
        public float WaveSeconds => _waveSeconds;
        public float SiteSoftness => _siteSoftness;
        public Color MarkedColor => _markedColor;
        public float MarkHeightMeters => _markHeightMeters;
        public float BrushRadiusMeters => _brushRadiusMeters;
        public float PathBrushRadiusMeters => _pathBrushRadiusMeters;
        public Vector2 BrushRadiusRangeMeters => _brushRadiusRangeMeters;
        public float BrushRadiusStepMeters => _brushRadiusStepMeters;
        public float StrokeSpacingMeters => _strokeSpacingMeters;
        public float HandlePickPixels => _handlePickPixels;
        public float HandleRadiusMeters => _handleRadiusMeters;
        public float CurveStepMeters => _curveStepMeters;
        public int StartStarBranches => _startStarBranches;
        public Vector2Int StarBranchesRange => _starBranchesRange;
        public float PinAxisMeters => _pinAxisMeters;
        public float CartMagnetMeters => _cartMagnetMeters;
        public float CartSpotGapMeters => _cartSpotGapMeters;
        public Color ZoneVeilColor => _zoneVeilColor;
        public Color OutlineColor => _outlineColor;
        public float OutlineWidthMeters => _outlineWidthMeters;
        public float OverlayLiftMeters => _overlayLiftMeters;
        public float GroundRoundingMeters => _groundRoundingMeters;
        public float FloorEdgeSoftnessMeters => _floorEdgeSoftnessMeters;
    }
}
