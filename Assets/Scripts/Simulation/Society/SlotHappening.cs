namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Ce qui est arrivé à la table des postes pendant un pas, en attendant d'être annoncé à la fin de la couche
    /// (ARCHITECTURE, section 5) : un habitant qui prend un poste ou le quitte, ou un ordre refusé (sans habitant).
    /// </summary>
    internal readonly struct SlotHappening
    {
        public SlotHappening(Villager villager, WorkSlot slot, bool assigned, AssignmentRefusal refusal)
        {
            Villager = villager;
            Slot = slot;
            Assigned = assigned;
            Refusal = refusal;
        }

        public Villager Villager { get; }
        public WorkSlot Slot { get; }
        public bool Assigned { get; }
        public AssignmentRefusal Refusal { get; }
    }
}
