using Diciela.Simulation.Economy;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « garnir » (GDD, section 11 ; décision 202) : dans la consigne de la place de charrette de ce rang, sur la
    /// Place de ce numéro, cette ligne remplace celle de la même marchandise, ou s'ajoute au bout. Sans cette place, ou
    /// pour une ligne sans marchandise ou d'un nombre négatif, il ne change rien.
    /// </summary>
    public sealed class SetConsigneCommand : Command
    {
        public SetConsigneCommand(int placeId, int spotIndex, ConsigneLine line)
        {
            PlaceId = placeId;
            SpotIndex = spotIndex;
            Line = line;
        }

        public int PlaceId { get; }
        public int SpotIndex { get; }
        public ConsigneLine Line { get; }

        internal override void Execute(World world)
        {
            CartSpot spot = world.Stalls.SpotOf(PlaceId, SpotIndex);
            if (spot != null)
                world.Stalls.SetLine(spot, Line);
        }
    }
}
