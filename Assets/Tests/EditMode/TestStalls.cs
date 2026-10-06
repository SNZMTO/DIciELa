using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le banc de l'étal, sur celui de la ville au travail : le campement, en (20 ; 14), garde sa charrette ; la Place,
    /// un disque de 7 m de rayon tracé en (20 ; 26), a une place de charrette en son milieu, l'avant vers l'est ; la
    /// Chimère, au poste « à tout faire », y gare la charrette du campement, puis garnit (porter, conduire, bâtir, garnir,
    /// défricher). Tout passe par les ordres du joueur.
    /// </summary>
    internal static class TestStalls
    {
        public static readonly Vector2 Center = new Vector2(20f, 26f);
        public const float Radius = 7f;
        public const float East = 90f;

        /// <summary>Un monde où la charrette du campement est garée à la place de charrette de la Place.</summary>
        public static World ParkedWorld(TestTown town, out CartSpot spot, out Cart cart)
        {
            World world = PlacedWorld(town, out spot);
            Cart parked = world.Carts.All[0];
            TestGrids.StepUntil(world, () => parked.State == CartState.Parked, 3000, "la charrette garée");
            cart = parked;
            return world;
        }

        /// <summary>Un monde où la Place est tracée et sa place de charrette posée ; la charrette n'y est pas encore.</summary>
        public static World PlacedWorld(TestTown town, out CartSpot spot)
        {
            TestData.Carts(town.Camp, 1);
            World world = town.WorkingWorld();
            TestTown.Send(world, new PaintPlaceCommand(new GroundDisc(Center, Radius)));
            TestTown.Send(world, new AddCartSpotCommand(Center, East));
            spot = world.Carts.Spots[0];
            return world;
        }

        /// <summary>Envoie l'ordre « garnir » à cette place de charrette.</summary>
        public static void Ask(World world, CartSpot spot, GoodDefinition good, ConsigneMode mode, int number)
        {
            TestTown.Send(world, new SetConsigneCommand(spot.Id, new ConsigneLine(good, mode, number)));
        }
    }
}
