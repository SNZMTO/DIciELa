using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Les gestes des outils (GDD, section 16 ; décisions 221 et 226) : tenir la touche de la roue l'ouvre autour du
    /// pointeur et la fait suivre où il vise, image après image ; la relâcher choisit. La touche d'un outil le choisit
    /// tout droit : chacun a la sienne, dans la table des outils (ToolAction). Les chiffres prennent les cases de la
    /// roue de l'outil en main (leurs actions s'appellent « Slot1 » à « Slot9 »). Le curseur des outils suit le
    /// pointeur, image après image. Les touches se lisent sur l'asset d'actions, d'où la roue tire aussi leurs noms. Il
    /// lit les actions et parle à la roue et au curseur, rien d'autre ; il passe après la caméra (ordre 100), comme les
    /// autres gestes.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class ToolControls : MonoBehaviour, IToolKeys
    {
        public const string MapName = "Tools";
        public const string WheelAction = "Wheel";
        public const string PointerAction = "Pointer";

        /// <summary>
        /// Le nombre de chiffres qui prennent une case : de 1 à 9 (la roue en a huit ; le 9 ne prend rien).
        /// </summary>
        public const int SlotKeys = 9;

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        private IToolWheel _wheel;
        private IToolCursor _cursor;
        private InputActionMap _map;
        private InputAction _wheelKey;
        private InputAction _pointer;
        private InputAction[] _tools;
        private Action<InputAction.CallbackContext>[] _chosen;
        private InputAction[] _slotKeys;
        private Action<InputAction.CallbackContext>[] _taken;
        private bool _held;

        /// <summary>
        /// Relie les gestes à la roue et au curseur de la ville qui vient de s'ouvrir ; null les rend muets.
        /// </summary>
        public void Bind(IToolWheel wheel, IToolCursor cursor)
        {
            _wheel = wheel;
            _cursor = cursor;
            _held = false;
        }

        /// <summary>Le nom, dans la carte « Tools », de l'action du chiffre de ce rang (0 : « Slot1 »).</summary>
        public static string SlotAction(int index)
        {
            return "Slot" + (index + 1);
        }

        public string WheelKey => Key(WheelAction);

        public string KeyOf(ToolChoice choice)
        {
            return Key(ToolAction.Of(choice).Name);
        }

        /// <summary>
        /// La touche d'une case, liée à sa place sur la rangée des chiffres : elle se lit sur le nom de cette place
        /// (« 1 »), pas sur le caractère qu'elle tape sur ce clavier (« & » en AZERTY).
        /// </summary>
        public string KeyOfSlot(int index)
        {
            InputAction action = index >= 0 && index < SlotKeys && _actions != null
                ? _actions.FindAction(MapName + "/" + SlotAction(index))
                : null;
            return action != null && action.bindings.Count > 0
                ? InputControlPath.ToHumanReadableString(action.bindings[0].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice)
                : "";
        }

        private void OnEnable()
        {
            if (_actions == null)
            {
                Debug.LogError("DICIELA : les gestes des outils n'ont pas leurs actions.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, true);
            _wheelKey = _map.FindAction(WheelAction, true);
            _pointer = _map.FindAction(PointerAction, true);
            // Un bouton lève « performed » à l'appui et « canceled » au relâchement.
            _wheelKey.performed += OnWheelPressed;
            _wheelKey.canceled += OnWheelReleased;
            _tools = new InputAction[ToolAction.All.Count];
            _chosen = new Action<InputAction.CallbackContext>[_tools.Length];
            for (int i = 0; i < _tools.Length; i++)
            {
                ToolChoice choice = ToolAction.All[i].Choice;
                _tools[i] = _map.FindAction(ToolAction.All[i].Name, true);
                _chosen[i] = _ => _wheel?.Choose(choice);
                _tools[i].performed += _chosen[i];
            }
            _slotKeys = new InputAction[SlotKeys];
            _taken = new Action<InputAction.CallbackContext>[SlotKeys];
            for (int i = 0; i < SlotKeys; i++)
            {
                int index = i;
                _slotKeys[i] = _map.FindAction(SlotAction(i), true);
                _taken[i] = _ => _wheel?.TakeSlot(index);
                _slotKeys[i].performed += _taken[i];
            }
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_map == null)
                return;
            _wheelKey.performed -= OnWheelPressed;
            _wheelKey.canceled -= OnWheelReleased;
            for (int i = 0; i < _tools.Length; i++)
                _tools[i].performed -= _chosen[i];
            for (int i = 0; i < _slotKeys.Length; i++)
                _slotKeys[i].performed -= _taken[i];
            _map.Disable();
            _map = null;
            _held = false;
        }

        private void LateUpdate()
        {
            Vector2 pointer = _pointer.ReadValue<Vector2>();
            if (_held && _wheel != null)
                _wheel.PointAt(pointer);
            _cursor?.FollowTo(pointer);
        }

        private void OnWheelPressed(InputAction.CallbackContext context)
        {
            if (_wheel == null)
                return;
            _held = true;
            _wheel.OpenAt(_pointer.ReadValue<Vector2>());
        }

        private void OnWheelReleased(InputAction.CallbackContext context)
        {
            if (!_held)
                return;
            _held = false;
            _wheel?.ReleaseAt(_pointer.ReadValue<Vector2>());
        }

        // Le nom d'une touche telle que le joueur la lit (« Tab », « B ») ; vide si l'action manque.
        private string Key(string actionName)
        {
            InputAction action = _actions != null ? _actions.FindAction(MapName + "/" + actionName) : null;
            return action != null ? action.GetBindingDisplayString() : "";
        }
    }
}
