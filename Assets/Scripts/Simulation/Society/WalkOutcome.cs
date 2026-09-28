namespace Diciela.Simulation.Society
{
    /// <summary>Où en est une marche après un pas.</summary>
    internal enum WalkOutcome
    {
        /// <summary>Elle continue.</summary>
        Walking,

        /// <summary>Le marcheur est au bout de sa ligne, sur le milieu de la maille visée.</summary>
        Arrived,

        /// <summary>Le but n'est plus joignable : pris, ou enfermé. Le marcheur décide de la suite.</summary>
        Lost
    }
}
