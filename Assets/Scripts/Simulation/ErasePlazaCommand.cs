using System;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « gommer la Place » (GDD, section 11 ; décision 204) : retirer de la Place ce que couvre cette forme. Il se
    /// vérifie par la même fonction que l'aperçu (PlazaZone.CheckErase) ; refusé, il ne change rien et la Place dit
    /// pourquoi.
    /// </summary>
    public sealed class ErasePlazaCommand : Command
    {
        public ErasePlazaCommand(GroundShape shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        public GroundShape Shape { get; }

        internal override void Execute(World world)
        {
            world.Plaza.Erase(Shape);
        }
    }
}
