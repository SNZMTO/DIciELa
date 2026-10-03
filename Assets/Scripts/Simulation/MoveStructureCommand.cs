using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « déplacer » (GDD, section 9) : une construction posée, sa nouvelle place et son angle. Il se vérifie par
    /// les règles de pose, comme l'aperçu, la construction déplacée ne comptant pas ; refusé, il ne change rien et les
    /// constructions disent pourquoi. Un chantier saute tout de suite ; un bâtiment retient sa nouvelle place, et
    /// l'économie ouvre son petit chantier (décision 199).
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
            Structure structure = world.Structures.Find(StructureId);
            if (world.Structures.Moves.Move(structure, Placement) && structure.MovingTo.HasValue)
                world.ConstructionSites.TrackMove(structure);
        }
    }
}
