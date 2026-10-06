using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Le geste « Échap », une pile (GDD, section 16 ; décision 152) : il ferme d'abord la fiche ouverte, puis la roue
    /// des outils, puis le panneau Construire, puis lâche l'outil en main (ou défait d'abord ce qu'il tient) ; plus tard,
    /// il ouvrira le menu. Le démarrage lui donne, dans cet ordre, ce qu'il peut refermer ; le premier qui s'en sert
    /// l'arrête. Il lit l'action, rien d'autre : les gestes ne connaissent pas l'interface.
    /// </summary>
    public sealed class EscapeControls : MonoBehaviour
    {
        public const string MapName = "Escape";
        public const string BackAction = "Back";

        [Tooltip("L'asset d'actions du jeu (Assets/Input).")]
        [SerializeField] private InputActionAsset _actions;

        private InputActionMap _map;
        private InputAction _back;
        private IEscapable[] _stack = Array.Empty<IEscapable>();

        /// <summary>Relie le geste à ce qu'il referme, le premier servi d'abord ; rien le rend muet.</summary>
        public void Bind(params IEscapable[] stack)
        {
            _stack = stack ?? Array.Empty<IEscapable>();
        }

        private void OnEnable()
        {
            if (_actions == null)
            {
                Debug.LogError("DICIELA : le geste « Échap » n'a pas ses actions.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, true);
            _back = _map.FindAction(BackAction, true);
            _back.performed += OnBack;
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_back != null)
                _back.performed -= OnBack;
            if (_map != null)
                _map.Disable();
            _map = null;
            _back = null;
        }

        private void OnBack(InputAction.CallbackContext context)
        {
            foreach (IEscapable escapable in _stack)
            {
                if (escapable != null && escapable.Escape())
                    return;
            }
        }
    }
}
