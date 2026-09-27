using System;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « poser » : une construction, sa place et son angle. Il se vérifie par les règles de pose, comme
    /// l'aperçu ; refusé, il ne change rien et les constructions disent pourquoi.
    /// </summary>
    public sealed class PlaceStructureCommand : Command
    {
        public PlaceStructureCommand(StructureDefinition definition, Placement placement)
        {
            if (ReferenceEquals(definition, null))
                throw new ArgumentNullException(nameof(definition));

            Definition = definition;
            Placement = placement;
        }

        public StructureDefinition Definition { get; }
        public Placement Placement { get; }

        internal override void Execute(World world)
        {
            world.Structures.Place(Definition, Placement);
        }
    }
}
