using System;
using System.Collections.Generic;
using Diciela.Controls;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur de la barre des outils de sol attend de sa vue (MVP ; décisions 206 et 210) : se montrer,
    /// montrer ce que l'outil en main peut poser (la zone, les sols) et ce qu'il pose, ses modes (leur usage au survol) et
    /// celui qui est pris, la superficie, les places de charrette, la taille du pinceau et la raison d'un refus, et dire
    /// ce que le joueur choisit. Les textes arrivent déjà traduits.
    /// </summary>
    public interface IPaintBarView
    {
        /// <summary>Levé quand le joueur clique un mode.</summary>
        event Action<PaintToolMode> ModeChosen;

        /// <summary>Levé quand le joueur clique ce qu'il veut poser : la zone, ou un sol.</summary>
        event Action<PaintInk> InkChosen;

        /// <summary>Montre la barre, ou la cache.</summary>
        void ShowShown(bool shown);

        /// <summary>Montre ce que l'outil peut poser, dans cet ordre : chacun son nom, son usage et sa vignette.</summary>
        void ShowInks(IReadOnlyList<PaintBarInk> inks);

        /// <summary>Enfonce ce que l'outil pose ; aucune vignette s'il ne pose rien.</summary>
        void SelectInk(PaintInk ink);

        /// <summary>Montre les modes, dans l'ordre de l'outil : chacun son nom, son usage et sa touche.</summary>
        void ShowModes(IReadOnlyList<PaintBarMode> modes);

        /// <summary>Enfonce le mode pris.</summary>
        void SelectMode(PaintToolMode mode);

        /// <summary>
        /// Écrit la superficie, les places de charrette et la taille du pinceau ; un texte vide cache son creux.
        /// </summary>
        void ShowValues(string area, string marks, string brushSize);

        /// <summary>Écrit pourquoi le geste est refusé, ou ce qu'un clic ferait ; un texte vide la cache.</summary>
        void ShowReason(string text);
    }
}
