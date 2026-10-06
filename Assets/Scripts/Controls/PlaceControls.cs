using Diciela.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Les gestes de l'outil de la Place (décision 206 ; SAVOIR, 2.8) : le pointeur vise ; un glisser du bouton gauche
    /// trace (un trait, un disque, un rectangle), et le relâcher l'envoie ; un clic peint un rond, pose ou retire une
    /// place de charrette ; R la tourne, bref d'un cran, tenu au degré (RotateKey) ; Ctrl + molette change la taille du
    /// pinceau (la caméra laisse alors la molette) ; un clic droit bref abandonne le geste, ou pose l'outil de côté (Échap
    /// aussi, par son geste à lui). Un appui qui commence sur l'interface lui appartient. Il lit les actions et appelle
    /// l'outil, rien d'autre ; il passe après la caméra (ordre 100), comme la pose.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class PlaceControls : MonoBehaviour
    {
        public const string MapName = "Place";
        public const string PaintAction = "Paint";
        public const string RotateAction = "Rotate";
        public const string SizeAction = "Size";
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
        private InputAction _paint;
        private InputAction _rotate;
        private InputAction _size;
        private InputAction _secondary;
        private InputAction _pointer;
        private InputAction _look;
        private RotateKey _rotateKey;
        private DragGesture _paintGesture;
        private DragGesture _secondaryGesture;

        /// <summary>L'outil que ces gestes pilotent ; null tant qu'aucun monde n'est né.</summary>
        public PlaceTool Tool { get; private set; }

        /// <summary>Relie les gestes à l'outil de la Place du monde qui vient de naître, et à l'interface ; null les rend muets.</summary>
        public void Bind(PlaceTool tool, IInterfaceArea interfaceArea)
        {
            Tool = tool;
            _interfaceArea = interfaceArea;
            ResetGestures();
        }

        private void OnEnable()
        {
            if (_actions == null || _settings == null || _gestures == null)
            {
                Debug.LogError("DICIELA : les gestes de la Place n'ont pas leurs actions ou leurs fiches.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, true);
            _paint = _map.FindAction(PaintAction, true);
            _rotate = _map.FindAction(RotateAction, true);
            _size = _map.FindAction(SizeAction, true);
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
            if (Tool == null || !Tool.IsActive)
            {
                // Un appui commencé avant de prendre l'outil ne trace rien, même relâché l'outil en main.
                _rotateKey.Reset(_rotate.IsPressed());
                _paintGesture = DragGesture.HeldElsewhere(_paint.IsPressed());
                _secondaryGesture = DragGesture.HeldElsewhere(_secondary.IsPressed());
                return;
            }

            Vector2 pointer = _pointer.ReadValue<Vector2>();
            float threshold = _gestures.DragThresholdPixels;
            bool onInterface = _interfaceArea != null && _interfaceArea.Contains(pointer);

            bool canAim = _rotateKey.Update(_rotate.IsPressed(), pointer, _look.ReadValue<Vector2>(), threshold,
                out float turnPixels, out bool step);
            if (turnPixels != 0f)
                Tool.RotateBy(turnPixels * _settings.RotationDegreesPerPixel);
            if (step)
                Tool.RotateStep();

            float size = _size.ReadValue<float>();
            if (size != 0f && !onInterface)
                Tool.ResizeBrush(size > 0f ? 1 : -1);

            // Pendant un geste, on vise même au-dessus de l'interface : le trait suit le pointeur jusqu'au relâchement.
            if (canAim)
            {
                if (onInterface && !Tool.IsDrawing)
                    Tool.LoseAim();
                else
                    Tool.AimAt(pointer);
            }

            if (_paintGesture.Update(_paint.IsPressed(), pointer, threshold, onInterface))
                Tool.BeginDrawing(_paintGesture.Start);
            if (Tool.IsDrawing && !_paintGesture.IsDragging)
                Tool.EndDrawing();
            if (_paintGesture.Clicked)
                Tool.Click();

            _secondaryGesture.Update(_secondary.IsPressed(), pointer, threshold, onInterface);
            if (_secondaryGesture.Clicked)
                Tool.Cancel();
        }

        private void ResetGestures()
        {
            _rotateKey.Reset(false);
            _paintGesture = default;
            _secondaryGesture = default;
        }
    }
}
