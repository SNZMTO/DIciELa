using Diciela.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Le geste de la prévision (GDD, section 16 ; décisions 228 et 229) : V la bascule, liée au caractère (SAVOIR,
    /// section 3) ; Échap l'éteint, en dernier dans sa pile (EscapeControls). Le démarrage lui donne la prévision de la
    /// ville à son ouverture, et la lui reprend à sa sortie ; sans elle, le geste est muet. Il lit l'action, rien
    /// d'autre.
    /// </summary>
    public sealed class ForesightControls : MonoBehaviour, IEscapable
    {
        public const string MapName = "Foresight";
        public const string ToggleAction = "Toggle";

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        private InputActionMap _map;
        private InputAction _toggle;
        private Foresight _foresight;

        /// <summary>La touche qui bascule la prévision, telle qu'une étiquette l'écrit ; vide sans action.</summary>
        public string Key => _toggle != null ? _toggle.GetBindingDisplayString() : "";

        /// <summary>Relie le geste à cette prévision ; null le rend muet.</summary>
        public void Bind(Foresight foresight)
        {
            _foresight = foresight;
        }

        /// <summary>Éteint la prévision si elle dure ; sinon, Échap passe à la suite de sa pile.</summary>
        public bool Escape()
        {
            if (_foresight == null || !_foresight.IsOn)
                return false;
            _foresight.Set(false);
            return true;
        }

        private void OnEnable()
        {
            if (_actions == null)
            {
                Debug.LogError("DICIELA : le geste de la prévision n'a pas ses actions.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, true);
            _toggle = _map.FindAction(ToggleAction, true);
            _toggle.performed += OnToggle;
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_toggle != null)
                _toggle.performed -= OnToggle;
            if (_map != null)
                _map.Disable();
            _map = null;
            _toggle = null;
        }

        private void OnToggle(InputAction.CallbackContext context)
        {
            _foresight?.Toggle();
        }
    }
}
