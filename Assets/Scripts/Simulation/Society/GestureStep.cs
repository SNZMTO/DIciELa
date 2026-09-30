namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Ce qu'un geste demande ensuite à la tâche en cours : marcher vers son nouveau but, travailler sur place, ou finir.
    /// </summary>
    internal enum GestureStep
    {
        Walk,
        Work,
        Done
    }
}
