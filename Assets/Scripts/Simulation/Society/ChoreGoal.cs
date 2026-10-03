using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Là où mène une tâche : ce qu'on vient servir (une construction, un arbre ou un rocher), par son empreinte et ce
    /// qu'il porte sur le maillage, et l'on se tient à ses abords ; ou une maille précise (devant une place de charrette).
    /// </summary>
    internal readonly struct ChoreGoal
    {
        private ChoreGoal(RotatedRect footprint, int occupant, Vector2Int cell, bool isCell)
        {
            Footprint = footprint;
            Occupant = occupant;
            Cell = cell;
            IsCell = isCell;
        }

        public RotatedRect Footprint { get; }
        public int Occupant { get; }

        /// <summary>La maille où aller, si l'on va à une maille précise.</summary>
        public Vector2Int Cell { get; }

        /// <summary>Vrai si l'on va à une maille précise, et non aux abords de ce qu'on vient servir.</summary>
        public bool IsCell { get; }

        public static ChoreGoal Of(Structure structure)
        {
            return new ChoreGoal(PlacementRules.Footprint(structure.Definition, structure.Placement), structure.Id,
                default, false);
        }

        public static ChoreGoal Of(NatureItem item)
        {
            return new ChoreGoal(item.Footprint, item.Occupant, default, false);
        }

        public static ChoreGoal At(Vector2Int cell)
        {
            return new ChoreGoal(default, 0, cell, true);
        }
    }
}
