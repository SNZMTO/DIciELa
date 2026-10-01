namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Pourquoi des marchandises sont nées ou ont disparu (ARCHITECTURE, section 3) : le bilan des marchandises égale
    /// la somme des causes. Les valeurs ne changent jamais de numéro.
    /// </summary>
    public enum GoodsCause
    {
        /// <summary>Le stock de départ, que reçoit le campement quand on l'installe (GDD, section 6).</summary>
        StartingStock = 0,

        /// <summary>Les matériaux d'un chantier fini, pris dans la construction.</summary>
        Construction = 1,

        /// <summary>Ce que rend un arbre ou un rocher retiré (GDD, section 2).</summary>
        Clearing = 2
    }
}
