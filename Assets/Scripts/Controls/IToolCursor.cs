using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Le curseur des outils, tel que les gestes le mènent (décision 221) : il suit le pointeur, image après image. Le
    /// point est un point de l'écran, en pixels, origine en bas à gauche. L'interface l'implémente ; les gestes ne le
    /// connaissent que par lui.
    /// </summary>
    public interface IToolCursor
    {
        void FollowTo(Vector2 screenPoint);
    }
}
