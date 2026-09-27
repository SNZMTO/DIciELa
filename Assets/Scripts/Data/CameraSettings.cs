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
        [Tooltip("Champ de vision vertical, en degrés (la démo du pack : autour de 40°).")]
        [SerializeField, Range(10f, 90f)] private float _fieldOfView = 40f;

        [Tooltip("Distance au sol la plus proche, en mètres : les habitants presque de face.")]
        [SerializeField, Min(1f)] private float _nearDistance = 15f;

        [Tooltip("Distance au sol la plus lointaine, en mètres : la ville en maquette.")]
        [SerializeField, Min(1f)] private float _farDistance = 220f;

        [Tooltip("Inclinaison de la vue au plus près, en degrés sous l'horizon.")]
        [SerializeField, Range(5f, 89f)] private float _nearPitch = 20f;

        [Tooltip("Inclinaison de la vue au plus loin, en degrés sous l'horizon.")]
        [SerializeField, Range(5f, 89f)] private float _farPitch = 60f;

        [Tooltip("Hauteur minimale de la caméra au-dessus du sol, en mètres.")]
        [SerializeField, Min(0f)] private float _groundClearance = 2f;

        [Tooltip("Distance d'affichage la plus lointaine, en mètres.")]
        [SerializeField, Min(10f)] private float _farClipPlane = 1500f;

        [Header("Gestes")]
        [Tooltip("Vitesse de déplacement, en fractions de la distance au sol par seconde.")]
        [SerializeField, Min(0f)] private float _panSpeedPerDistance = 0.9f;

        [Tooltip("Vitesse de rotation, en degrés par seconde.")]
        [SerializeField, Min(0f)] private float _rotateDegreesPerSecond = 90f;

        [Tooltip("Rapprochement par cran de molette : 0,15 = 15 % plus près.")]
        [SerializeField, Range(0.01f, 0.9f)] private float _zoomStepPerNotch = 0.15f;

        [Tooltip("Valeur de défilement d'un cran de molette : 1 quand l'Input System la ramène à la même échelle partout " +
                 "(son réglage par défaut), 120 sinon, sous Windows.")]
        [SerializeField, Min(0.01f)] private float _scrollPerNotch = 1f;

        [Tooltip("Temps de lissage du zoom, en secondes (0 : immédiat).")]
        [SerializeField, Min(0f)] private float _zoomSmoothing = 0.12f;

        [Tooltip("Bande au bord de l'écran qui fait défiler la vue, en pixels (0 : jamais).")]
        [SerializeField, Min(0f)] private float _edgeScrollPixels = 12f;

        [Tooltip("Distance en pixels au-delà de laquelle un clic devient un glisser.")]
        [SerializeField, Min(0f)] private float _dragThresholdPixels = 6f;

        public float FieldOfView => _fieldOfView;
        public float NearDistance => _nearDistance;
        public float FarDistance => _farDistance;
        public float NearPitch => _nearPitch;
        public float FarPitch => _farPitch;
        public float GroundClearance => _groundClearance;
        public float FarClipPlane => _farClipPlane;
        public float PanSpeedPerDistance => _panSpeedPerDistance;
        public float RotateDegreesPerSecond => _rotateDegreesPerSecond;
        public float ZoomStepPerNotch => _zoomStepPerNotch;
        public float ScrollPerNotch => _scrollPerNotch;
        public float ZoomSmoothing => _zoomSmoothing;
        public float EdgeScrollPixels => _edgeScrollPixels;
        public float DragThresholdPixels => _dragThresholdPixels;
    }
}
