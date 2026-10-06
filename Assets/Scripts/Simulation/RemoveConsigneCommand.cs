using Diciela.Data;
using Diciela.Simulation.Economy;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « ne plus garnir de » : retire de la consigne de la place de charrette de ce numéro la ligne de cette
    /// marchandise ; la charrette en rapportera ce qu'elle porte (décision 202).
    /// </summary>
    public sealed class RemoveConsigneCommand : Command
    {
        public RemoveConsigneCommand(int spotId, GoodDefinition good)
        {
            SpotId = spotId;
            Good = good;
        }

        public int SpotId { get; }
        public GoodDefinition Good { get; }

        internal override void Execute(World world)
        {
            CartSpot spot = world.Carts.FindSpot(SpotId);
            if (spot != null && !ReferenceEquals(Good, null))
                world.Stalls.RemoveLine(spot, Good);
        }
    }
}
