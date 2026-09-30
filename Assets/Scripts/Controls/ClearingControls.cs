using Diciela.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Les gestes de l'outil « Retirer » (ARCHITECTURE, section 11) : l'outil en main, un clic gauche sur la ville touche
    /// l'arbre ou le rocher sous le pointeur ; un glisser tire un cadre, que l'interface dessine, et le relâcher marque
    /// ce qu'il contient. Un appui qui commence sur l'interface lui appartient (SAVOIR, 2.8). Il lit les actions et
    /// appelle l'outil, rien d'autre ; il passe après la caméra (ordre 100), comme le geste « toucher ».
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class ClearingControls : MonoBehaviour
    {
        public const string MapName = "Clearing";
        public const string MarkAction = "Mark";
        public const string PointerAction = "Pointer";

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        [Tooltip("La fiche des réglages des gestes : le seuil du clic ou glisser (Assets/Data/Settings).")]
        [SerializeField] private GestureSettings _gestures;

        private ISelectionFrame _frame;
        private IInterfaceArea _interfaceArea;
        private InputActionMap _map;
        private InputAction _mark;
        private InputAction _pointer;
        private DragGesture _gesture;
        private bool _framing;
        private Vector2 _frameStart;
        private Vector2 _frameEnd;

        /// <summary>L'outil que ces gestes pilotent ; null tant qu'aucun monde n'est né.</summary>
        public ClearingTool Tool { get; private set; }

        /// <summary>
        /// Relie les gestes à l'outil « Retirer » du monde qui vient de naître, au cadre que l'interface dessine et à
        /// l'interface, sur laquelle ils se taisent ; null les rend muets.
        /// </summary>
        public void Bind(ClearingTool tool, ISelectionFrame frame, IInterfaceArea interfaceArea)
        {
            StopFraming();
            Tool = tool;
            _frame = frame;
            _interfaceArea = interfaceArea;
            _gesture = default;
        }

        private void OnEnable()
        {
            if (_actions == null || _gestures == null)
            {
                Debug.LogError("DICIELA : les gestes de l'outil « Retirer » n'ont pas leurs actions ou leur fiche.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, true);
            _mark = _map.FindAction(MarkAction, true);
            _pointer = _map.FindAction(PointerAction, true);
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_map != null)
                _map.Disable();
            _map = null;
            StopFraming();
            _gesture = default;
        }

        private void LateUpdate()
        {
            if (Tool == null || !Tool.IsActive)
            {
                StopFraming();
                _gesture = default;
                return;
            }

            Vector2 pointer = _pointer.ReadValue<Vector2>();
            bool onInterface = _interfaceArea != null && _interfaceArea.Contains(pointer);
            _gesture.Update(_mark.IsPressed(), pointer, _gestures.DragThresholdPixels, onInterface);
            if (_gesture.IsDragging)
            {
                _framing = true;
                _frameStart = _gesture.Start;
                _frameEnd = pointer;
                _frame?.ShowFrame(_frameStart, _frameEnd);
            }
            else if (_framing)
            {
                StopFraming();
                Tool.MarkWithin(_frameStart, _frameEnd);
            }
            if (_gesture.Clicked)
                Tool.TouchAt(pointer);
        }

        private void StopFraming()
        {
            if (_framing)
                _frame?.HideFrame();
            _framing = false;
        }
    }
}
