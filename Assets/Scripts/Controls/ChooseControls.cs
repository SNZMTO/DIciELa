using Diciela.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Le geste « toucher » (ARCHITECTURE, section 11) : un clic gauche sur la ville, quand aucun outil n'est en main,
    /// touche ce qu'on voit sous le pointeur. Un clic qui commence sur l'interface lui appartient, et un glisser n'est pas
    /// un clic (SAVOIR, 2.8). Il lit les actions et appelle l'outil « choisir », rien d'autre ; un outil en main (poser,
    /// retirer, déplacer, tracer la Place : la main le dit), le clic gauche est à lui. Il passe après la caméra (ordre
    /// 100) : il vise avec la vue de l'image qu'on va voir.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class ChooseControls : MonoBehaviour
    {
        public const string MapName = "Choose";
        public const string TouchAction = "Touch";
        public const string PointerAction = "Pointer";

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        [Tooltip("La fiche des réglages des gestes : le seuil du clic ou glisser (Assets/Data/Settings).")]
        [SerializeField] private GestureSettings _gestures;

        private Hand _hand;
        private IInterfaceArea _interfaceArea;
        private InputActionMap _map;
        private InputAction _touch;
        private InputAction _pointer;
        private DragGesture _touchGesture;

        /// <summary>L'outil que ce geste pilote ; null tant qu'aucun monde n'est né.</summary>
        public ChooseTool Tool { get; private set; }

        /// <summary>
        /// Relie le geste à l'outil « choisir » du monde qui vient de naître, à la main (un outil en main a le clic) et à
        /// l'interface, sur laquelle il se tait ; null le rend muet.
        /// </summary>
        public void Bind(ChooseTool tool, Hand hand, IInterfaceArea interfaceArea)
        {
            Tool = tool;
            _hand = hand;
            _interfaceArea = interfaceArea;
            _touchGesture = default;
        }

        private void OnEnable()
        {
            if (_actions == null || _gestures == null)
            {
                Debug.LogError("DICIELA : le geste « toucher » n'a pas ses actions ou sa fiche des gestes.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, true);
            _touch = _map.FindAction(TouchAction, true);
            _pointer = _map.FindAction(PointerAction, true);
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_map != null)
                _map.Disable();
            _map = null;
            _touchGesture = default;
        }

        private void LateUpdate()
        {
            if (Tool == null || (_hand != null && _hand.IsHolding))
            {
                // Un appui commencé un outil en main lui appartient : lâcher l'outil pendant l'appui ne touche rien.
                _touchGesture = DragGesture.HeldElsewhere(_touch.IsPressed());
                return;
            }

            Vector2 pointer = _pointer.ReadValue<Vector2>();
            bool onInterface = _interfaceArea != null && _interfaceArea.Contains(pointer);
            _touchGesture.Update(_touch.IsPressed(), pointer, _gestures.DragThresholdPixels, onInterface);
            if (_touchGesture.Clicked)
                Tool.TouchAt(pointer);
        }
    }
}
