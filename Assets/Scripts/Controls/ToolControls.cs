using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Les gestes des outils (GDD, section 16 ; décisions 168 et 184) : tenir la touche de la roue l'ouvre autour du
    /// pointeur et la fait suivre où il vise, image après image ; la relâcher choisit. La touche d'un outil le choisit
    /// tout droit : chacun a la sienne, dans la table des outils (ToolAction). Les touches 1 à 9 prennent les cartes du
    /// panneau Construire ouvert. Les touches se lisent sur l'asset d'actions, d'où la roue et les cartes tirent aussi
    /// leurs noms. Il lit les actions et parle à la roue et aux cartes, rien d'autre ; il passe après la caméra (ordre
    /// 100), comme les autres gestes.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class ToolControls : MonoBehaviour, IToolKeys
    {
        public const string MapName = "Tools";
        public const string WheelAction = "Wheel";
        public const string PointerAction = "Pointer";

        /// <summary>Le nombre de cartes qu'une touche prend : de 1 à 9.</summary>
        public const int CardKeys = 9;

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        private IToolWheel _wheel;
        private ICardPicker _cards;
        private InputActionMap _map;
        private InputAction _wheelKey;
        private InputAction _pointer;
        private InputAction[] _tools;
        private Action<InputAction.CallbackContext>[] _chosen;
        private InputAction[] _cardKeys;
        private Action<InputAction.CallbackContext>[] _taken;
        private bool _held;

        /// <summary>Relie les gestes à la roue et aux cartes de la ville qui vient de s'ouvrir ; null les rend muets.</summary>
        public void Bind(IToolWheel wheel, ICardPicker cards)
        {
            _wheel = wheel;
            _cards = cards;
            _held = false;
        }

        /// <summary>Le nom, dans la carte « Tools », de l'action qui prend la carte de ce rang (0 : « Card1 »).</summary>
        public static string CardAction(int index)
        {
            return "Card" + (index + 1);
        }

        public string WheelKey => Key(WheelAction);

        public string KeyOf(ToolChoice choice)
        {
            return Key(ToolAction.Of(choice).Name);
        }

        /// <summary>
        /// La touche d'une carte, liée à sa place sur la rangée des chiffres : elle se lit sur le nom de cette place
        /// (« 1 »), pas sur le caractère qu'elle tape sur ce clavier (« & » en AZERTY).
        /// </summary>
        public string KeyOfCard(int index)
        {
            InputAction action = index >= 0 && index < CardKeys && _actions != null
                ? _actions.FindAction(MapName + "/" + CardAction(index))
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
            _cardKeys = new InputAction[CardKeys];
            _taken = new Action<InputAction.CallbackContext>[CardKeys];
            for (int i = 0; i < CardKeys; i++)
            {
                int index = i;
                _cardKeys[i] = _map.FindAction(CardAction(i), true);
                _taken[i] = _ => _cards?.TakeCard(index);
                _cardKeys[i].performed += _taken[i];
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
            for (int i = 0; i < _cardKeys.Length; i++)
                _cardKeys[i].performed -= _taken[i];
            _map.Disable();
            _map = null;
            _held = false;
        }

        private void LateUpdate()
        {
            if (_held && _wheel != null)
                _wheel.PointAt(_pointer.ReadValue<Vector2>());
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
