using Diciela.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Les gestes de la pose (GDD, section 9 ; SAVOIR, 2.8) : le pointeur vise ; R bref tourne d'un cran, R tenu en
    /// bougeant la souris tourne au degré (le curseur est tenu et la visée attend) ; un clic gauche pose ; un clic droit
    /// bref pose l'outil de côté (Échap aussi, par son geste à lui). Ils pilotent de même l'outil Déplacer, quand c'est lui
    /// qui est en main (IAimingTool). Il lit les actions et appelle l'outil, rien d'autre. Il passe après la caméra
    /// (ordre 100) : la visée se fait avec la vue de l'image qu'on va voir, jamais celle d'avant.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class PlacementControls : MonoBehaviour
    {
        public const string MapName = "Placement";
        public const string PlaceAction = "Place";
        public const string RotateAction = "Rotate";
        public const string SecondaryAction = "Secondary";
        public const string PointerAction = "Pointer";
        public const string LookAction = "Look";

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        [Tooltip("La fiche des réglages de la pose : tourner au degré (Assets/Data/Settings).")]
        [SerializeField] private PlacementSettings _settings;

        [Tooltip("La fiche des réglages des gestes : le seuil du clic ou glisser (Assets/Data/Settings).")]
        [SerializeField] private GestureSettings _gestures;

        private IInterfaceArea _interfaceArea;
        private InputActionMap _map;
        private InputAction _place;
        private InputAction _rotate;
        private InputAction _secondary;
        private InputAction _pointer;
        private InputAction _look;
        private CursorGrip _rotateGrip;
        private DragGesture _placeGesture;
        private DragGesture _rotateGesture;
        private DragGesture _secondaryGesture;

        /// <summary>L'outil de pose que ces gestes pilotent ; null tant qu'aucun monde n'est né.</summary>
        public PlacementTool Tool { get; private set; }

        /// <summary>L'outil Déplacer qu'ils pilotent aussi, quand il est en main ; null s'il n'y en a pas.</summary>
        public MoveTool Mover { get; private set; }

        /// <summary>
        /// Relie les gestes aux outils du monde qui vient de naître, et à l'interface, sur laquelle ils se taisent ; null
        /// les rend muets.
        /// </summary>
        public void Bind(PlacementTool tool, IInterfaceArea interfaceArea, MoveTool mover = null)
        {
            Tool = tool;
            Mover = mover;
            _interfaceArea = interfaceArea;
            ResetGestures();
        }

        // L'outil en main que ces gestes pilotent : la main n'en tient qu'un.
        private IAimingTool Held
        {
            get
            {
                if (Tool != null && Tool.IsActive)
                    return Tool;
                return Mover != null && Mover.IsActive ? Mover : null;
            }
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
            _pointer = _map.FindAction(PointerAction, true);
            _look = _map.FindAction(LookAction, true);
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_map != null)
                _map.Disable();
            _map = null;
            ResetGestures();
        }

        private void LateUpdate()
        {
            IAimingTool held = Held;
            if (held == null)
            {
                // Un appui commencé avant de prendre une carte ne la pose pas, même relâché la carte en main.
                _rotateGrip.Release();
                _placeGesture = DragGesture.HeldElsewhere(_place.IsPressed());
                _rotateGesture = DragGesture.HeldElsewhere(_rotate.IsPressed());
                _secondaryGesture = DragGesture.HeldElsewhere(_secondary.IsPressed());
                return;
            }

            Vector2 pointer = _pointer.ReadValue<Vector2>();
            float threshold = _gestures.DragThresholdPixels;
            bool onInterface = _interfaceArea != null && _interfaceArea.Contains(pointer);

            // R : bref, un cran au relâchement ; tenu en bougeant la souris, au degré, le curseur tenu et la visée attendant.
            bool gripWasHeld = _rotateGrip.IsHeld;
            bool rotateStarts = _rotateGesture.Update(_rotate.IsPressed(), pointer, threshold, false);
            if (_rotateGesture.IsDragging)
            {
                float turn = rotateStarts ? _rotateGesture.Delta.x : _look.ReadValue<Vector2>().x;
                _rotateGrip.Hold(_rotateGesture.Start);
                held.RotateBy(turn * _settings.RotationDegreesPerPixel);
            }
            else
            {
                _rotateGrip.Release();
                if (_rotateGesture.Clicked)
                    held.RotateStep();
            }
            // Le curseur rendu n'est à sa place qu'à l'image suivante : on ne vise pas avec l'ancienne position.
            bool justReleased = gripWasHeld && !_rotateGrip.IsHeld;
            if (!_rotate.IsPressed() && !justReleased)
            {
                if (onInterface)
                    held.LoseAim();
                else
                    held.AimAt(pointer);
            }

            _placeGesture.Update(_place.IsPressed(), pointer, threshold, onInterface);
            if (_placeGesture.Clicked)
                held.Confirm();

            _secondaryGesture.Update(_secondary.IsPressed(), pointer, threshold, onInterface);
            if (_secondaryGesture.Clicked)
                held.Cancel();
        }

        private void ResetGestures()
        {
            _rotateGrip.Release();
            _placeGesture = default;
            _rotateGesture = default;
            _secondaryGesture = default;
        }
    }
}
