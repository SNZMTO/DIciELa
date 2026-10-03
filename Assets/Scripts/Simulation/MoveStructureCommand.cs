using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « déplacer » (GDD, section 9) : une construction posée, sa nouvelle place et son angle. Il se vérifie par
    /// les règles de pose, comme l'aperçu, la construction déplacée ne comptant pas ; refusé, il ne change rien et les
    /// constructions disent pourquoi.
    /// </summary>
    public sealed class MoveStructureCommand : Command
    {
        public MoveStructureCommand(int structureId, Placement placement)
        {
            StructureId = structureId;
            Placement = placement;
        }

        public int StructureId { get; }
        public Placement Placement { get; }

        internal override void Execute(World world)
        {
            // Une construction ne se retire pas : celle que vise l'ordre est toujours là.
            world.Structures.Moves.Move(world.Structures.Find(StructureId), Placement);
        }
    }
}
