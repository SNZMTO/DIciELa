namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Pourquoi un ordre « dormir » ou « réveiller » la Chimère est refusé (GDD, section 6). Refusé, il ne change rien.
    /// Les valeurs ne changent jamais de numéro.
    /// </summary>
    public enum SleepRefusal
    {
        /// <summary>Le monde n'a pas encore de Chimère.</summary>
        NoChimera = 0,

        /// <summary>Elle n'a pas de logis bâti où dormir.</summary>
        NoHome = 1,

        /// <summary>Elle dort déjà, ou y va.</summary>
        AlreadyInBed = 2,

        /// <summary>Elle ne peut pas rejoindre la porte de son logis d'où elle est.</summary>
        Unreachable = 3,

        /// <summary>Elle est debout : il n'y a personne à réveiller.</summary>
        NotAsleep = 4
    }
}
