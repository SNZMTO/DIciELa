namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Une façon de se déplacer (ARCHITECTURE, section 8) : chacune paie son prix pour entrer dans une maille, et la
    /// recherche des trajets cherche le moins cher pour elle. Les valeurs ne changent jamais de numéro.
    /// </summary>
    public enum Mover
    {
        /// <summary>À pied : le même prix partout où l'on passe (GDD, section 9).</summary>
        OnFoot = 0,

        /// <summary>En tirant une charrette : l'inverse de son pas, plus cher dans l'herbe que sur un sol (CartPace).</summary>
        Cart = 1
    }
}
