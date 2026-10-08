using System.Collections.Generic;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur de la carte de l'outil en main attend de sa vue (MVP ; décision 221) : se cacher, les
    /// mains vides ; sinon dire l'outil (son icône, sa couleur, son nom) et, pour un outil de sol, son mode ; ses
    /// nombres et ses réglages en puces ; la raison d'un refus, ou ce qu'un clic ferait. Les textes arrivent déjà
    /// traduits ; les couleurs sont des classes de la feuille de style.
    /// </summary>
    public interface IToolCardView
    {
        /// <summary>Cache la carte : rien n'est en main.</summary>
        void ShowNothing();

        /// <summary>Montre la carte de cet outil ; un mode vide ne se dit pas.</summary>
        void ShowTool(HudFace icon, string pastel, string name, HudFace modeIcon, string mode);

        /// <summary>Montre ces puces, dans cet ordre.</summary>
        void ShowChips(IReadOnlyList<ToolCardChip> chips);

        /// <summary>
        /// Écrit pourquoi le geste est refusé (refusal vrai) ou ce qu'un clic ferait (faux) ; un texte vide l'efface.
        /// </summary>
        void ShowReason(string text, bool refusal);
    }
}
