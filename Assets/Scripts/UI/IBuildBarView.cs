using System;
using System.Collections.Generic;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur de la barre de construction attend de sa vue (MVP) : montrer des onglets et des cartes, une
    /// sélection, le bouton de l'outil « Retirer », une raison, et dire ce que le joueur choisit. Les textes arrivent
    /// déjà traduits.
    /// </summary>
    public interface IBuildBarView
    {
        /// <summary>Levé quand le joueur ouvre une famille : son rang dans la liste montrée.</summary>
        event Action<int> FamilyChosen;

        /// <summary>Levé quand le joueur choisit une construction de la famille ouverte : son rang.</summary>
        event Action<int> StructureChosen;

        /// <summary>Levé quand le joueur clique le bouton de l'outil « Retirer ».</summary>
        event Action ClearingChosen;

        /// <summary>Montre les familles en onglets, et laquelle est ouverte.</summary>
        void ShowFamilies(IReadOnlyList<BuildBarFamily> families, int open);

        /// <summary>Montre les constructions de la famille ouverte, en cartes.</summary>
        void ShowStructures(IReadOnlyList<BuildBarCard> cards);

        /// <summary>Marque la construction en main (-1 : aucune).</summary>
        void SelectStructure(int index);

        /// <summary>Montre ou cache le bouton de l'outil « Retirer », enfoncé si l'outil est en main.</summary>
        void ShowClearing(bool shown, bool held);

        /// <summary>Écrit pourquoi la pose est refusée ; un texte vide cache la raison.</summary>
        void ShowReason(string text);
    }
}
