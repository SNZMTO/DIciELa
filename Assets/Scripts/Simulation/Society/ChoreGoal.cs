using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Là où mène une tâche, de l'une des quatre sortes de ChoreGoalKind : ce qu'on vient servir (une construction, un
    /// arbre ou un rocher), par son empreinte et ce qu'il porte sur le maillage, et l'on se tient à ses abords ; un sol en
    /// chantier, et l'on se tient sur l'une de ses mailles, puisqu'on y marche ; ou une maille précise (devant une place
    /// de charrette). Une construction se lit là où elle est maintenant : déplacée, la tâche la suit (décision 199).
    /// </summary>
    internal readonly struct ChoreGoal
    {
        private readonly RotatedRect _footprint;

        private ChoreGoal(ChoreGoalKind kind, Structure structure = null, FloorPatch floor = null,
            RotatedRect footprint = default, int occupant = 0, Vector2Int cell = default)
        {
            Kind = kind;
            Structure = structure;
            Floor = floor;
            _footprint = footprint;
            Occupant = occupant;
            Cell = cell;
        }

        /// <summary>La sorte de but : elle dit lequel des champs suivants compte.</summary>
        public ChoreGoalKind Kind { get; }

        /// <summary>La construction qu'on vient servir (sorte Structure).</summary>
        public Structure Structure { get; }

        /// <summary>Le sol en chantier qu'on vient bâtir (sorte Floor) : on se tient sur ses mailles.</summary>
        public FloorPatch Floor { get; }

        /// <summary>L'empreinte de ce qu'on vient servir, là où il est maintenant (sortes Structure et Item).</summary>
        public RotatedRect Footprint => Kind == ChoreGoalKind.Structure
            ? PlacementRules.Footprint(Structure.Definition, Structure.Placement)
            : _footprint;

        /// <summary>Ce que l'empreinte porte sur le maillage (sortes Structure et Item).</summary>
        public int Occupant { get; }

        /// <summary>La maille où aller (sorte Cell).</summary>
        public Vector2Int Cell { get; }

        public static ChoreGoal Of(Structure structure)
        {
            return new ChoreGoal(ChoreGoalKind.Structure, structure, occupant: structure.Id);
        }

        /// <summary>Le chantier qu'on vient servir : la construction qu'il bâtit, ou son sol.</summary>
        public static ChoreGoal Of(ConstructionSite site)
        {
            return site.Structure != null
                ? Of(site.Structure)
                : new ChoreGoal(ChoreGoalKind.Floor, floor: site.Floor);
        }

        public static ChoreGoal Of(NatureItem item)
        {
            return new ChoreGoal(ChoreGoalKind.Item, footprint: item.Footprint, occupant: item.Occupant);
        }

        public static ChoreGoal At(Vector2Int cell)
        {
            return new ChoreGoal(ChoreGoalKind.Cell, cell: cell);
        }
    }
}
