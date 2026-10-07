namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Où un ordre trace ou efface un sol (décision 210) : l'outil Chemins, partout où l'on marche ; l'outil de la Place,
    /// seulement dans sa zone. Les valeurs ne changent jamais de numéro.
    /// </summary>
    public enum FloorScope
    {
        /// <summary>Partout où l'on marche, la Place comprise : l'outil Chemins.</summary>
        Anywhere = 0,

        /// <summary>Seulement dans la zone de la Place : l'outil de la Place.</summary>
        Place = 1
    }
}
