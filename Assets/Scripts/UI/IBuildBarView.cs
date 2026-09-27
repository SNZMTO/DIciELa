using System;
using System.Collections.Generic;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur de la barre de construction attend de sa vue (MVP) : montrer des noms, une sélection, une
    /// raison, et dire ce que le joueur choisit. Les textes arrivent déjà traduits.
    /// </summary>
    public interface IBuildBarView
    {
        /// <summary>Levé quand le joueur ouvre une famille : son rang dans la liste montrée.</summary>
        event Action<int> FamilyChosen;

        /// <summary>Levé quand le joueur choisit une construction de la famille ouverte : son rang.</summary>
        event Action<int> StructureChosen;

        /// <summary>Montre les familles, et laquelle est ouverte.</summary>
        void ShowFamilies(IReadOnlyList<string> names, int open);

        /// <summary>Montre les constructions de la famille ouverte.</summary>
        void ShowStructures(IReadOnlyList<string> names);

        /// <summary>Marque la construction en main (-1 : aucune).</summary>
        void SelectStructure(int index);

        /// <summary>Écrit pourquoi la pose est refusée ; un texte vide cache la raison.</summary>
        void ShowReason(string text);
    }
}
