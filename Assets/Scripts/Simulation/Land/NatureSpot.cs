using System;
using Diciela.Data;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Un arbre ou un rocher tel que la carte le décrit au lancement (ARCHITECTURE, section 10) : sa sorte, sa place et son
    /// angle, et sa taille (1 : celle de sa fiche). Une donnée pure, lue sur le Terrain peint par le développeur.
    /// </summary>
    public readonly struct NatureSpot
    {
        public NatureSpot(NatureDefinition definition, Placement placement, float scale = 1f)
        {
            if (ReferenceEquals(definition, null))
                throw new ArgumentNullException(nameof(definition));
            if (!(scale > 0f))
                throw new ArgumentOutOfRangeException(nameof(scale), "Un arbre ou un rocher a une taille positive.");

            Definition = definition;
            Placement = placement;
            Scale = scale;
        }

        public NatureDefinition Definition { get; }

        /// <summary>Sa place (le pied) et son angle.</summary>
        public Placement Placement { get; }

        /// <summary>Sa taille, en fois celle de sa fiche.</summary>
        public float Scale { get; }
    }
}
