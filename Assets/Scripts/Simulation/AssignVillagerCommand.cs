namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « affecter » (GDD, sections 6 et 7) : le joueur met un habitant au premier poste libre d'un lieu de
    /// travail ; la Chimère comme les autres, elle qui ne prend jamais de poste d'elle-même. Refusé, il ne change rien,
    /// et la table des postes dit pourquoi.
    /// </summary>
    public sealed class AssignVillagerCommand : Command
    {
        public AssignVillagerCommand(int villagerId, int workplaceId)
        {
            VillagerId = villagerId;
            WorkplaceId = workplaceId;
        }

        /// <summary>Le numéro de l'habitant.</summary>
        public int VillagerId { get; }

        /// <summary>Le numéro de la construction où il va travailler.</summary>
        public int WorkplaceId { get; }

        internal override void Execute(World world)
        {
            world.WorkSlots.Assign(world.Villagers.Find(VillagerId), world.Structures.Find(WorkplaceId));
        }
    }
}
