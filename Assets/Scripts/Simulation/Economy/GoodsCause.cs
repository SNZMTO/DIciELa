namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Pourquoi des marchandises sont nées ou ont disparu (ARCHITECTURE, section 3) : le bilan des marchandises égale
    /// la somme des causes. Les valeurs ne changent jamais de numéro.
    /// </summary>
    public enum GoodsCause
    {
        /// <summary>Le contenu du tas de départ, posé avec lui (GDD, section 6).</summary>
        StartingPile = 0,

        /// <summary>Les matériaux d'un chantier fini, pris dans la construction.</summary>
        Construction = 1
    }
}
