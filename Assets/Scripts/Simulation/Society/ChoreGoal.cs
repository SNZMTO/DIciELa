using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Là où mène une tâche : ce qu'on vient servir (une construction, un arbre ou un rocher), par son empreinte et ce
    /// qu'il porte sur le maillage, et l'on se tient à ses abords ; un sol en chantier, et l'on se tient sur l'une de ses
    /// mailles, puisqu'on y marche ; ou une maille précise (devant une place de charrette). Une construction se lit là où
    /// elle est maintenant : déplacée, la tâche la suit (décision 199).
    /// </summary>
    internal readonly struct ChoreGoal
    {
        private readonly RotatedRect _footprint;

        private ChoreGoal(Structure structure, RotatedRect footprint, int occupant, Vector2Int cell, bool isCell,
            FloorPatch floor = null)
        {
            Structure = structure;
            Floor = floor;
            _footprint = footprint;
            Occupant = occupant;
            Cell = cell;
            IsCell = isCell;
        }

        /// <summary>La construction qu'on vient servir, s'il y en a une.</summary>
        public Structure Structure { get; }

        /// <summary>Le sol en chantier qu'on vient bâtir, s'il y en a un : on se tient sur ses mailles.</summary>
        public FloorPatch Floor { get; }

        /// <summary>L'empreinte de ce qu'on vient servir, là où il est maintenant.</summary>
        public RotatedRect Footprint => Structure != null
            ? PlacementRules.Footprint(Structure.Definition, Structure.Placement)
            : _footprint;

        public int Occupant { get; }

        /// <summary>La maille où aller, si l'on va à une maille précise.</summary>
        public Vector2Int Cell { get; }

        /// <summary>Vrai si l'on va à une maille précise, et non aux abords de ce qu'on vient servir.</summary>
        public bool IsCell { get; }

        public static ChoreGoal Of(Structure structure)
        {
            return new ChoreGoal(structure, default, structure.Id, default, false);
        }

        /// <summary>Le chantier qu'on vient servir : la construction qu'il bâtit, ou son sol.</summary>
        public static ChoreGoal Of(ConstructionSite site)
        {
            if (site.Structure != null)
                return Of(site.Structure);
            return new ChoreGoal(null, default, 0, default, false, site.Floor);
        }

        public static ChoreGoal Of(NatureItem item)
        {
            return new ChoreGoal(null, item.Footprint, item.Occupant, default, false);
        }

        public static ChoreGoal At(Vector2Int cell)
        {
            return new ChoreGoal(null, default, 0, cell, true);
        }
    }
}
