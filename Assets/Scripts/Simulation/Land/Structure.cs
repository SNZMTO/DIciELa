using Diciela.Data;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Une construction posée sur la carte : sa sorte, sa place, son angle, son état (ARCHITECTURE, section 4). C'est la
    /// vérité ; le maillage en est déduit.
    /// </summary>
    public sealed class Structure
    {
        internal Structure(int id, StructureDefinition definition, Vector2 center, float rotationDegrees)
        {
            Id = id;
            Definition = definition;
            Center = center;
            RotationDegrees = rotationDegrees;
            State = StructureState.Planned;
        }

        /// <summary>L'identifiant de la construction dans ce monde ; jamais 0.</summary>
        public int Id { get; }

        public StructureDefinition Definition { get; }

        /// <summary>Le centre de l'empreinte, en mètres sur le plan XZ.</summary>
        public Vector2 Center { get; }

        public float RotationDegrees { get; }

        public StructureState State { get; }
    }
}
