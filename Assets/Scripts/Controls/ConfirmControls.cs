using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Le geste « valider » (ARCHITECTURE, section 11) : Entrée valide ce que l'interface propose ; pour l'instant, l'écran
    /// de création (« C'est parti »). Il lit l'action et appelle ce qu'on lui a relié, rien d'autre : les gestes ne
    /// connaissent pas l'interface, le démarrage les relie.
    /// </summary>
    public sealed class ConfirmControls : MonoBehaviour
    {
        public const string MapName = "Confirm";
        public const string ConfirmAction = "Confirm";

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        private InputActionMap _map;
        private InputAction _confirm;
        private Action _confirmed;

        /// <summary>Relie le geste à ce qu'il valide ; null le rend muet.</summary>
        public void Bind(Action confirmed)
        {
            _confirmed = confirmed;
        }

        private void OnEnable()
        {
            if (_actions == null)
            {
                Debug.LogError("DICIELA : le geste « valider » n'a pas ses actions.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, true);
            _confirm = _map.FindAction(ConfirmAction, true);
            _confirm.performed += OnConfirm;
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_confirm != null)
                _confirm.performed -= OnConfirm;
            if (_map != null)
                _map.Disable();
            _map = null;
            _confirm = null;
        }

        private void OnConfirm(InputAction.CallbackContext context)
        {
            _confirmed?.Invoke();
        }
    }
}
