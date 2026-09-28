using System;
using Diciela.Data;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « créer la Chimère » (GDD, section 6) : on la crée au lancement d'une partie, et elle apparaît au point
    /// d'arrivée de la carte. Le démarrage l'envoie pour un monde neuf ; l'écran de création l'enverra. Une seule
    /// Chimère par monde : un second ordre ne change rien.
    /// </summary>
    public sealed class CreateChimeraCommand : Command
    {
        public CreateChimeraCommand(VillagerKindDefinition kind)
        {
            if (ReferenceEquals(kind, null))
                throw new ArgumentNullException(nameof(kind));

            Kind = kind;
        }

        /// <summary>Sa sorte : sa marche, sa flânerie, son personnage.</summary>
        public VillagerKindDefinition Kind { get; }

        internal override void Execute(World world)
        {
            world.Villagers.CreateChimera(Kind, world.Map.ArrivalPoint);
        }
    }
}
