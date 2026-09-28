using Diciela.Data;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Une construction posée sur la carte : sa sorte, sa place et son angle, son état (ARCHITECTURE, section 4). C'est
    /// la vérité ; le maillage en est déduit. Posée, elle attend d'être bâtie, sauf si sa fiche la dit dressée dès la
    /// pose : la tente, que la Chimère plante d'un coup (GDD, section 9).
    /// </summary>
    public sealed class Structure
    {
        internal Structure(int id, StructureDefinition definition, Placement placement)
        {
            Id = id;
            Definition = definition;
            Placement = placement;
            State = definition.BuiltWhenPlaced ? StructureState.Built : StructureState.Planned;
        }

        /// <summary>L'identifiant de la construction dans ce monde ; jamais 0.</summary>
        public int Id { get; }

        public StructureDefinition Definition { get; }

        /// <summary>Sa place et son angle.</summary>
        public Placement Placement { get; }

        public StructureState State { get; }
    }
}
