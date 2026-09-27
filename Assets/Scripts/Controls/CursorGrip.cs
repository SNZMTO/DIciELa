using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Controls
{
    /// <summary>
    /// Le curseur tenu pendant qu'on tourne à la souris (bouton du milieu, ou R tenu) : caché et verrouillé, pour que la
    /// rotation ne bute pas sur le bord de l'écran ni ne fasse défiler la vue ; rendu à sa place au relâchement
    /// (SAVOIR, 2.8). Chaque geste tient le sien ; le premier relâché rend le curseur. C'est une structure : gardée dans
    /// un champ, elle se met à jour sur place.
    /// </summary>
    public struct CursorGrip
    {
        private Vector2 _returnTo;

        /// <summary>Vrai tant que ce geste tient le curseur.</summary>
        public bool IsHeld { get; private set; }

        /// <summary>Tient le curseur ; il reviendra à ce point de l'écran.</summary>
        public void Hold(Vector2 returnTo)
        {
            if (IsHeld)
                return;
            IsHeld = true;
            _returnTo = returnTo;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>Rend le curseur, là où le geste a commencé.</summary>
        public void Release()
        {
            if (!IsHeld)
                return;
            IsHeld = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Mouse.current?.WarpCursorPosition(_returnTo);
        }
    }
}
