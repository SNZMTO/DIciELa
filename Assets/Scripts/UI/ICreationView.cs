using System;
using System.Collections.Generic;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur de l'écran de création attend de sa vue (MVP) : montrer des lignes et leur choix, et dire
    /// ce que fait le joueur. Les textes arrivent déjà traduits.
    /// </summary>
    public interface ICreationView
    {
        /// <summary>Levé quand le joueur fait défiler une ligne : son rang, et le sens (-1 à gauche, +1 à droite).</summary>
        event Action<int, int> Stepped;

        /// <summary>Levé quand le joueur valide son personnage.</summary>
        event Action Confirmed;

        /// <summary>Montre le titre, le bouton qui valide, les flèches et les lignes, par leurs noms.</summary>
        void ShowRows(string title, string confirm, string previous, string next, IReadOnlyList<string> labels);

        /// <summary>Écrit le choix d'une ligne.</summary>
        void ShowChoice(int row, string choice);
    }
}
