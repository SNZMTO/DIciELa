namespace Diciela.Controls
{
    /// <summary>
    /// Les cartes du panneau Construire, telles que leurs touches les prennent (GDD, section 16 ; décision 184) : la
    /// touche 1 prend la première carte de la famille ouverte, et ainsi de suite jusqu'à 9, quand le panneau est ouvert.
    /// L'interface l'implémente ; les gestes ne la connaissent que par elle.
    /// </summary>
    public interface ICardPicker
    {
        /// <summary>Prend la carte de ce rang (0 : la première) dans la famille ouverte, si le panneau est ouvert.</summary>
        void TakeCard(int index);
    }
}
