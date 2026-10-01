namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Où en est une construction. Une construction posée attend d'être bâtie (GDD, section 9), sauf celle que sa fiche
    /// dit dressée dès la pose (le campement) ; les états suivants viennent avec les chantiers. Les valeurs ne changent
    /// jamais de numéro.
    /// </summary>
    public enum StructureState
    {
        /// <summary>Un fantôme : posée, elle attend ses bâtisseurs.</summary>
        Planned = 0,

        /// <summary>Bâtie : elle sert.</summary>
        Built = 1
    }
}
