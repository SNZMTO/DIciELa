using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les places de charrette de la Place (GDD, section 11 ; décisions 194 et 197), par le chemin du joueur : elles
    /// naissent quand la Place est bâtie, sur un cercle à mi-chemin de son milieu et de son bord ; chacune appelle la
    /// charrette d'un entrepôt, la plus proche de la Place d'abord. Le banc de CartDriveTests : le campement garde sa
    /// charrette, la Place de 12 × 12 m se pose en (20 ; 26).
    /// </summary>
    public sealed class CartSpotTests
    {
        private TestTown _town;
        private StructureDefinition _place;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            TestData.Carts(_town.Camp, 1, 0);
            _place = _town.KeepKnown(TestData.Floor("place", 12f, 12f));
            TestData.Carts(_place, 0, 2);
            TestData.BuiltWhenPlaced(_place);
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void TheSpotsLieAroundTheMiddleOfThePlaceWithoutTouching()
        {
            // Cinq places, le plus qu'aura la Place à son dernier niveau (GDD, section 11). La charrette du pack, 1,6 ×
            // 3,2 m, tient dans un cercle de 1,8 m de rayon : deux garées à plus de 3,6 m l'une de l'autre ne se touchent
            // pas, quel que soit leur cap.
            TestData.Carts(_place, 0, 5);
            World world = _town.WorkingWorld();
            Structure place = TestTown.Place(world, _place, 20f, 26f);
            IReadOnlyList<CartSpot> spots = world.Carts.Spots;

            Assert.AreEqual(5, spots.Count);
            for (int i = 0; i < spots.Count; i++)
            {
                Assert.AreSame(place, spots[i].Place);
                Assert.AreEqual(i, spots[i].Index);
                Assert.AreEqual(6f * 0.55f, Vector2.Distance(spots[i].Position, place.Placement.Center), 1e-3f,
                    "Sur un cercle autour du milieu, à 55 % du demi-côté.");
                Vector2 stand = spots[i].Position + Heading.Direction(spots[i].HeadingDegrees) * 2.2f;
                Assert.Less(Vector2.Distance(stand, place.Placement.Center), 6f - 0.3f,
                    "Le charretier la gare en se tenant sur la Place.");
                for (int j = i + 1; j < spots.Count; j++)
                {
                    Assert.Greater(Vector2.Distance(spots[i].Position, spots[j].Position), 3.6f,
                        "Deux charrettes garées ne se touchent pas.");
                }
            }
        }

        [Test]
        public void ASecondWarehouseSendsItsCartToTheSecondSpot()
        {
            World world = _town.WorkingWorld();
            TestTown.Place(world, _place, 20f, 26f);
            Cart first = world.Carts.All[0];
            TestGrids.StepUntil(world, () => first.State == CartState.Parked, 3000, "la charrette du campement garée");

            StructureDefinition warehouse = _town.KeepKnown(TestData.Structure("entrepot", 6f, 5f));
            TestData.BuiltWhenPlaced(warehouse);
            TestData.Storage(warehouse, 200);
            TestData.Carts(warehouse, 1, 0);
            Structure built = TestTown.Place(world, warehouse, 8f, 14f);
            Assert.AreEqual(2, world.Carts.All.Count, "L'entrepôt a sa charrette.");
            Cart second = world.Carts.All[1];
            Assert.AreSame(built, second.Home);

            TestGrids.StepUntil(world, () => second.State == CartState.Parked, 3000, "la charrette de l'entrepôt garée");
            Assert.AreSame(world.Carts.Spots[1], second.Spot, "La seconde place appelle la charrette de l'entrepôt.");
            Assert.AreSame(world.Carts.Spots[0], first.Spot, "La première garde la sienne.");
            Assert.AreEqual(world.Carts.Spots[1].Position, second.Position);
        }

        [Test]
        public void TheNearestCartToThePlaceAnswersFirst()
        {
            // L'entrepôt, en (10 ; 26), est plus près de la Place que le campement, en (20 ; 14) : sa charrette répond à
            // la première place, celle du campement à la seconde.
            StructureDefinition warehouse = _town.KeepKnown(TestData.Structure("entrepot", 4f, 4f));
            TestData.BuiltWhenPlaced(warehouse);
            TestData.Storage(warehouse, 200);
            TestData.Carts(warehouse, 1, 0);
            World world = _town.WorkingWorld();
            Structure near = TestTown.Place(world, warehouse, 10f, 26f);
            TestTown.Place(world, _place, 20f, 26f);

            TestGrids.StepUntil(world, () => world.Carts.All[0].State == CartState.Parked &&
                                             world.Carts.All[1].State == CartState.Parked, 6000, "les deux charrettes garées");
            Assert.AreSame(near, world.Carts.CartAt(world.Carts.Spots[0]).Home, "La plus proche répond la première.");
            Assert.AreSame(TestTown.CampOf(world), world.Carts.CartAt(world.Carts.Spots[1]).Home);
        }

        [Test]
        public void ThePlaceCallsACartOnceItsSiteIsBuilt()
        {
            // Une Place qui demande 5 Bois et 30 min de travail : la Chimère porte, bâtit, puis va chercher la charrette.
            StructureDefinition plan = _town.KeepKnown(TestData.Floor("place_a_batir", 12f, 12f));
            TestData.Carts(plan, 0, 2);
            TestData.Cost(plan, 0.5f, new GoodAmount(_town.Wood, 5));
            World world = _town.WorkingWorld();
            Structure site = TestTown.Place(world, plan, 20f, 26f);
            Assert.IsEmpty(world.Carts.Spots, "Un chantier n'appelle pas de charrette.");
            Cart cart = world.Carts.All[0];

            TestGrids.StepUntil(world, () => cart.State == CartState.Parked, 6000, "la charrette garée");
            Assert.AreEqual(StructureState.Built, site.State);
            Assert.AreSame(world.Carts.Spots[0], cart.Spot);
        }
    }
}
