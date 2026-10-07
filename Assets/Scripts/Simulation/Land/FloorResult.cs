namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Pourquoi un sol se trace ou s'efface, ou non (décisions 209 et 210). L'aperçu et l'ordre lisent la même réponse,
    /// rendue par la même fonction (ARCHITECTURE, section 5). Les valeurs ne changent jamais de numéro.
    /// </summary>
    public enum FloorResult
    {
        Ok = 0,

        /// <summary>
        /// La forme ne change rien : tout ce qu'elle couvre a déjà un sol, ou quelque chose s'y tient (tracer) ; rien
        /// n'y est à effacer (gommer).
        /// </summary>
        Nothing = 1,

        /// <summary>La ville ne sait pas encore tracer ce sol : il lui en faut l'idée (GDD, section 13).</summary>
        Unknown = 2
    }
}
