using System;
using System.Collections.Generic;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur de la fiche d'un lieu attend de sa vue (MVP) : se montrer ou se cacher, écrire son titre,
    /// son contenu, son poste et ses boutons, et dire ce que le joueur clique. Les textes arrivent déjà traduits.
    /// </summary>
    public interface IPlaceSheetView
    {
        /// <summary>Levé quand le joueur clique un bouton de la fiche : son rang dans la liste montrée.</summary>
        event Action<int> ActionClicked;

        /// <summary>Levé quand le joueur clique la croix.</summary>
        event Action CloseClicked;

        /// <summary>Montre la fiche, ou la cache.</summary>
        void ShowSheet(bool shown);

        void ShowTitle(string title);

        /// <summary>Le contenu du lieu, une ligne par marchandise ; aucune ligne cache le contenu.</summary>
        void ShowStock(IReadOnlyList<PlaceSheetLine> lines);

        /// <summary>Le poste du lieu et qui le tient ; un nom vide cache le poste.</summary>
        void ShowSlot(string name, string holder);

        /// <summary>Les boutons de la fiche, dans l'ordre ; aucun bouton les cache.</summary>
        void ShowActions(IReadOnlyList<PlaceSheetAction> actions);
    }
}
