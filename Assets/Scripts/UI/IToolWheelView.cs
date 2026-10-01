using System;
using Diciela.Controls;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur de la roue des outils attend de sa vue (MVP) : montrer les touches, la bulle de l'outil visé
    /// et les outils actifs, et dire ce que le joueur vise et choisit. Les textes arrivent déjà traduits.
    /// </summary>
    public interface IToolWheelView
    {
        /// <summary>Levé quand le joueur choisit un outil : d'un clic, en relâchant la touche de la roue, par sa touche.</summary>
        event Action<ToolChoice> Chosen;

        /// <summary>Levé quand le joueur vise un outil, ou plus rien (null).</summary>
        event Action<ToolChoice?> Pointed;

        /// <summary>Écrit la touche de la roue, sur l'icône des outils.</summary>
        void ShowWheelKey(string key);

        /// <summary>Écrit la touche de cet outil, sur lui.</summary>
        void ShowToolKey(ToolChoice tool, string key);

        /// <summary>Écrit le nom et l'usage de l'outil visé dans la bulle du milieu ; un nom vide la cache.</summary>
        void ShowLabel(string name, string detail);

        /// <summary>Enfonce cet outil s'il est actif (le panneau Construire ouvert, l'outil « Retirer » en main).</summary>
        void ShowActive(ToolChoice tool, bool active);
    }
}
