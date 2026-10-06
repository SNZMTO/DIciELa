using Diciela.Simulation.Economy;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « garnir » (GDD, section 11 ; décision 202) : dans la consigne de la place de charrette de ce numéro, cette
    /// ligne remplace celle de la même marchandise, ou s'ajoute au bout. Sans cette place, ou pour une ligne sans
    /// marchandise ou d'un nombre négatif, il ne change rien.
    /// </summary>
    public sealed class SetConsigneCommand : Command
    {
        public SetConsigneCommand(int spotId, ConsigneLine line)
        {
            SpotId = spotId;
            Line = line;
        }

        public int SpotId { get; }
        public ConsigneLine Line { get; }

        internal override void Execute(World world)
        {
            CartSpot spot = world.Carts.FindSpot(SpotId);
            if (spot != null)
                world.Stalls.SetLine(spot, Line);
        }
    }
}
