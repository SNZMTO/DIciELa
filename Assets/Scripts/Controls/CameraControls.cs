using Diciela.Data;
using Diciela.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Les gestes de la caméra (GDD, section 16) : se déplacer (clavier, bords de l'écran, glisser au clic droit),
    /// tourner, zoomer à la molette. Il lit les actions et appelle la caméra ; les touches sont liées à leur position
    /// sur le clavier, pour que ZQSD tombe juste en AZERTY (SAVOIR, section 3).
    /// </summary>
    public sealed class CameraControls : MonoBehaviour
    {
        public const string MapName = "Camera";
        public const string MoveAction = "Move";
        public const string RotateAction = "Rotate";
        public const string ZoomAction = "Zoom";
        public const string DragAction = "Drag";
        public const string PointerAction = "Pointer";

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        [Tooltip("La caméra que ces gestes commandent.")]
        [SerializeField] private CameraRig _rig;

        [Tooltip("La fiche des réglages de la caméra (Assets/Data/Settings).")]
        [SerializeField] private CameraSettings _settings;

        private InputActionMap _map;
        private InputAction _move;
        private InputAction _rotate;
        private InputAction _zoom;
        private InputAction _drag;
        private InputAction _pointer;
        private Vector2 _pressStart;
        private bool _pressed;
        private bool _dragging;

        private void OnEnable()
        {
            if (_actions == null || _rig == null || _settings == null)
            {
                Debug.LogError("DICIELA : les gestes de la caméra n'ont pas leurs actions, leur caméra ou leur fiche.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, true);
            _move = _map.FindAction(MoveAction, true);
            _rotate = _map.FindAction(RotateAction, true);
            _zoom = _map.FindAction(ZoomAction, true);
            _drag = _map.FindAction(DragAction, true);
            _pointer = _map.FindAction(PointerAction, true);
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_map != null)
                _map.Disable();
            _map = null;
            _pressed = false;
            _dragging = false;
        }

        private void Update()
        {
            if (!_rig.IsShowing)
                return;

            float deltaSeconds = Time.unscaledDeltaTime;
            Vector2 pointer = _pointer.ReadValue<Vector2>();

            Vector2 move = _move.ReadValue<Vector2>() + EdgeDirection(pointer);
            if (move != Vector2.zero)
                _rig.Pan(Vector2.ClampMagnitude(move, 1f), deltaSeconds);

            float rotate = _rotate.ReadValue<float>();
            if (rotate != 0f)
                _rig.Rotate(rotate * _settings.RotateDegreesPerSecond * deltaSeconds);

            float scroll = _zoom.ReadValue<float>();
            if (scroll != 0f)
                _rig.Zoom(scroll / _settings.ScrollPerNotch);

            UpdateDrag(pointer);
        }

        /// <summary>
        /// Le glisser ne commence qu'au-delà d'un seuil en pixels : un clic droit bref reste un clic (SAVOIR, 2.8). Il
        /// saisit alors le sol là où l'on a appuyé, et le tire jusqu'au pointeur.
        /// </summary>
        private void UpdateDrag(Vector2 pointer)
        {
            if (!_drag.IsPressed())
            {
                _pressed = false;
                _dragging = false;
                return;
            }

            // La première image où l'on voit le bouton enfoncé, même s'il l'était avant que la caméra montre la carte.
            if (!_pressed)
            {
                _pressed = true;
                _pressStart = pointer;
                return;
            }

            if (!_dragging && (pointer - _pressStart).magnitude >= _settings.DragThresholdPixels)
                _dragging = _rig.BeginDrag(_pressStart);
            if (_dragging)
                _rig.DragTo(pointer);
        }

        /// <summary>La direction de défilement quand le pointeur touche un bord de la fenêtre, si elle a le focus.</summary>
        private Vector2 EdgeDirection(Vector2 pointer)
        {
            float margin = _settings.EdgeScrollPixels;
            if (!(margin > 0f) || !Application.isFocused)
                return Vector2.zero;
            if (pointer.x < 0f || pointer.y < 0f || pointer.x > Screen.width || pointer.y > Screen.height)
                return Vector2.zero;

            var direction = Vector2.zero;
            if (pointer.x <= margin)
                direction.x -= 1f;
            else if (pointer.x >= Screen.width - margin)
                direction.x += 1f;
            if (pointer.y <= margin)
                direction.y -= 1f;
            else if (pointer.y >= Screen.height - margin)
                direction.y += 1f;
            return direction;
        }
    }
}
