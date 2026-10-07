using System;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « tracer un sol » (GDD, section 9 ; décisions 209 et 210) : ce sol, dans cette forme, partout où l'on marche
    /// (l'outil Chemins) ou dans la zone de la Place (son outil). Il se vérifie par la même fonction que l'aperçu
    /// (Floors.CheckTrace) ; refusé, il ne change rien et les sols disent pourquoi. Tracé, le sol attend ses bâtisseurs
    /// en fantôme : l'économie ouvre son chantier, au bout de la file.
    /// </summary>
    public sealed class TraceFloorCommand : Command
    {
        public TraceFloorCommand(GroundShape shape, FloorDefinition floor, FloorScope scope)
        {
            if (ReferenceEquals(floor, null))
                throw new ArgumentNullException(nameof(floor));

            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            Floor = floor;
            Scope = scope;
        }

        public GroundShape Shape { get; }
        public FloorDefinition Floor { get; }
        public FloorScope Scope { get; }

        internal override void Execute(World world)
        {
            FloorPatch patch = world.Floors.Trace(Shape, Floor, Scope);
            if (patch != null)
                world.ConstructionSites.TrackFloor(patch);
        }
    }
}
