namespace Diciela.Controls
{
    /// <summary>
    /// Ce qu'Échap peut refermer ou lâcher (GDD, section 16) : la fiche ouverte, la roue des outils, le panneau
    /// Construire, l'outil en main. Chacun dit s'il s'en
    /// est servi ; le geste « Échap » s'arrête au premier qui répond oui.
    /// </summary>
    public interface IEscapable
    {
        /// <summary>Referme ou lâche ce qui est ouvert ; faux s'il n'y avait rien.</summary>
        bool Escape();
    }
}
