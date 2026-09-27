namespace Diciela.Simulation.Core
{
    /// <summary>
    /// Les trois moments d'une journée (GDD, section 3). La journée commence avec le travail.
    /// </summary>
    public enum DayPhase
    {
        Work = 0,
        Evening = 1,
        Night = 2
    }
}
