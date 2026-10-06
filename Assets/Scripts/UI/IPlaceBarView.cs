using System;
using System.Collections.Generic;
using Diciela.Controls;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur de la barre de la Place attend de sa vue (MVP ; décision 206) : se montrer, montrer les
    /// modes de l'outil (leur usage au survol) et celui qui est pris, la superficie, les places de charrette, la taille
    /// du pinceau et la raison d'un refus, et dire quel mode le joueur choisit. Les textes arrivent déjà traduits.
    /// </summary>
    public interface IPlaceBarView
    {
        /// <summary>Levé quand le joueur clique un mode.</summary>
        event Action<PlaceToolMode> ModeChosen;

        /// <summary>Montre la barre, ou la cache.</summary>
        void ShowShown(bool shown);

        /// <summary>Montre les modes, dans l'ordre de PlaceToolMode : chacun son nom, son usage et sa touche.</summary>
        void ShowModes(IReadOnlyList<PlaceBarMode> modes);

        /// <summary>Enfonce le mode pris.</summary>
        void SelectMode(PlaceToolMode mode);

        /// <summary>Écrit la superficie, les places de charrette et la taille du pinceau ; une taille vide la cache.</summary>
        void ShowValues(string area, string marks, string brushSize);

        /// <summary>Écrit pourquoi le geste est refusé, ou ce qu'un clic ferait ; un texte vide la cache.</summary>
        void ShowReason(string text);
    }
}
