using Diciela.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Les gestes de la pose (GDD, section 9 ; SAVOIR, 2.8) : le pointeur vise ; R bref tourne d'un cran, R tenu en
    /// bougeant la souris tourne au degré (la visée attend pendant ce temps) ; un clic gauche pose ; un clic droit bref
    /// ou Échap pose l'outil de côté. Il lit les actions et appelle l'outil, rien d'autre.
    /// </summary>
    public sealed class PlacementControls : MonoBehaviour
    {
        public const string MapName = "Placement";
        public const string PlaceAction = "Place";
        public const string RotateAction = "Rotate";
        public const string SecondaryAction = "Secondary";
        public const string CancelAction = "Cancel";
        public const string PointerAction = "Pointer";

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        [Tooltip("La fiche des réglages de la pose : tourner au degré (Assets/Data/Settings).")]
        [SerializeField] private PlacementSettings _settings;

        [Tooltip("La fiche des réglages des gestes : le seuil du clic ou glisser (Assets/Data/Settings).")]
        [SerializeField] private GestureSettings _gestures;

        private PlacementTool _tool;
        private InputActionMap _map;
        private InputAction _place;
        private InputAction _rotate;
        private InputAction _secondary;
        private InputAction _cancel;
        private InputAction _pointer;
        private DragGesture _placeGesture;
        private DragGesture _rotateGesture;
        private DragGesture _secondaryGesture;

        /// <summary>Relie les gestes à l'outil du monde qui vient de naître ; null les rend muets.</summary>
        public void Bind(PlacementTool tool)
        {
            _tool = tool;
            ResetGestures();
        }

        private void OnEnable()
        {
            if (_actions == null || _settings == null || _gestures == null)
            {
                Debug.LogError("DICIELA : les gestes de la pose n'ont pas leurs actions ou leurs fiches.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, true);
            _place = _map.FindAction(PlaceAction, true);
            _rotate = _map.FindAction(RotateAction, true);
            _secondary = _map.FindAction(SecondaryAction, true);
            _cancel = _map.FindAction(CancelAction, true);
            _pointer = _map.FindAction(PointerAction, true);
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_map != null)
                _map.Disable();
            _map = null;
            ResetGestures();
        }

        private void Update()
        {
            if (_tool == null || !_tool.IsActive)
            {
                ResetGestures();
                return;
            }

            Vector2 pointer = _pointer.ReadValue<Vector2>();
            float threshold = _gestures.DragThresholdPixels;

            // R : bref, un cran au relâchement ; tenu en bougeant la souris, au degré, la visée attendant.
            _rotateGesture.Update(_rotate.IsPressed(), pointer, threshold);
            if (_rotateGesture.IsDragging)
                _tool.RotateBy(_rotateGesture.Delta.x * _settings.RotationDegreesPerPixel);
            else if (_rotateGesture.Clicked)
                _tool.RotateStep();
            if (!_rotate.IsPressed())
                _tool.AimAt(pointer);

            _placeGesture.Update(_place.IsPressed(), pointer, threshold);
            if (_placeGesture.Clicked)
                _tool.Confirm();

            _secondaryGesture.Update(_secondary.IsPressed(), pointer, threshold);
            if (_secondaryGesture.Clicked || _cancel.WasPressedThisFrame())
                _tool.Cancel();
        }

        private void ResetGestures()
        {
            _placeGesture = default;
            _rotateGesture = default;
            _secondaryGesture = default;
        }
    }
}
