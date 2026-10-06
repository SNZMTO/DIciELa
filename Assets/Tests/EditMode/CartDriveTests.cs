using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les charrettes qui accourent (GDD, section 11 ; décisions 194, 197 et 204), par le chemin du joueur : le campement
    /// garde sa charrette ; la Place, un disque de 7 m de rayon tracé en (20 ; 26), a deux places de charrette, deux
    /// marques côte à côte, l'avant au nord ; la Chimère, au poste « à tout faire » (porter, conduire, bâtir, défricher), va
    /// prendre la charrette au campement, l'attelle, la tire au bout de son timon de 2,2 m, moitié moins vite qu'à pied, et
    /// la gare à la première place.
    /// </summary>
    public sealed class CartDriveTests
    {
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
        public void TheCampKeepsItsCartAtHome()
        {
            World world = _town.World();
            var added = new List<Cart>();
            world.Carts.CartAdded += added.Add;
            TestTown.Place(world, _town.Camp, 20f, 14f);

            Assert.AreEqual(1, added.Count, "Le campement a sa charrette.");
            Assert.AreSame(TestTown.CampOf(world), added[0].Home);
            Assert.AreEqual(CartState.AtHome, added[0].State, "Rangée chez elle : on ne la voit pas.");
            Assert.IsNull(added[0].Spot, "Aucune place ne l'appelle.");
            Assert.IsEmpty(world.Carts.Spots, "Sans marque sur la Place, pas de place de charrette.");
        }

        [Test]
        public void TheCampCartComesToTheFirstSpotAndParksThere()
        {
            World world = _town.WorkingWorld();
            var changes = new List<CartState>();
            world.Carts.CartChanged += cart => changes.Add(cart.State);
            TraceThePlace(world);
            Assert.AreEqual(2, world.Carts.Spots.Count, "Deux marques, deux places de charrette.");
            Cart cart = world.Carts.All[0];

            TestGrids.StepUntil(world, () => cart.State == CartState.Parked, 3000, "la charrette garée");
            CartSpot spot = world.Carts.Spots[0];
            Assert.AreSame(spot, cart.Spot, "La première place appelle la charrette.");
            Assert.AreSame(cart, world.Carts.CartAt(spot));
            Assert.IsNull(world.Carts.CartAt(world.Carts.Spots[1]), "Une seule charrette : la seconde place attend.");
            Assert.AreEqual(spot.Position, cart.Position, "Garée à sa place.");
            Assert.AreEqual(spot.HeadingDegrees, cart.HeadingDegrees, 1e-3f, "Tournée comme le dit sa place.");
            Assert.IsTrue(world.Place.Contains(world.Grid.CellOf(cart.Position)), "Sur la Place.");
            CollectionAssert.AreEqual(new[] { CartState.Pulled, CartState.Parked }, changes,
                "Attelée, puis garée : la vue l'apprend par ses événements.");
        }

        [Test]
        public void TheCartFollowsItsCarterAtTheEndOfItsShaft()
        {
            World world = _town.WorkingWorld();
            TraceThePlace(world);
            Cart cart = world.Carts.All[0];
            Villager chimera = world.Villagers.Chimera;

            float farthest = 0f, worstTurn = 0f;
            int pulled = 0;
            TestGrids.StepUntil(world, () =>
            {
                if (cart.State == CartState.Pulled)
                {
                    pulled++;
                    Vector2 toCarter = chimera.Position - cart.Position;
                    farthest = Mathf.Max(farthest, toCarter.magnitude);
                    worstTurn = Mathf.Max(worstTurn,
                        Mathf.Abs(Mathf.DeltaAngle(cart.HeadingDegrees, Heading.Of(toCarter))));
                }
                return cart.State == CartState.Parked;
            }, 3000, "la charrette garée");
            Assert.Greater(pulled, 50, "Elle a été tirée un bon moment.");
            Assert.AreEqual(2.2f, farthest, 0.01f, "Au bout de son timon, jamais plus loin.");
            Assert.Less(worstTurn, 0.5f, "Son avant pointe vers qui la tire.");
        }

        [Test]
        public void PullingACartHalvesTheCartersPace()
        {
            World world = _town.WorkingWorld();
            TraceThePlace(world);
            Cart cart = world.Carts.All[0];
            Villager chimera = world.Villagers.Chimera;

            float free = 0f, pulling = 0f;
            CartState before = cart.State;
            TestGrids.StepUntil(world, () =>
            {
                float moved = Vector2.Distance(chimera.PreviousPosition, chimera.Position);
                if (before == CartState.Pulled && cart.State == CartState.Pulled)
                    pulling = Mathf.Max(pulling, moved);
                else if (before == CartState.AtHome && cart.State == CartState.AtHome)
                    free = Mathf.Max(free, moved);
                before = cart.State;
                return cart.State == CartState.Parked;
            }, 3000, "la charrette garée");
            Assert.Greater(free, 0f, "Elle a marché jusqu'au campement.");
            Assert.AreEqual(free * 0.5f, pulling, 1e-4f, "Elle tire la charrette moitié moins vite qu'elle ne marche.");
        }

        [Test]
        public void StoppedOnTheWayTheCartGoesHomeAndItsSpotCallsItAgain()
        {
            World world = _town.WorkingWorld();
            TraceThePlace(world);
            Cart cart = world.Carts.All[0];
            Villager chimera = world.Villagers.Chimera;
            Structure camp = TestTown.CampOf(world);
            TestGrids.StepUntil(world, () => cart.State == CartState.Pulled, 3000, "la charrette attelée");
            for (int i = 0; i < 10; i++)
                world.Step();

            TestTown.Send(world, new UnassignVillagerCommand(chimera.Id));
            Assert.AreEqual(CartState.AtHome, cart.State, "Laissée en chemin, elle retourne au campement.");
            Assert.IsNull(cart.Spot, "Aucune place ne l'appelle plus.");
            Assert.IsNull(world.Carts.CartAt(world.Carts.Spots[0]), "Sa place est libre.");
            Assert.AreEqual(camp.Placement.Center, cart.Position);

            TestTown.Send(world, new AssignVillagerCommand(chimera.Id, camp.Id));
            TestGrids.StepUntil(world, () => cart.State == CartState.Parked, 3000, "la charrette garée");
            Assert.AreSame(world.Carts.Spots[0], cart.Spot, "Sa place l'a rappelée, et elle y est garée.");
        }

        // La Place, un disque de 7 m de rayon en (20 ; 26), et ses deux places de charrette, côte à côte, l'avant au nord.
        private static void TraceThePlace(World world)
        {
            TestTown.Send(world, new PaintPlaceCommand(new GroundDisc(new Vector2(20f, 26f), 7f)));
            TestTown.Send(world, new AddCartSpotCommand(new Vector2(18f, 26f), 0f));
            TestTown.Send(world, new AddCartSpotCommand(new Vector2(23f, 26f), 0f));
        }
    }
}
