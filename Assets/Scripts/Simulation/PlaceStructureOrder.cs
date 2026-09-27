using System;
using Diciela.Data;
using UnityEngine;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « poser » : une construction, sa place (le centre de son empreinte, en mètres sur le plan XZ) et son
    /// angle. Refusé, il ne change rien et les constructions disent pourquoi.
    /// </summary>
    public sealed class PlaceStructureOrder : IOrder
    {
        public PlaceStructureOrder(StructureDefinition definition, Vector2 center, float rotationDegrees)
        {
            if (ReferenceEquals(definition, null))
                throw new ArgumentNullException(nameof(definition));

            Definition = definition;
            Center = center;
            RotationDegrees = rotationDegrees;
        }

        public StructureDefinition Definition { get; }
        public Vector2 Center { get; }
        public float RotationDegrees { get; }

        public void Execute(World world)
        {
            world.Structures.Place(Definition, Center, RotationDegrees);
        }
    }
}
