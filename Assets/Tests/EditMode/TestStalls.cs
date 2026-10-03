using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le banc de l'étal, sur celui de la ville au travail : le campement, en (20 ; 14), garde sa charrette ; la Place de
    /// 12 × 12 m, dressée dès la pose en (20 ; 26), a deux places de charrette ; la Chimère, au poste « à tout faire »,
    /// y gare la charrette du campement, puis garnit (porter, conduire, bâtir, garnir, défricher).
    /// </summary>
    internal static class TestStalls
    {
        /// <summary>Un monde où la charrette du campement est garée à la première place de la Place.</summary>
        public static World ParkedWorld(TestTown town, out Structure place, out Cart cart)
        {
            TestData.Carts(town.Camp, 1, 0);
            StructureDefinition plan = town.KeepKnown(TestData.Floor("place", 12f, 12f));
            TestData.Carts(plan, 0, 2);
            TestData.BuiltWhenPlaced(plan);
            World world = town.WorkingWorld();
            place = TestTown.Place(world, plan, 20f, 26f);
            Cart parked = world.Carts.All[0];
            TestGrids.StepUntil(world, () => parked.State == CartState.Parked, 3000, "la charrette garée");
            cart = parked;
            return world;
        }

        /// <summary>Envoie l'ordre « garnir » à la première place de charrette de cette Place.</summary>
        public static void Ask(World world, Structure place, GoodDefinition good, ConsigneMode mode, int number)
        {
            TestTown.Send(world, new SetConsigneCommand(place.Id, 0, new ConsigneLine(good, mode, number)));
        }
    }
}
