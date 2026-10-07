using System;
using System.Collections.Generic;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « gommer un sol » (GDD, section 9 ; décisions 209 et 210) : effacer les sols que couvre cette forme, partout
    /// (l'outil Chemins) ou dans la zone de la Place (son outil). Il se vérifie par la même fonction que l'aperçu
    /// (Floors.CheckErase) ; refusé, il ne change rien et les sols disent pourquoi. Un fantôme disparaît tout de suite :
    /// un chantier qu'il vide quitte la file. Un sol bâti entre dans un petit chantier, au bout de la file, et sert
    /// jusqu'à sa fin.
    /// </summary>
    public sealed class EraseFloorCommand : Command
    {
        private readonly List<FloorPatch> _emptied = new List<FloorPatch>();

        public EraseFloorCommand(GroundShape shape, FloorScope scope)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            Scope = scope;
        }

        public GroundShape Shape { get; }
        public FloorScope Scope { get; }

        internal override void Execute(World world)
        {
            if (!world.Floors.Erase(Shape, Scope, _emptied, out FloorPatch erasing))
                return;
            for (int i = 0; i < _emptied.Count; i++)
                world.ConstructionSites.Drop(_emptied[i]);
            if (erasing != null)
                world.ConstructionSites.TrackFloor(erasing);
        }
    }
}
