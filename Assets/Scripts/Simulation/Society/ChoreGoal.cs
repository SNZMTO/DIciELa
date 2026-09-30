using Diciela.Simulation.Core;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Là où mène une tâche : ce qu'on vient servir (une construction, un arbre ou un rocher), par son empreinte et ce
    /// qu'il porte sur le maillage. On se tient à ses abords.
    /// </summary>
    internal readonly struct ChoreGoal
    {
        private ChoreGoal(RotatedRect footprint, int occupant)
        {
            Footprint = footprint;
            Occupant = occupant;
        }

        public RotatedRect Footprint { get; }
        public int Occupant { get; }

        public static ChoreGoal Of(Structure structure)
        {
            return new ChoreGoal(PlacementRules.Footprint(structure.Definition, structure.Placement), structure.Id);
        }

        public static ChoreGoal Of(NatureItem item)
        {
            return new ChoreGoal(item.Footprint, item.Occupant);
        }
    }
}
