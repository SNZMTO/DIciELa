using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les places de charrette (GDD, section 11 ; décisions 197 et 204), par le chemin du joueur : des marques qu'il pose
    /// sur la Place, à la taille de la plus grande charrette (2,8 × 5,8 m par défaut), tout entières dans la zone, sans se
    /// chevaucher, deux au plus au départ ; la charrette s'y gare l'avant au bord avant de la marque, son timon (2,2 m)
    /// tenu au bout. Chacune appelle, dans l'ordre où elle a été posée, la charrette rangée la plus proche. La Place, un
    /// disque de 7 m de rayon en (20 ; 26) ; le campement, en (20 ; 14), garde sa charrette.
    /// </summary>
    public sealed class CartSpotTests
    {
        private static readonly Vector2 Center = new Vector2(20f, 26f);
        private TestTown _town;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            TestData.Carts(_town.Camp, 1);
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void ASpotIsWhereTheCartWillParkItsFrontAtTheFrontOfTheMark()
        {
            World world = _town.WorkingWorld();
            TestTown.Send(world, new PaintPlaceCommand(new GroundDisc(Center, 7f)));
            TestTown.Send(world, new AddCartSpotCommand(Center, 0f));

            Assert.AreEqual(1, world.Carts.Spots.Count, "La marque est une place de charrette.");
            CartSpot spot = world.Carts.Spots[0];
            Assert.AreEqual(Center, spot.Mark.Footprint.Center, "La marque est là où le joueur l'a posée.");
            // L'avant de la charrette au bord avant de la marque (26 + 2,9), son timon de 2,2 m tenu au bout.
            Assert.AreEqual(20f, spot.Position.x, 1e-4f);
            Assert.AreEqual(26.7f, spot.Position.y, 1e-4f, "Elle se gare à 0,7 m devant le milieu de la marque.");
            Assert.AreEqual(0f, spot.HeadingDegrees, 1e-4f, "L'avant au nord, comme la marque.");

            Cart cart = world.Carts.All[0];
            TestGrids.StepUntil(world, () => cart.State == CartState.Parked, 3000, "la charrette garée");
            Assert.AreEqual(spot.Position, cart.Position, "Garée à sa place.");
        }

        [Test]
        public void ASpotStaysWithinThePlaceAloneAndTwoAtMostAtTheStart()
        {
            World world = _town.World();
            var refused = new List<PlaceResult>();
            world.Place.PlaceRefused += refused.Add;
            TestTown.Send(world, new AddCartSpotCommand(Center, 0f));
            TestTown.Send(world, new PaintPlaceCommand(new GroundDisc(Center, 7f)));
            // Au nord, elle va jusqu'à 34,4 m : le disque s'arrête à 33 m.
            TestTown.Send(world, new AddCartSpotCommand(new Vector2(20f, 31.5f), 0f));
            TestTown.Send(world, new AddCartSpotCommand(new Vector2(18f, 26f), 0f));
            TestTown.Send(world, new AddCartSpotCommand(new Vector2(19f, 26f), 0f));
            TestTown.Send(world, new AddCartSpotCommand(new Vector2(23f, 26f), 0f));
            TestTown.Send(world, new AddCartSpotCommand(new Vector2(20f, 21f), 90f));

            CollectionAssert.AreEqual(new[]
            {
                PlaceResult.OutsideThePlace, PlaceResult.OutsideThePlace, PlaceResult.MarksOverlap,
                PlaceResult.TooManyMarks
            }, refused, "Sans Place, hors de la Place, sur une autre marque, puis une de trop.");
            Assert.AreEqual(2, world.Carts.Spots.Count, "Deux places de charrette au départ.");
        }

        [Test]
        public void ASecondWarehouseSendsItsCartToTheSecondSpot()
        {
            World world = _town.WorkingWorld();
            TraceThePlace(world);
            Cart first = world.Carts.All[0];
            TestGrids.StepUntil(world, () => first.State == CartState.Parked, 3000, "la charrette du campement garée");

            Structure built = TestTown.Place(world, Warehouse(6f, 5f), 8f, 14f);
            Assert.AreEqual(2, world.Carts.All.Count, "L'entrepôt a sa charrette.");
            Cart second = world.Carts.All[1];
            Assert.AreSame(built, second.Home);

            TestGrids.StepUntil(world, () => second.State == CartState.Parked, 3000, "la charrette de l'entrepôt garée");
            Assert.AreSame(world.Carts.Spots[1], second.Spot, "La seconde place appelle la charrette de l'entrepôt.");
            Assert.AreSame(world.Carts.Spots[0], first.Spot, "La première garde la sienne.");
            Assert.AreEqual(world.Carts.Spots[1].Position, second.Position);
        }

        [Test]
        public void TheNearestCartToASpotAnswersFirst()
        {
            // L'entrepôt, en (8 ; 26), est plus près des places que le campement, en (20 ; 14) : sa charrette répond à la
            // première place, celle du campement à la seconde.
            StructureDefinition warehouse = Warehouse(4f, 4f);
            World world = _town.WorkingWorld();
            Structure near = TestTown.Place(world, warehouse, 8f, 26f);
            TraceThePlace(world);

            TestGrids.StepUntil(world, () => world.Carts.All[0].State == CartState.Parked &&
                                             world.Carts.All[1].State == CartState.Parked, 6000, "les deux charrettes garées");
            Assert.AreSame(near, world.Carts.CartAt(world.Carts.Spots[0]).Home, "La plus proche répond la première.");
            Assert.AreSame(TestTown.CampOf(world), world.Carts.CartAt(world.Carts.Spots[1]).Home);
        }

        // La Place, un disque de 7 m de rayon en (20 ; 26), et ses deux places de charrette, côte à côte, l'avant au nord.
        private static void TraceThePlace(World world)
        {
            TestTown.Send(world, new PaintPlaceCommand(new GroundDisc(Center, 7f)));
            TestTown.Send(world, new AddCartSpotCommand(new Vector2(18f, 26f), 0f));
            TestTown.Send(world, new AddCartSpotCommand(new Vector2(23f, 26f), 0f));
        }

        // Un entrepôt de cette taille, dressé dès la pose, qui garde une charrette.
        private StructureDefinition Warehouse(float width, float depth)
        {
            StructureDefinition warehouse = _town.KeepKnown(TestData.Structure("entrepot", width, depth));
            TestData.BuiltWhenPlaced(warehouse);
            TestData.Storage(warehouse, 200);
            TestData.Carts(warehouse, 1);
            return warehouse;
        }
    }
}
