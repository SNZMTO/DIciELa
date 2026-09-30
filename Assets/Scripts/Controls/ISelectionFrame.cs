using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Le cadre qu'on tire à la souris pour prendre d'un coup ce qu'il contient (ARCHITECTURE, section 11) : ses deux
    /// coins en pixels de l'écran, l'origine en bas à gauche, comme le pointeur. L'interface le dessine.
    /// </summary>
    public interface ISelectionFrame
    {
        void ShowFrame(Vector2 corner, Vector2 opposite);
        void HideFrame();
    }
}
