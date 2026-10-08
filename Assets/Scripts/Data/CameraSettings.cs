using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Les réglages de la caméra de gestion (GDD, section 16). Les valeurs écrites ici ne servent qu'à une fiche neuve :
    /// la fiche fait foi, et ses chiffres se règlent en jouant.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Réglages de la caméra", fileName = "CameraSettings")]
    public sealed class CameraSettings : ScriptableObject
    {
        [Header("Vue")]
        [Tooltip("Champ de vision vertical, en degrés : 35, un peu plus serré que la démo du pack (40), plus maquette.")]
        [SerializeField, Range(10f, 90f)] private float _fieldOfView = 35f;

        [Tooltip("Distance au sol la plus proche, en mètres : les habitants presque de face.")]
        [SerializeField, Min(1f)] private float _nearDistance = 8f;

        [Tooltip("Distance au sol la plus lointaine, en mètres : la ville en maquette.")]
        [SerializeField, Min(1f)] private float _farDistance = 220f;

        [Tooltip("Distance au sol au lancement, en mètres : 25, près de la Chimère et de son campement.")]
        [SerializeField, Min(1f)] private float _startDistance = 25f;

        [Tooltip("Inclinaison de la vue au plus près, en degrés sous l'horizon : 22,5, le plus bas permis " +
                 "(la moitié du champ de vision, plus la marge de l'horizon).")]
        [SerializeField, Range(5f, 89f)] private float _nearPitch = 22.5f;

        [Tooltip("Inclinaison de la vue au plus loin, en degrés sous l'horizon.")]
        [SerializeField, Range(5f, 89f)] private float _farPitch = 50f;

        [Tooltip("Inclinaison la plus forte, en degrés sous l'horizon, même en inclinant soi-même : la vue d'oiseau.")]
        [SerializeField, Range(5f, 90f)] private float _maxPitch = 80f;

        [Tooltip("Marge du haut de l'écran sous l'horizon, en degrés : on ne voit jamais le vide au-delà de la carte, et " +
                 "le sol près du haut de l'écran reste proche.")]
        [SerializeField, Range(0f, 30f)] private float _horizonMarginDegrees = 5f;

        [Tooltip("Hauteur minimale de la caméra au-dessus du sol, en mètres.")]
        [SerializeField, Min(0f)] private float _groundClearance = 2f;

        [Tooltip("Distance d'affichage la plus lointaine, en mètres.")]
        [SerializeField, Min(10f)] private float _farClipPlane = 1500f;

        [Header("Ombres et brouillard, selon le zoom")]
        [Tooltip("Distance des ombres la plus courte, en mètres (de près, des ombres fines).")]
        [SerializeField, Min(1f)] private float _shadowMinMeters = 50f;

        [Tooltip("Distance des ombres la plus longue, en mètres : elle couvre le sol visible, de loin (SAVOIR, 2.10).")]
        [SerializeField, Min(1f)] private float _shadowMaxMeters = 300f;

        [Tooltip("Le brouillard de la carte ne commence pas avant ce multiple de la distance au sol : la ville vue de loin " +
                 "reste nette ; de près, le brouillard de la carte ne change pas.")]
        [SerializeField, Min(0f)] private float _fogStartPerDistance = 1.3f;

        [Header("Gestes")]
        [Tooltip("Vitesse de déplacement, en fractions de la distance au sol par seconde.")]
        [SerializeField, Min(0f)] private float _panSpeedPerDistance = 0.9f;

        [Tooltip("Vitesse de rotation, en degrés par seconde.")]
        [SerializeField, Min(0f)] private float _rotateDegreesPerSecond = 90f;

        [Tooltip("Rotation au bouton du milieu glissé, en degrés par pixel. Positive, la vue tourne du côté où va la " +
                 "souris, comme la touche E vers la droite ; négative, le sol suit la souris.")]
        [SerializeField, Range(-2f, 2f)] private float _orbitDegreesPerPixel = 0.25f;

        [Tooltip("Inclinaison au bouton du milieu glissé de haut en bas, en degrés par pixel. Positive, la vue se relève " +
                 "vers l'horizon quand la souris monte ; négative, l'inverse.")]
        [SerializeField, Range(-2f, 2f)] private float _tiltDegreesPerPixel = 0.2f;

        [Tooltip("Rapprochement par cran de molette : 0,15 = 15 % plus près.")]
        [SerializeField, Range(0.01f, 0.9f)] private float _zoomStepPerNotch = 0.15f;

        [Tooltip("Valeur de défilement d'un cran de molette : 1 quand l'Input System la ramène à la même échelle partout " +
                 "(son réglage par défaut), 120 sinon, sous Windows.")]
        [SerializeField, Min(0.01f)] private float _scrollPerNotch = 1f;

        [Tooltip("Temps de lissage du zoom, en secondes (0 : immédiat).")]
        [SerializeField, Min(0f)] private float _zoomSmoothing = 0.12f;

        [Tooltip("Bande au bord de l'écran qui fait défiler la vue, en pixels (0 : jamais).")]
        [SerializeField, Min(0f)] private float _edgeScrollPixels = 12f;

        [Tooltip("La prévision (décision 228) : le temps du fondu de son filtre de rêverie, quand elle s'allume ou " +
                 "s'éteint, en secondes.")]
        [SerializeField, Range(0.01f, 2f)] private float _foresightBlendSeconds = 0.2f;

        [Tooltip("L'atelier : sans geste depuis tant de secondes, la Chimère se balance doucement sur son socle.")]
        [SerializeField, Min(0f)] private float _atelierIdleSeconds = 3f;

        [Tooltip("L'atelier : l'amplitude du balancement, en degrés de part et d'autre.")]
        [SerializeField, Range(0f, 45f)] private float _atelierSwayDegrees = 12f;

        [Tooltip("L'atelier : la durée d'un aller-retour du balancement, en secondes.")]
        [SerializeField, Min(0.5f)] private float _atelierSwaySeconds = 6f;

        public float FieldOfView => _fieldOfView;
        public float NearDistance => _nearDistance;
        public float FarDistance => _farDistance;
        public float StartDistance => _startDistance;
        public float NearPitch => _nearPitch;
        public float FarPitch => _farPitch;
        public float MaxPitch => _maxPitch;
        public float HorizonMarginDegrees => _horizonMarginDegrees;
        public float GroundClearance => _groundClearance;
        public float FarClipPlane => _farClipPlane;
        public float ShadowMinMeters => _shadowMinMeters;
        public float ShadowMaxMeters => _shadowMaxMeters;
        public float FogStartPerDistance => _fogStartPerDistance;
        public float PanSpeedPerDistance => _panSpeedPerDistance;
        public float RotateDegreesPerSecond => _rotateDegreesPerSecond;
        public float AtelierIdleSeconds => _atelierIdleSeconds;
        public float AtelierSwayDegrees => _atelierSwayDegrees;
        public float AtelierSwaySeconds => _atelierSwaySeconds;
        public float OrbitDegreesPerPixel => _orbitDegreesPerPixel;
        public float TiltDegreesPerPixel => _tiltDegreesPerPixel;
        public float ZoomStepPerNotch => _zoomStepPerNotch;
        public float ScrollPerNotch => _scrollPerNotch;
        public float ZoomSmoothing => _zoomSmoothing;
        public float EdgeScrollPixels => _edgeScrollPixels;
        public float ForesightBlendSeconds => _foresightBlendSeconds;
    }
}
