using System;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « tracer la Place » (GDD, section 11 ; décision 204) : peindre cette forme au sol, à main levée ou toute
    /// faite. Il se vérifie par la même fonction que l'aperçu (PlazaZone.CheckPaint) ; refusé, il ne change rien et la
    /// Place dit pourquoi.
    /// </summary>
    public sealed class PaintPlazaCommand : Command
    {
        public PaintPlazaCommand(GroundShape shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        public GroundShape Shape { get; }

        internal override void Execute(World world)
        {
            world.Plaza.Paint(Shape);
        }
    }
}
