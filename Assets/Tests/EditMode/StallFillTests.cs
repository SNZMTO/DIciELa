using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Garnir une charrette (GDD, section 11 ; décision 202), par l'ordre « garnir » : la Chimère porte à la charrette
    /// garée ce que demande la consigne de sa place, ligne après ligne, dans la limite de ses 20 places ; une ligne
    /// « garder au moins N » prend ce qui dépasse N au stock. Rien ne se perd en route. Le banc de l'étal (TestStalls) ;
    /// le campement a reçu 30 Bois et 20 Pierre, la Chimère porte 5 places.
    /// </summary>
    public sealed class StallFillTests
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
        public void TheChimeraBringsWhatTheConsigneAsks()
        {
            World world = TestStalls.ParkedWorld(_town, out Structure place, out Cart cart);
            Inventory camp = world.Warehouses.StockOf(TestTown.CampOf(world));

            TestStalls.Ask(world, place, _town.Wood, ConsigneMode.Amount, 6);
            TestGrids.StepUntil(world, () =>
            {
                Assert.AreEqual(30, TestTown.EverywhereOf(world, _town.Wood), "Rien ne se perd en route.");
                return cart.Stock.CountOf(_town.Wood) == 6;
            }, 4000, "six Bois sur la charrette");
            Assert.AreEqual(24, camp.CountOf(_town.Wood), "Ils sont pris au campement.");
            for (int i = 0; i < 600; i++)
                world.Step();
            Assert.AreEqual(6, cart.Stock.CountOf(_town.Wood), "Elle en a ce qu'on lui demande, ni plus ni moins.");
            Assert.AreEqual(0, camp.SpaceReserved, "Plus aucune place retenue au campement.");
            Assert.AreEqual(0, cart.Stock.SpaceReserved, "Ni sur la charrette.");
        }

        [Test]
        public void KeepAtLeastTakesWhatIsAboveIt()
        {
            World world = TestStalls.ParkedWorld(_town, out Structure place, out Cart cart);
            Inventory camp = world.Warehouses.StockOf(TestTown.CampOf(world));

            TestStalls.Ask(world, place, _town.Stone, ConsigneMode.KeepAtLeast, 15);
            TestGrids.StepUntil(world, () => cart.Stock.CountOf(_town.Stone) == 5, 4000, "cinq Pierre sur la charrette");
            for (int i = 0; i < 600; i++)
                world.Step();
            Assert.AreEqual(5, cart.Stock.CountOf(_town.Stone), "Ce qui dépasse 15 au stock, rien de plus.");
            Assert.AreEqual(15, camp.CountOf(_town.Stone), "Le campement en garde 15.");
        }

        [Test]
        public void TheLinesShareTheCartsPlacesInOrder()
        {
            // 15 Bois puis 15 Pierre : la charrette tient 20 places ; le Bois, d'abord, en prend 15, la Pierre le reste.
            World world = TestStalls.ParkedWorld(_town, out Structure place, out Cart cart);
            TestStalls.Ask(world, place, _town.Wood, ConsigneMode.Amount, 15);
            TestStalls.Ask(world, place, _town.Stone, ConsigneMode.Amount, 15);

            TestGrids.StepUntil(world, () => cart.Stock.Used == 20, 6000, "la charrette pleine");
            Assert.AreEqual(15, cart.Stock.CountOf(_town.Wood), "La première ligne est servie d'abord.");
            Assert.AreEqual(5, cart.Stock.CountOf(_town.Stone), "La seconde prend les places qui restent.");
        }

        [Test]
        public void ABulkyGoodTakesMorePlacesOnTheCart()
        {
            // Des meubles, deux places chacun : sur 20 places, dix meubles au plus.
            GoodDefinition furniture = _town.Keep(TestData.Good("meuble", 2));
            TestData.StartingGoods(_town.Opening, new GoodAmount(_town.Wood, 30), new GoodAmount(furniture, 15));
            World world = TestStalls.ParkedWorld(_town, out Structure place, out Cart cart);

            TestStalls.Ask(world, place, furniture, ConsigneMode.Amount, 15);
            TestGrids.StepUntil(world, () => cart.Stock.Used == 20, 6000, "la charrette pleine");
            Assert.AreEqual(10, cart.Stock.CountOf(furniture), "Dix meubles remplissent ses vingt places.");
        }

        [Test]
        public void NothingIsBroughtToACartOnItsWay()
        {
            // La consigne réglée dès la pose de la Place : rien ne part tant que la charrette n'est pas garée.
            TestData.Carts(_town.Camp, 1, 0);
            StructureDefinition plan = _town.KeepKnown(TestData.Floor("place", 12f, 12f));
            TestData.Carts(plan, 0, 2);
            TestData.BuiltWhenPlaced(plan);
            World world = _town.WorkingWorld();
            Structure place = TestTown.Place(world, plan, 20f, 26f);
            Cart cart = world.Carts.All[0];
            TestStalls.Ask(world, place, _town.Wood, ConsigneMode.Amount, 6);

            TestGrids.StepUntil(world, () =>
            {
                if (cart.State != CartState.Parked)
                    Assert.AreEqual(0, cart.Stock.Total + world.Stalls.IncomingOf(cart, _town.Wood),
                        "On ne garnit qu'une charrette garée.");
                return cart.Stock.CountOf(_town.Wood) == 6;
            }, 6000, "six Bois sur la charrette garée");
        }
    }
}
