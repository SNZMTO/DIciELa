using Diciela.Data;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Une construction posée sur la carte : sa sorte, sa place et son angle, son état (ARCHITECTURE, section 4). C'est
    /// la vérité ; le maillage en est déduit. Posée, elle attend d'être bâtie, sauf si sa fiche la dit dressée dès la
    /// pose : le campement, que la Chimère installe d'un coup (GDD, section 9).
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

        /// <summary>Sa place et son angle ; ils changent quand on la déplace.</summary>
        public Placement Placement { get; private set; }

        public StructureState State { get; private set; }

        /// <summary>
        /// La place où on la déplace, de l'ordre jusqu'à ce qu'elle y soit remontée ; null si on ne la déplace pas. Un
        /// chantier saute tout de suite : il n'en a jamais.
        /// </summary>
        public Placement? MovingTo { get; private set; }

        /// <summary>On la déplace vers cette place : elle sert encore, jusqu'à ce qu'on la défasse.</summary>
        internal void StartMove(Placement target)
        {
            MovingTo = target;
        }

        /// <summary>Un ouvrier commence à la défaire : elle ne sert plus, le temps du déplacement.</summary>
        internal void TakeDown()
        {
            State = StructureState.Moving;
        }

        /// <summary>Remontée à sa nouvelle place : elle sert de nouveau.</summary>
        internal void EndMove()
        {
            MovingTo = null;
            State = StructureState.Built;
        }

        /// <summary>Elle change de place. Seules les constructions l'appellent, avec son maillage.</summary>
        internal void MoveTo(Placement placement)
        {
            Placement = placement;
        }

        /// <summary>Son chantier est fini : elle sert. Seules les constructions l'appellent.</summary>
        internal void BecomeBuilt()
        {
            State = StructureState.Built;
        }
    }
}
