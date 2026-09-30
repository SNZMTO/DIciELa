namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « retirer » (GDD, section 6) : le joueur retire un habitant de son poste ; il laisse sa tâche (ce qu'il
    /// porte retourne à la ville) et flâne. Refusé, il ne change rien, et la table des postes dit pourquoi.
    /// </summary>
    public sealed class UnassignVillagerCommand : Command
    {
        public UnassignVillagerCommand(int villagerId)
        {
            VillagerId = villagerId;
        }

        /// <summary>Le numéro de l'habitant.</summary>
        public int VillagerId { get; }

        internal override void Execute(World world)
        {
            world.WorkSlots.Unassign(world.Villagers.Find(VillagerId));
        }
    }
}
