using Diciela.Data;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Ce qui est arrivé aux constructions pendant un pas, en attendant d'être annoncé à la fin de la couche
    /// (ARCHITECTURE, section 5) : une construction posée, bâtie, déplacée, ou dont un déplacement commence ; une pose ou
    /// un déplacement refusés.
    /// </summary>
    internal readonly struct StructureHappening
    {
        public enum Kinds
        {
            Placed,
            Refused,
            Built,
            Moved,
            MoveRefused,
            MoveStarted
        }

        private StructureHappening(Kinds kind, Structure structure, StructureDefinition definition, Placement placement,
            PlacementResult result)
        {
            Kind = kind;
            Structure = structure;
            Definition = definition;
            Placement = placement;
            Result = result;
        }

        public Kinds Kind { get; }

        /// <summary>La construction concernée ; null pour une pose refusée.</summary>
        public Structure Structure { get; }

        public StructureDefinition Definition { get; }

        /// <summary>Sa place ; pour un refus, celle qui était demandée.</summary>
        public Placement Placement { get; }

        /// <summary>Pourquoi une pose ou un déplacement sont refusés.</summary>
        public PlacementResult Result { get; }

        public static StructureHappening Of(Kinds kind, Structure structure)
        {
            return new StructureHappening(kind, structure, structure.Definition, structure.Placement, PlacementResult.Ok);
        }

        public static StructureHappening Refused(StructureDefinition definition, Placement placement,
            PlacementResult result)
        {
            return new StructureHappening(Kinds.Refused, null, definition, placement, result);
        }

        public static StructureHappening MoveRefusedFor(Structure structure, Placement placement, PlacementResult result)
        {
            return new StructureHappening(Kinds.MoveRefused, structure, structure.Definition, placement, result);
        }
    }
}
