using System;
using System.Collections.Generic;
using Diciela.Controls;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur des options d'un outil de sol attend de sa vue (MVP ; décision 216) : les formes du mode
    /// Forme, les styles d'un tracé ou d'un polygone, les symétries et le nombre de l'étoile, chaque rangée montrée ou
    /// cachée (une liste vide la cache), ce qui est pris enfoncé ; et dire ce que le joueur choisit. Les textes arrivent
    /// déjà traduits.
    /// </summary>
    public interface IPaintOptionsView
    {
        /// <summary>Levé quand le joueur clique une forme du mode Forme.</summary>
        event Action<PaintShape> ShapeChosen;

        /// <summary>Levé quand le joueur clique un style de tracé : droit, ou en courbe.</summary>
        event Action<PaintTraceStyle> StyleChosen;

        /// <summary>Levé quand le joueur clique une symétrie.</summary>
        event Action<PaintSymmetry> SymmetryChosen;

        /// <summary>Levé quand le joueur clique un nombre de branches de l'étoile.</summary>
        event Action<int> StarChosen;

        void ShowShapes(IReadOnlyList<PaintBarChoice<PaintShape>> shapes);

        void SelectShape(PaintShape shape);

        void ShowStyles(IReadOnlyList<PaintBarChoice<PaintTraceStyle>> styles);

        void SelectStyle(PaintTraceStyle style);

        void ShowSymmetries(IReadOnlyList<PaintBarChoice<PaintSymmetry>> symmetries);

        void SelectSymmetry(PaintSymmetry symmetry);

        void ShowStars(IReadOnlyList<PaintBarChoice<int>> stars);

        void SelectStar(int branches);
    }
}
