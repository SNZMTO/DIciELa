using System;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La caméra de gestion (GDD, section 16) : elle regarde un point du sol, qui reste sur la carte et suit son relief,
    /// d'une distance qui règle aussi l'inclinaison : de loin, la ville en maquette ; de près, les habitants presque de
    /// face. Elle ne lit pas les touches : les gestes l'appellent.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la caméra (Assets/Data/Settings).")]
        [SerializeField] private CameraSettings _settings;

        private Camera _camera;
        private Relief _relief;
        private Vector2 _pivot;
        private float _yawDegrees;
        private float _targetDistance;
        private float _distance;
        private float _distanceVelocity;

        /// <summary>Vrai quand la caméra montre une carte.</summary>
        public bool IsShowing => _relief != null;

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
            _camera.fieldOfView = _settings.FieldOfView;
            _camera.farClipPlane = _settings.FarClipPlane;
            Apply();
        }

        /// <summary>Déplace le point visé : x vers la droite de l'écran, y vers le haut, plus vite quand on est loin.</summary>
        public void Pan(Vector2 direction, float deltaSeconds)
        {
            if (!IsShowing)
                return;
            MovePivot(direction * (_distance * _settings.PanSpeedPerDistance * deltaSeconds));
        }

        /// <summary>Déplace le point visé comme si l'on tirait le sol avec le pointeur, d'un nombre de pixels.</summary>
        public void Drag(Vector2 pixelDelta, float screenHeightPixels)
        {
            if (!IsShowing || !(screenHeightPixels > 0f))
                return;
            float metersPerPixel = 2f * _distance * Mathf.Tan(_settings.FieldOfView * 0.5f * Mathf.Deg2Rad) /
                                   screenHeightPixels;
            MovePivot(-pixelDelta * metersPerPixel);
        }

        /// <summary>Tourne autour du point visé ; un angle positif tourne la vue vers la droite.</summary>
        public void Rotate(float degrees)
        {
            if (!IsShowing)
                return;
            _yawDegrees = Mathf.Repeat(_yawDegrees + degrees, 360f);
        }

        /// <summary>Rapproche (crans positifs) ou éloigne (crans négatifs) la caméra, entre ses deux bornes.</summary>
        public void Zoom(float notches)
        {
            if (!IsShowing)
                return;
            float factor = Mathf.Pow(1f - _settings.ZoomStepPerNotch, notches);
            _targetDistance = Mathf.Clamp(_targetDistance * factor, _settings.NearDistance, _settings.FarDistance);
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
        }

        /// <summary>Déplace le point visé dans le repère de la vue (x à droite, y devant), sans quitter la carte.</summary>
        private void MovePivot(Vector2 viewMeters)
        {
            float yaw = _yawDegrees * Mathf.Deg2Rad;
            var right = new Vector2(Mathf.Cos(yaw), -Mathf.Sin(yaw));
            var forward = new Vector2(Mathf.Sin(yaw), Mathf.Cos(yaw));
            Vector2 moved = _pivot + right * viewMeters.x + forward * viewMeters.y;
            _pivot = new Vector2(Mathf.Clamp(moved.x, 0f, _relief.SizeX), Mathf.Clamp(moved.y, 0f, _relief.SizeZ));
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
