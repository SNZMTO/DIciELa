using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « retirer une place de charrette » (décisions 204 et 205) : sa marque s'en va de la Place, avec sa
    /// consigne ; si elle avait appelé une charrette, garée et vide, celle-ci retourne chez elle. Mais une charrette
    /// appelée qu'on vient chercher, ou en route, ou qui porte encore des marchandises, ou vers qui l'on en porte, garde
    /// sa place : on la vide d'abord (« Sa charrette arrive, ou porte encore des marchandises »). Le refus passe par la
    /// Place, comme les autres.
    /// </summary>
    public sealed class RemoveCartSpotCommand : Command
    {
        public RemoveCartSpotCommand(int spotId)
        {
            SpotId = spotId;
        }

        public int SpotId { get; }

        internal override void Execute(World world)
        {
            CartSpot spot = world.Carts.FindSpot(SpotId);
            Cart cart = spot == null ? null : world.Carts.CartAt(spot);
            if (cart != null && (cart.State != CartState.Parked || world.Stalls.IsBusy(cart)))
            {
                world.Place.Refuse(PlaceResult.CartBusy);
                return;
            }
            world.Place.RemoveMark(SpotId);
        }
    }
}
