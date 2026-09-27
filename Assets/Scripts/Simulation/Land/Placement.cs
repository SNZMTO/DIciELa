using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// La place et l'angle d'une construction, qui vont toujours ensemble (comme le Placement de Timberborn) : le
    /// centre de son empreinte, en mètres sur le plan XZ (y du vecteur = z du monde), et son angle, en degrés. À
    /// l'angle 0, la construction regarde le nord (+z), où s'ouvre sa porte ; un angle positif la tourne vers l'est.
    /// </summary>
    public readonly struct Placement
    {
        public Placement(Vector2 center, float rotationDegrees)
        {
            Center = center;
            RotationDegrees = rotationDegrees;
        }

        /// <summary>Le centre de l'empreinte, en mètres sur le plan XZ.</summary>
        public Vector2 Center { get; }

        public float RotationDegrees { get; }
    }
}
