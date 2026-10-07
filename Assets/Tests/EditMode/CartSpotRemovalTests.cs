using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Retirer une place de charrette (décisions 204 et 205), par l'ordre du joueur : sa marque s'en va de la Place, avec
    /// sa consigne ; garée et vide, sa charrette retourne chez elle. Mais une charrette appelée qu'on vient chercher, en
    /// route, ou qui porte des marchandises, garde sa place : « Sa charrette arrive, ou porte encore des marchandises ».
    /// Sans quoi une conduite en cours garerait une charrette sans place (décision 205). Le banc de l'étal
    /// (TestStalls) : une place de charrette au milieu de la Place, la charrette du campement.
    /// </summary>
    public sealed class CartSpotRemovalTests
    {
        private TestTown _town;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void ThePlaceOfAParkedEmptyCartGoesAndTheCartGoesHome()
        {
            World world = TestStalls.ParkedWorld(_town, out CartSpot spot, out Cart cart);
            TestTown.Send(world, new RemoveCartSpotCommand(spot.Id));

            Assert.IsEmpty(world.Carts.Spots, "La place de charrette s'en est allée.");
            Assert.IsEmpty(world.Plaza.Marks, "Et sa marque avec elle.");
            Assert.AreEqual(CartState.AtHome, cart.State, "Sa charrette est rentrée au campement.");
            Assert.IsNull(cart.Spot, "Plus rien ne l'appelle.");
        }

        [Test]
        public void ACalledCartKeepsItsPlaceWhileSomeoneFetchesIt()
        {
            World world = TestStalls.PlacedWorld(_town, out CartSpot spot);
            Cart cart = world.Carts.All[0];
            TestGrids.StepUntil(world, () => cart.Spot != null, 3000, "la charrette appelée");
            Assert.AreEqual(CartState.AtHome, cart.State, "On vient la chercher : elle est encore rangée.");
            var refused = new List<PlazaResult>();
            world.Plaza.PlazaRefused += refused.Add;

            TestTown.Send(world, new RemoveCartSpotCommand(spot.Id));
            CollectionAssert.AreEqual(new[] { PlazaResult.CartBusy }, refused, "Sa charrette arrive.");
            Assert.AreSame(spot, world.Carts.Spots[0], "Elle garde sa place.");
            TestGrids.StepUntil(world, () => cart.State == CartState.Parked, 3000, "la charrette garée");
        }

        [Test]
        public void ACartOnItsWayKeepsItsPlace()
        {
            World world = TestStalls.PlacedWorld(_town, out CartSpot spot);
            Cart cart = world.Carts.All[0];
            TestGrids.StepUntil(world, () => cart.State == CartState.Pulled, 3000, "la charrette attelée");
            var refused = new List<PlazaResult>();
            world.Plaza.PlazaRefused += refused.Add;

            TestTown.Send(world, new RemoveCartSpotCommand(spot.Id));
            CollectionAssert.AreEqual(new[] { PlazaResult.CartBusy }, refused, "Sa charrette arrive.");
            Assert.AreSame(spot, world.Carts.Spots[0], "Elle garde sa place.");
            TestGrids.StepUntil(world, () => cart.State == CartState.Parked, 3000, "la charrette garée");
        }

        [Test]
        public void ACartThatHoldsGoodsKeepsItsPlaceUntilEmptied()
        {
            World world = TestStalls.ParkedWorld(_town, out CartSpot spot, out Cart cart);
            TestStalls.Ask(world, spot, _town.Wood, ConsigneMode.Amount, 6);
            TestGrids.StepUntil(world, () => cart.Stock.CountOf(_town.Wood) == 6, 4000, "six Bois sur la charrette");
            var refused = new List<PlazaResult>();
            world.Plaza.PlazaRefused += refused.Add;

            TestTown.Send(world, new RemoveCartSpotCommand(spot.Id));
            CollectionAssert.AreEqual(new[] { PlazaResult.CartBusy }, refused, "Elle porte encore des marchandises.");
            Assert.AreEqual(CartState.Parked, cart.State);

            TestTown.Send(world, new RemoveConsigneCommand(spot.Id, _town.Wood));
            TestGrids.StepUntil(world, () => !world.Stalls.IsBusy(cart), 4000, "le Bois rentré au campement");
            TestTown.Send(world, new RemoveCartSpotCommand(spot.Id));
            Assert.IsEmpty(world.Carts.Spots, "Vidée, sa place s'en va.");
            Assert.AreEqual(CartState.AtHome, cart.State);
            Assert.AreEqual(30, TestTown.EverywhereOf(world, _town.Wood), "Rien ne s'est perdu.");
        }

        [Test]
        public void ANumberWithoutAPlaceIsRefused()
        {
            World world = TestStalls.ParkedWorld(_town, out CartSpot spot, out Cart _);
            var refused = new List<PlazaResult>();
            world.Plaza.PlazaRefused += refused.Add;
            TestTown.Send(world, new RemoveCartSpotCommand(spot.Id + 1));
            CollectionAssert.AreEqual(new[] { PlazaResult.UnknownMark }, refused);
            Assert.AreEqual(1, world.Carts.Spots.Count);
        }
    }
}
