using System;
using System.Collections.Generic;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur de l'écran de création attend de sa vue (MVP) : montrer des lignes, la pièce et la couleur
    /// de chacune, et dire ce que fait le joueur. Les textes arrivent déjà traduits.
    /// </summary>
    public interface ICreationView
    {
        /// <summary>Levé quand le joueur change la pièce d'une ligne : son rang, et le sens (-1 à gauche, +1 à droite).</summary>
        event Action<int, int> Stepped;

        /// <summary>Levé quand le joueur change la couleur d'une ligne : son rang, et le sens.</summary>
        event Action<int, int> Recolored;

        /// <summary>Levé quand le joueur valide son personnage.</summary>
        event Action Confirmed;

        /// <summary>
        /// Montre le titre, le bouton qui valide et les lignes, par leurs noms ; l'identifiant de chaque ligne choisit son
        /// icône.
        /// </summary>
        void ShowRows(string title, string confirm, IReadOnlyList<string> labels, IReadOnlyList<string> rowIds);

        /// <summary>Écrit la pièce et la couleur d'une ligne ; une couleur vide cache ses flèches.</summary>
        void ShowChoice(int row, string choice, string color);
    }
}
