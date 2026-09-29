using Diciela.Data;
using Diciela.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Les gestes de l'atelier (GDD, section 6) : faire tourner la Chimère à venir sur son socle, au clavier (les touches
    /// qui tournent la caméra en ville, liées à leur position : A et E en AZERTY) ou en la glissant au clic gauche, hors
    /// du panneau. Glisser vers la droite lui tourne la face vers la droite de l'écran, comme la touche E ; la caméra de
    /// l'atelier la regarde de face. Mêmes vitesses que la caméra qui tourne en ville. Il lit les actions et appelle
    /// l'aperçu ; délié, il se tait.
    /// </summary>
    public sealed class PreviewControls : MonoBehaviour
    {
        public const string MapName = "Preview";
        public const string TurnAction = "Turn";
        public const string GrabAction = "Grab";
        public const string PointerAction = "Pointer";

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        [Tooltip("La fiche des réglages de la caméra : les vitesses de rotation (Assets/Data/Settings).")]
        [SerializeField] private CameraSettings _settings;

        [Tooltip("La fiche des réglages des gestes : le seuil du clic ou glisser (Assets/Data/Settings).")]
        [SerializeField] private GestureSettings _gestures;

        private InputActionMap _map;
        private InputAction _turn;
        private InputAction _grab;
        private InputAction _pointer;
        private DragGesture _grabGesture;
        private AppearancePreview _preview;
        private IInterfaceArea _interfaceArea;

        /// <summary>
        /// Relie les gestes à l'aperçu qu'ils font tourner et au panneau qui garde ses glissers ; null les rend muets.
        /// </summary>
        public void Bind(AppearancePreview preview, IInterfaceArea interfaceArea)
        {
            _preview = preview;
            _interfaceArea = interfaceArea;
            _grabGesture = default;
        }

        private void OnEnable()
        {
            if (_actions == null || _settings == null || _gestures == null)
            {
                Debug.LogError("DICIELA : les gestes de l'atelier n'ont pas leurs actions ou leurs fiches.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, true);
            _turn = _map.FindAction(TurnAction, true);
            _grab = _map.FindAction(GrabAction, true);
            _pointer = _map.FindAction(PointerAction, true);
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_map != null)
                _map.Disable();
            _map = null;
            _grabGesture = default;
        }

        private void Update()
        {
            if (_preview == null)
                return;

            // Vue de face, la face de la Chimère va vers la droite de l'écran quand elle tourne dans le sens inverse
            // des aiguilles d'une montre, vue d'en haut : d'où le signe.
            float turn = _turn.ReadValue<float>();
            if (turn != 0f)
                _preview.Turn(-turn * _settings.RotateDegreesPerSecond * Time.unscaledDeltaTime);

            Vector2 pointer = _pointer.ReadValue<Vector2>();
            bool onInterface = _interfaceArea != null && _interfaceArea.Contains(pointer);
            _grabGesture.Update(_grab.IsPressed(), pointer, _gestures.DragThresholdPixels, onInterface);
            if (_grabGesture.IsDragging && _grabGesture.Delta.x != 0f)
                _preview.Turn(-_grabGesture.Delta.x * _settings.OrbitDegreesPerPixel);
        }
    }
}
