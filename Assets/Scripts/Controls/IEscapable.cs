namespace Diciela.Controls
{
    /// <summary>
    /// Ce qu'Échap peut refermer, lâcher ou éteindre (GDD, section 16) : la fiche ouverte, la roue des outils, l'outil
    /// en main, la prévision. Chacun dit s'il s'en est servi ; le geste « Échap » s'arrête au premier qui répond oui.
    /// </summary>
    public interface IEscapable
    {
        /// <summary>Referme ou lâche ce qui est ouvert ; faux s'il n'y avait rien.</summary>
        bool Escape();
    }
}
