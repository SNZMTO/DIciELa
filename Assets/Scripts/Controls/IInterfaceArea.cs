using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// La part de l'écran que couvre l'interface (ARCHITECTURE, section 11) : un geste qui y commence lui appartient, et
    /// la ville ne le voit pas. L'interface la tient ; les gestes la lisent, sans connaître l'interface.
    /// </summary>
    public interface IInterfaceArea
    {
        /// <summary>Vrai si ce point de l'écran (en pixels, depuis le bas à gauche, comme le pointeur) est sur elle.</summary>
        bool Contains(Vector2 screenPoint);
    }
}
