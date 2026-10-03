using Diciela.Data;
using Diciela.Simulation.Economy;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « ne plus garnir de » : retire de la consigne de la place de charrette de ce rang, sur la Place de ce
    /// numéro, la ligne de cette marchandise ; la charrette en rapportera ce qu'elle porte (décision 202).
    /// </summary>
    public sealed class RemoveConsigneCommand : Command
    {
        public RemoveConsigneCommand(int placeId, int spotIndex, GoodDefinition good)
        {
            PlaceId = placeId;
            SpotIndex = spotIndex;
            Good = good;
        }

        public int PlaceId { get; }
        public int SpotIndex { get; }
        public GoodDefinition Good { get; }

        internal override void Execute(World world)
        {
            CartSpot spot = world.Stalls.SpotOf(PlaceId, SpotIndex);
            if (spot != null && !ReferenceEquals(Good, null))
                world.Stalls.RemoveLine(spot, Good);
        }
    }
}
