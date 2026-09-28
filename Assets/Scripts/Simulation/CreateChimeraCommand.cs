using System;
using Diciela.Data;
using Diciela.Simulation.Society;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « créer la Chimère » (GDD, section 6) : on la crée au lancement d'une partie, avec l'apparence choisie,
    /// et elle apparaît au point d'arrivée de la carte. Une seule Chimère par monde : un second ordre ne change rien, et
    /// le monde dit qu'il l'a refusé.
    /// </summary>
    public sealed class CreateChimeraCommand : Command
    {
        public CreateChimeraCommand(VillagerKindDefinition kind, Appearance appearance)
        {
            if (ReferenceEquals(kind, null))
                throw new ArgumentNullException(nameof(kind));
            if (ReferenceEquals(appearance, null))
                throw new ArgumentNullException(nameof(appearance));

            Kind = kind;
            Appearance = appearance;
        }

        /// <summary>Sa sorte : sa marche, sa flânerie.</summary>
        public VillagerKindDefinition Kind { get; }

        /// <summary>Son apparence : son corps, sa couleur, ses pièces.</summary>
        public Appearance Appearance { get; }

        internal override void Execute(World world)
        {
            world.Villagers.CreateChimera(Kind, Appearance, world.Map.ArrivalPoint);
        }
    }
}
