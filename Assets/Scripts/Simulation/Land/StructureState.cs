namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Où en est une construction. Une construction posée attend d'être bâtie (GDD, section 9), sauf celle que sa fiche
    /// dit dressée dès la pose (le campement) ; déplacée, elle se défait puis se remonte (GDD, section 9). Les valeurs ne
    /// changent jamais de numéro.
    /// </summary>
    public enum StructureState
    {
        /// <summary>Un fantôme : posée, elle attend ses bâtisseurs.</summary>
        Planned = 0,

        /// <summary>Bâtie : elle sert.</summary>
        Built = 1,

        /// <summary>Défaite, ou pas encore remontée à sa nouvelle place : un déplacement en cours ; elle ne sert pas.</summary>
        Moving = 2
    }
}
