namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Où en est le sol d'une maille (GDD, section 9 ; décision 209) : sans sol, l'herbe ; tracé, il attend ses bâtisseurs
    /// en fantôme et n'accélère personne ; bâti, il sert ; en cours d'effacement, il sert jusqu'à la fin de son petit
    /// chantier. Les valeurs ne changent jamais de numéro.
    /// </summary>
    public enum FloorState : byte
    {
        /// <summary>Pas de sol : l'herbe.</summary>
        None = 0,

        /// <summary>Un fantôme : tracé, il attend ses bâtisseurs.</summary>
        Planned = 1,

        /// <summary>Bâti : il sert.</summary>
        Built = 2,

        /// <summary>Bâti, et en cours d'effacement : il sert jusqu'à la fin de son petit chantier.</summary>
        Erasing = 3
    }
}
