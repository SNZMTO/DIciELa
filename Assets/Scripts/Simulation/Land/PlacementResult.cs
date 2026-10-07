namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Pourquoi une construction se pose ou non ici. L'aperçu et l'ordre lisent la même réponse, rendue par
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
        DoorBlocked = 5,

        /// <summary>Elle est unique, et déjà posée : une seule dans la ville (GDD, section 9).</summary>
        AlreadyPlaced = 6,
        /// <summary>Un arbre ou un rocher est sous l'empreinte : il faut d'abord le retirer (GDD, section 2).</summary>
        NatureInTheWay = 7,

        /// <summary>La ville ne sait pas encore bâtir cette construction : il lui en faut l'idée (GDD, section 13).</summary>
        Unknown = 8,

        /// <summary>Elle se déplace déjà : on la déplace de nouveau une fois remontée (décision 200).</summary>
        Moving = 9,

        /// <summary>L'empreinte recouvre la Place : on ne bâtit pas sur elle, on la pave et la décore (décision 205).</summary>
        OnThePlace = 10,

        /// <summary>L'empreinte recouvre un sol ou son fantôme : on l'efface d'abord (décision 209).</summary>
        OnAFloor = 11
    }
}
