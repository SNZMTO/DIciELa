namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Pourquoi une construction se pose ou non ici. L'aperçu (le fantôme) et l'ordre lisent la même réponse, rendue par
    /// la même fonction (ARCHITECTURE, section 5). Les valeurs ne changent jamais de numéro.
    /// </summary>
    public enum PlacementResult
    {
        Ok = 0,

        /// <summary>L'empreinte ou la zone devant sa porte déborde de la carte.</summary>
        OutsideMap = 1,

        /// <summary>Le sol est trop en pente sous l'empreinte.</summary>
        TooSteep = 2,

        /// <summary>L'empreinte recouvre une autre construction.</summary>
        Overlaps = 3,

        /// <summary>L'empreinte se pose devant la porte d'une autre construction.</summary>
        BlocksADoor = 4,

        /// <summary>Une autre construction occupe déjà le devant de sa porte.</summary>
        DoorBlocked = 5
    }
}
