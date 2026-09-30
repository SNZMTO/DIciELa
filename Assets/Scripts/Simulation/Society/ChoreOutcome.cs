namespace Diciela.Simulation.Society
{
    /// <summary>Où en est la tâche d'un habitant, après un pas.</summary>
    internal enum ChoreOutcome
    {
        /// <summary>Elle continue.</summary>
        Continuing,

        /// <summary>Elle est finie, ou arrêtée par la fin du travail.</summary>
        Done,

        /// <summary>Elle a échoué : son but n'est plus joignable.</summary>
        Failed
    }
}
