using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// La roue des outils, telle que ses gestes la commandent (décision 168) : touche tenue, elle s'ouvre autour du
    /// pointeur, suit où il vise, et choisit au relâchement ; la touche d'un outil le choisit tout droit. Les points sont
    /// des points de l'écran, en pixels, origine en bas à gauche. L'interface l'implémente ; les gestes ne la connaissent
    /// que par elle.
    /// </summary>
    public interface IToolWheel
    {
        void OpenAt(Vector2 screenPoint);

        void PointAt(Vector2 screenPoint);

        void ReleaseAt(Vector2 screenPoint);

        void Choose(ToolChoice choice);
    }
}
