using System;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La caméra de gestion (GDD, section 16) : elle regarde un point du sol, qui reste sur la carte et suit son relief,
    /// d'une distance qui règle aussi l'inclinaison : de loin, la ville en maquette ; de près, les habitants presque de
    /// face. On peut l'incliner soi-même, dans des bornes. Tirer le sol et zoomer gardent sous le pointeur le point du
    /// sol qu'il vise (GroundGrip). Elle ne lit pas les touches : les gestes l'appellent.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        // Le zoom est fini quand la distance est à moins d'un centimètre de sa cible.
        private const float SettledMeters = 0.01f;

        [Tooltip("La fiche des réglages de la caméra (Assets/Data/Settings).")]
        [SerializeField] private CameraSettings _settings;

        private readonly GroundGrip _grip = new GroundGrip();
        private Camera _camera;
        private Relief _relief;
        private Vector2 _pivot;
        private float _yawDegrees;
        private float _tiltDegrees;
        private float _targetDistance;
        private float _distance;
        private float _distanceVelocity;

        /// <summary>Vrai quand la caméra montre une carte.</summary>
        public bool IsShowing => _relief != null;

        /// <summary>La caméra elle-même, pour les rayons de la visée.</summary>
        public Camera Camera => _camera;

        /// <summary>La distance au point visé, en mètres, telle qu'on la voit (lissée).</summary>
        public float Distance => _distance;

        /// <summary>L'inclinaison de la vue, en degrés sous l'horizon.</summary>
        public float PitchDegrees => CameraPose.Pitch(_settings, _distance, _tiltDegrees);

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        /// <summary>
        /// Montre la carte depuis son point d'arrivée, où apparaît la Chimère, à la distance de départ. Appelé par le
        /// démarrage.
        /// </summary>
        public void Show(MapDescription map)
        {
            if (ReferenceEquals(map, null))
                throw new ArgumentNullException(nameof(map));
            if (_settings == null)
                throw new InvalidOperationException("La caméra n'a pas sa fiche de réglages.");

            _relief = map.Relief;
            _pivot = map.ArrivalPoint;
            _yawDegrees = 0f;
            _tiltDegrees = 0f;
            _targetDistance = Mathf.Clamp(_settings.StartDistance, _settings.NearDistance, _settings.FarDistance);
            _distance = _targetDistance;
            _distanceVelocity = 0f;
            _grip.Clear();
            _camera.fieldOfView = _settings.FieldOfView;
            _camera.farClipPlane = _settings.FarClipPlane;
            Apply();
        }

        /// <summary>Cesse de montrer la carte : le monde est parti, les gestes n'ont plus rien à bouger.</summary>
        public void Hide()
        {
            _relief = null;
            _grip.Clear();
            _distanceVelocity = 0f;
        }

        /// <summary>Déplace le point visé : x vers la droite de l'écran, y vers le haut, plus vite quand on est loin.</summary>
        public void Pan(Vector2 direction, float deltaSeconds)
        {
            if (!IsShowing)
                return;
            _grip.ReleaseZoom();
            SetPivot(_pivot + CameraPose.ViewToGround(_yawDegrees,
                direction * (_distance * _settings.PanSpeedPerDistance * deltaSeconds)));
        }

        /// <summary>
        /// Saisit le sol sous ce point de l'écran, pour le tirer ensuite avec <see cref="DragTo"/>. Faux si ce point ne
        /// vise pas le sol (au-dessus de l'horizon, ou plus loin que ce que la caméra affiche) : rien n'est saisi.
        /// </summary>
        public bool BeginDrag(Vector2 screenPoint)
        {
            return IsShowing && _grip.BeginDrag(_camera, _relief, screenPoint, PivotHeight());
        }

        /// <summary>
        /// Tire le sol saisi jusqu'à ce point de l'écran : il revient sous le pointeur, quelle que soit l'inclinaison de
        /// la vue (SAVOIR, 2.8).
        /// </summary>
        public void DragTo(Vector2 screenPoint)
        {
            if (IsShowing && _grip.TryDrag(_camera, screenPoint, out Vector2 shift))
                SetPivot(_pivot + shift);
        }

        /// <summary>Tourne autour du point visé ; un angle positif tourne la vue vers la droite.</summary>
        public void Rotate(float degrees)
        {
            if (!IsShowing)
                return;
            _grip.ReleaseZoom();
            _yawDegrees = Mathf.Repeat(_yawDegrees + degrees, 360f);
        }

        /// <summary>
        /// Incline la vue autour du point visé : un angle positif la relève vers l'horizon. L'inclinaison choisie tient,
        /// zoom compris, entre ses bornes (CameraPose).
        /// </summary>
        public void Tilt(float degrees)
        {
            if (!IsShowing)
                return;
            _grip.ReleaseZoom();
            float curve = CameraPose.CurvePitch(_settings, _distance);
            _tiltDegrees = Mathf.Clamp(_tiltDegrees - degrees, CameraPose.LowestPitch(_settings) - curve,
                CameraPose.HighestPitch(_settings) - curve);
        }

        /// <summary>
        /// Rapproche (crans positifs) ou éloigne (crans négatifs) la caméra, entre ses deux bornes, vers le sol sous ce
        /// point de l'écran : il reste sous le pointeur pendant tout le zoom. Au-dessus de l'horizon, vers le point visé.
        /// </summary>
        public void Zoom(float notches, Vector2 screenPoint)
        {
            if (!IsShowing)
                return;
            float factor = Mathf.Pow(1f - _settings.ZoomStepPerNotch, notches);
            _targetDistance = Mathf.Clamp(_targetDistance * factor, _settings.NearDistance, _settings.FarDistance);
            _grip.BeginZoom(_camera, _relief, screenPoint, PivotHeight());
        }

        private void LateUpdate()
        {
            if (!IsShowing)
                return;
            _distance = _settings.ZoomSmoothing > 0f
                ? Mathf.SmoothDamp(_distance, _targetDistance, ref _distanceVelocity, _settings.ZoomSmoothing,
                    Mathf.Infinity, Time.unscaledDeltaTime)
                : _targetDistance;
            Apply();
            if (!_grip.IsZooming)
                return;

            if (_grip.TryKeepZoom(_camera, out Vector2 shift))
            {
                SetPivot(_pivot + shift);
                Apply();
            }
            if (Mathf.Abs(_distance - _targetDistance) < SettledMeters)
                _grip.ReleaseZoom();
        }

        private float PivotHeight()
        {
            return _relief.HeightAt(_pivot.x, _pivot.y);
        }

        /// <summary>Pose le point visé, en mètres sur le plan XZ, sans quitter la carte.</summary>
        private void SetPivot(Vector2 pivot)
        {
            _pivot = new Vector2(Mathf.Clamp(pivot.x, 0f, _relief.SizeX), Mathf.Clamp(pivot.y, 0f, _relief.SizeZ));
        }

        private void Apply()
        {
            CameraPose.Place(transform, new Vector3(_pivot.x, PivotHeight(), _pivot.y), _yawDegrees, PitchDegrees,
                _distance, _relief, _settings.GroundClearance);
        }
    }
}
