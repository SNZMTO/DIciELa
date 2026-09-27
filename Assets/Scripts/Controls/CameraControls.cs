using Diciela.Data;
using Diciela.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Les gestes de la caméra (GDD, section 16) : se déplacer (clavier, bords de l'écran, clic droit glissé qui tire le
    /// sol), tourner (clavier, ou bouton du milieu glissé, qui incline aussi), zoomer à la molette vers le pointeur. Il lit les actions et
    /// appelle la caméra ; les touches sont liées à leur position sur le clavier, pour que ZQSD tombe juste en AZERTY
    /// (SAVOIR, section 3).
    /// </summary>
    public sealed class CameraControls : MonoBehaviour
    {
        public const string MapName = "Camera";
        public const string MoveAction = "Move";
        public const string RotateAction = "Rotate";
        public const string ZoomAction = "Zoom";
        public const string DragAction = "Drag";
        public const string OrbitAction = "Orbit";
        public const string PointerAction = "Pointer";
        public const string LookAction = "Look";

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        [Tooltip("La caméra que ces gestes commandent.")]
        [SerializeField] private CameraRig _rig;

        [Tooltip("La fiche des réglages de la caméra (Assets/Data/Settings).")]
        [SerializeField] private CameraSettings _settings;

        [Tooltip("La fiche des réglages des gestes : le seuil du clic ou glisser (Assets/Data/Settings).")]
        [SerializeField] private GestureSettings _gestures;

        private InputActionMap _map;
        private InputAction _move;
        private InputAction _rotate;
        private InputAction _zoom;
        private InputAction _drag;
        private InputAction _orbit;
        private InputAction _pointer;
        private InputAction _look;
        private DragGesture _dragGesture;
        private DragGesture _orbitGesture;
        private CursorGrip _orbitGrip;
        private EdgeScroll _edgeScroll;
        private IInterfaceArea _interfaceArea;

        /// <summary>Relie les gestes à l'interface du jeu : sur elle, la ville ne défile, ne zoome ni ne se tire.</summary>
        public void Bind(IInterfaceArea interfaceArea)
        {
            _interfaceArea = interfaceArea;
        }

        private void OnEnable()
        {
            if (_actions == null || _rig == null || _settings == null || _gestures == null)
            {
                Debug.LogError("DICIELA : les gestes de la caméra n'ont pas leurs actions, leur caméra ou leurs fiches.",
                    this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, true);
            _move = _map.FindAction(MoveAction, true);
            _rotate = _map.FindAction(RotateAction, true);
            _zoom = _map.FindAction(ZoomAction, true);
            _drag = _map.FindAction(DragAction, true);
            _orbit = _map.FindAction(OrbitAction, true);
            _pointer = _map.FindAction(PointerAction, true);
            _look = _map.FindAction(LookAction, true);
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_map != null)
                _map.Disable();
            _map = null;
            _orbitGrip.Release();
            _dragGesture = default;
            _orbitGesture = default;
            _edgeScroll = default;
        }

        private void Update()
        {
            if (!_rig.IsShowing)
                return;

            float deltaSeconds = Time.unscaledDeltaTime;
            Vector2 pointer = _pointer.ReadValue<Vector2>();
            bool onInterface = _interfaceArea != null && _interfaceArea.Contains(pointer);

            // Pendant un glisser, le bord de l'écran ne fait pas défiler : on tire ou on tourne, on ne se déplace pas.
            bool holding = _drag.IsPressed() || _orbit.IsPressed();
            Vector2 edge = _edgeScroll.Update(pointer, new Vector2(Screen.width, Screen.height),
                _settings.EdgeScrollPixels, Application.isFocused);
            Vector2 move = _move.ReadValue<Vector2>() + (onInterface || holding ? Vector2.zero : edge);
            if (move != Vector2.zero)
                _rig.Pan(Vector2.ClampMagnitude(move, 1f), deltaSeconds);

            float rotate = _rotate.ReadValue<float>();
            if (rotate != 0f)
                _rig.Rotate(rotate * _settings.RotateDegreesPerSecond * deltaSeconds);

            float scroll = _zoom.ReadValue<float>();
            if (scroll != 0f && !onInterface)
                _rig.Zoom(scroll / _settings.ScrollPerNotch, pointer);

            // Clic droit glissé : on saisit le sol là où l'on a appuyé, et on le tire jusqu'au pointeur.
            float threshold = _gestures.DragThresholdPixels;
            if (_dragGesture.Update(_drag.IsPressed(), pointer, threshold, onInterface))
                _rig.BeginDrag(_dragGesture.Start);
            if (_dragGesture.IsDragging)
                _rig.DragTo(pointer);

            // Bouton du milieu glissé : la vue tourne de gauche à droite et s'incline de haut en bas. Le curseur est tenu :
            // on lit le déplacement de la souris, qui ne bute jamais sur le bord, et le curseur revient où il était.
            bool orbitStarts = _orbitGesture.Update(_orbit.IsPressed(), pointer, threshold, onInterface);
            if (_orbitGesture.IsDragging)
            {
                Vector2 turn = orbitStarts ? _orbitGesture.Delta : _look.ReadValue<Vector2>();
                _orbitGrip.Hold(_orbitGesture.Start);
                if (turn.x != 0f)
                    _rig.Rotate(turn.x * _settings.OrbitDegreesPerPixel);
                if (turn.y != 0f)
                    _rig.Tilt(turn.y * _settings.TiltDegreesPerPixel);
            }
            else
                _orbitGrip.Release();
        }
    }
}
