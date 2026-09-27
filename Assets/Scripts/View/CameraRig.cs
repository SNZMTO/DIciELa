using System;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La caméra de gestion (GDD, section 16) : elle regarde un point du sol, qui reste sur la carte et suit son relief,
    /// d'une distance qui règle aussi l'inclinaison : de loin, la ville en maquette ; de près, les habitants presque de
    /// face. Tirer le sol et zoomer gardent sous le pointeur le point du sol qu'il vise. Elle ne lit pas les touches :
    /// les gestes l'appellent.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        // Le zoom est fini quand la distance est à moins d'un centimètre de sa cible.
        private const float SettledMeters = 0.01f;

        [Tooltip("La fiche des réglages de la caméra (Assets/Data/Settings).")]
        [SerializeField] private CameraSettings _settings;

        private Camera _camera;
        private Relief _relief;
        private Vector2 _pivot;
        private float _yawDegrees;
        private float _targetDistance;
        private float _distance;
        private float _distanceVelocity;
        private bool _grabbing;
        private Vector3 _grabbedPoint;
        private bool _zoomAnchored;
        private Vector3 _zoomAnchor;
        private Vector2 _zoomScreenPoint;

        /// <summary>Vrai quand la caméra montre une carte.</summary>
        public bool IsShowing => _relief != null;

        /// <summary>La caméra elle-même, pour les rayons de la visée.</summary>
        public Camera Camera => _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        /// <summary>Montre la carte depuis son centre, à mi-distance. Appelé par le démarrage quand le monde naît.</summary>
        public void Show(Relief relief)
        {
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));
            if (_settings == null)
                throw new InvalidOperationException("La caméra n'a pas sa fiche de réglages.");

            _relief = relief;
            _pivot = new Vector2(relief.SizeX / 2f, relief.SizeZ / 2f);
            _yawDegrees = 0f;
            _targetDistance = (_settings.NearDistance + _settings.FarDistance) / 2f;
            _distance = _targetDistance;
            _distanceVelocity = 0f;
            _grabbing = false;
            _zoomAnchored = false;
            _camera.fieldOfView = _settings.FieldOfView;
            _camera.farClipPlane = _settings.FarClipPlane;
            Apply();
        }

        /// <summary>Déplace le point visé : x vers la droite de l'écran, y vers le haut, plus vite quand on est loin.</summary>
        public void Pan(Vector2 direction, float deltaSeconds)
        {
            if (!IsShowing)
                return;
            _zoomAnchored = false;
            MovePivot(direction * (_distance * _settings.PanSpeedPerDistance * deltaSeconds));
        }

        /// <summary>
        /// Saisit le sol sous ce point de l'écran, pour le tirer ensuite avec <see cref="DragTo"/>. Faux si ce point ne
        /// vise pas le sol (au-dessus de l'horizon, ou plus loin que ce que la caméra affiche) : rien n'est saisi.
        /// </summary>
        public bool BeginDrag(Vector2 screenPoint)
        {
            _zoomAnchored = false;
            _grabbing = IsShowing &&
                        TryGroundPoint(screenPoint, _relief.HeightAt(_pivot.x, _pivot.y), out _grabbedPoint);
            return _grabbing;
        }

        /// <summary>
        /// Tire le sol saisi jusqu'à ce point de l'écran : il revient sous le pointeur, quelle que soit l'inclinaison de
        /// la vue (SAVOIR, 2.8).
        /// </summary>
        public void DragTo(Vector2 screenPoint)
        {
            if (IsShowing && _grabbing)
                BringUnder(_grabbedPoint, screenPoint);
        }

        /// <summary>Tourne autour du point visé ; un angle positif tourne la vue vers la droite.</summary>
        public void Rotate(float degrees)
        {
            if (!IsShowing)
                return;
            _zoomAnchored = false;
            _yawDegrees = Mathf.Repeat(_yawDegrees + degrees, 360f);
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
            _zoomAnchored = TryGroundPoint(screenPoint, _relief.HeightAt(_pivot.x, _pivot.y), out _zoomAnchor);
            _zoomScreenPoint = screenPoint;
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
            if (!_zoomAnchored)
                return;

            BringUnder(_zoomAnchor, _zoomScreenPoint);
            Apply();
            if (Mathf.Abs(_distance - _targetDistance) < SettledMeters)
                _zoomAnchored = false;
        }

        /// <summary>Déplace le point visé pour que ce point du sol revienne sous ce point de l'écran.</summary>
        private void BringUnder(Vector3 groundPoint, Vector2 screenPoint)
        {
            if (TryGroundPoint(screenPoint, groundPoint.y, out Vector3 underPointer))
                SetPivot(_pivot + new Vector2(groundPoint.x - underPointer.x, groundPoint.z - underPointer.z));
        }

        /// <summary>Déplace le point visé dans le repère de la vue (x à droite, y devant).</summary>
        private void MovePivot(Vector2 viewMeters)
        {
            float yaw = _yawDegrees * Mathf.Deg2Rad;
            var right = new Vector2(Mathf.Cos(yaw), -Mathf.Sin(yaw));
            var forward = new Vector2(Mathf.Sin(yaw), Mathf.Cos(yaw));
            SetPivot(_pivot + right * viewMeters.x + forward * viewMeters.y);
        }

        /// <summary>Pose le point visé, en mètres sur le plan XZ, sans quitter la carte.</summary>
        private void SetPivot(Vector2 pivot)
        {
            _pivot = new Vector2(Mathf.Clamp(pivot.x, 0f, _relief.SizeX), Mathf.Clamp(pivot.y, 0f, _relief.SizeZ));
        }

        /// <summary>
        /// Le point où le rayon de ce point de l'écran touche le plan horizontal à cette hauteur ; faux s'il ne le touche
        /// pas, ou plus loin que ce que la caméra affiche.
        /// </summary>
        private bool TryGroundPoint(Vector2 screenPoint, float height, out Vector3 point)
        {
            Ray ray = _camera.ScreenPointToRay(screenPoint);
            var ground = new Plane(Vector3.up, new Vector3(0f, height, 0f));
            if (ground.Raycast(ray, out float along) && along <= _camera.farClipPlane)
            {
                point = ray.GetPoint(along);
                return true;
            }
            point = default;
            return false;
        }

        private void Apply()
        {
            float farness = Mathf.InverseLerp(_settings.NearDistance, _settings.FarDistance, _distance);
            float pitch = Mathf.Lerp(_settings.NearPitch, _settings.FarPitch, farness);
            Quaternion rotation = Quaternion.Euler(pitch, _yawDegrees, 0f);
            var target = new Vector3(_pivot.x, _relief.HeightAt(_pivot.x, _pivot.y), _pivot.y);

            Vector3 position = target - rotation * Vector3.forward * _distance;
            float ground = _relief.HeightAt(position.x, position.z) + _settings.GroundClearance;
            if (position.y < ground)
                position.y = ground;

            transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position, Vector3.up));
        }
    }
}
