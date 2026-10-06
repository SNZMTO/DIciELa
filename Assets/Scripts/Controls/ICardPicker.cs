namespace Diciela.Controls
{
    /// <summary>
    /// Les cartes du panneau Construire, telles que leurs touches les prennent (GDD, section 16 ; décisions 184 et 206) :
    /// la touche 1 prend la première carte de la famille ouverte, et ainsi de suite jusqu'à 9, quand le panneau est
    /// ouvert ; sinon, l'outil de la Place en main, elles prennent ses modes. L'interface l'implémente et aiguille
    /// (CardKeys) ; les gestes ne la connaissent que par elle.
    /// </summary>
    public interface ICardPicker
    {
        /// <summary>Prend la carte de ce rang (0 : la première), ou le mode de ce rang de l'outil de la Place en main.</summary>
        void TakeCard(int index);
    }
}
