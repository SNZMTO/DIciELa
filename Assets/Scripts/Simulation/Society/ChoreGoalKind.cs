namespace Diciela.Simulation.Society
{
    /// <summary>Ce vers quoi mène une tâche, et donc où l'on se tient pour la faire.</summary>
    internal enum ChoreGoalKind
    {
        /// <summary>Une construction : à ses abords, là où elle est maintenant.</summary>
        Structure,

        /// <summary>Un arbre ou un rocher : à ses abords.</summary>
        Item,

        /// <summary>Un sol en chantier : sur l'une de ses mailles, puisqu'on y marche.</summary>
        Floor,

        /// <summary>Une maille précise (devant une place de charrette).</summary>
        Cell
    }
}
