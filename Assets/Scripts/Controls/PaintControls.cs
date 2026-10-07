using Diciela.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Les gestes des outils de sol (décisions 206, 210 et 220 ; SAVOIR, 2.8), l'outil de la Place et l'outil Chemins,
    /// pour celui des deux qui est en main : le pointeur vise ; un glisser du bouton gauche trace (un trait, un disque,
    /// un rectangle), et le relâcher l'envoie ; un clic peint un rond, pose ou retire une place de charrette, aimantée
    /// à côté d'une place posée (Ctrl tenu coupe l'aimant) ; R la tourne, bref d'un cran, tenu au degré (RotateKey) ;
    /// Ctrl + molette change la taille du pinceau (la caméra laisse alors la molette) ; un clic droit bref abandonne le
    /// geste, ou pose l'outil de côté (Échap aussi, par son geste à lui). Un appui qui commence sur l'interface lui
    /// appartient. Il lit les actions et appelle l'outil, rien d'autre ; il passe après la caméra (ordre 100), comme la
    /// pose.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class PaintControls : MonoBehaviour
    {
        public const string MapName = "Place";
        public const string PaintAction = "Paint";
        public const string RotateAction = "Rotate";
        public const string SizeAction = "Size";
        public const string SecondaryAction = "Secondary";
        public const string PointerAction = "Pointer";
        public const string LookAction = "Look";
        public const string FreeAction = "Free";

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
        private InputAction _free;
        private RotateKey _rotateKey;
        private DragGesture _paintGesture;
        private DragGesture _secondaryGesture;

        private PaintTool[] _tools = System.Array.Empty<PaintTool>();

        /// <summary>L'outil en main parmi ceux que ces gestes pilotent ; null si aucun ne l'est.</summary>
        public PaintTool Tool
        {
            get
            {
                for (int i = 0; i < _tools.Length; i++)
                {
                    if (_tools[i].IsActive)
                        return _tools[i];
                }
                return null;
            }
        }

        /// <summary>
        /// Relie les gestes aux outils de sol du monde qui vient de naître, et à l'interface ; sans outil, ils sont muets.
        /// </summary>
        public void Bind(IInterfaceArea interfaceArea, params PaintTool[] tools)
        {
            _tools = tools ?? System.Array.Empty<PaintTool>();
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
            _free = _map.FindAction(FreeAction, true);
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
            PaintTool tool = Tool;
            if (tool == null)
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
                tool.RotateBy(turnPixels * _settings.RotationDegreesPerPixel);
            if (step)
                tool.RotateStep();

            float size = _size.ReadValue<float>();
            if (size != 0f && !onInterface)
                tool.ResizeBrush(size > 0f ? 1 : -1);

            // Pendant un geste, on vise même au-dessus de l'interface : le trait suit le pointeur jusqu'au relâchement.
            if (canAim)
            {
                if (onInterface && !tool.IsDrawing)
                    tool.LoseAim();
                else
                    tool.AimAt(pointer, !_free.IsPressed());
            }

            if (_paintGesture.Update(_paint.IsPressed(), pointer, threshold, onInterface))
                tool.BeginDrawing(_paintGesture.Start);
            // Le relâchement finit le trait en cours, et lâche ce que le glisser tenait (un point posé, l'épingle), même
            // sans trait.
            if (_paintGesture.Dropped || tool.IsDrawing && !_paintGesture.IsDragging)
                tool.EndDrawing();
            if (_paintGesture.Clicked)
                tool.Click();

            _secondaryGesture.Update(_secondary.IsPressed(), pointer, threshold, onInterface);
            if (_secondaryGesture.Clicked)
                tool.Cancel();
        }

        private void ResetGestures()
        {
            _rotateKey.Reset(false);
            _paintGesture = default;
            _secondaryGesture = default;
        }
    }
}
