using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le surplus d'une charrette (GDD, section 11 ; décision 202) : seul le joueur en fait, en baissant une quantité ou
    /// en retirant une ligne ; la Chimère le rapporte au campement. Une consigne qu'on ne touche pas ne rend jamais rien,
    /// même quand le stock de la ville baisse sous « garder au moins N ». Interrompue, elle rend sa charge et ses
    /// promesses. Le banc de l'étal (TestStalls) ; le campement a reçu 30 Bois et 20 Pierre.
    /// </summary>
    public sealed class StallReturnTests
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
        public void LoweringALineSendsTheSurplusBack()
        {
            World world = TestStalls.ParkedWorld(_town, out CartSpot spot, out Cart cart);
            Inventory camp = world.Warehouses.StockOf(TestTown.CampOf(world));
            TestStalls.Ask(world, spot, _town.Wood, ConsigneMode.Amount, 6);
            TestGrids.StepUntil(world, () => cart.Stock.CountOf(_town.Wood) == 6, 4000, "six Bois sur la charrette");

            TestStalls.Ask(world, spot, _town.Wood, ConsigneMode.Amount, 2);
            TestGrids.StepUntil(world, () =>
            {
                Assert.AreEqual(30, TestTown.EverywhereOf(world, _town.Wood), "Rien ne se perd en route.");
                return camp.CountOf(_town.Wood) == 28;
            }, 4000, "le surplus rentré");
            Assert.AreEqual(2, cart.Stock.CountOf(_town.Wood), "Il lui reste ce qu'on lui demande.");
            Assert.AreEqual(0, cart.Stock.SpaceReserved);
            Assert.AreEqual(0, camp.SpaceReserved);
        }

        [Test]
        public void RemovingALineSendsAllOfItBack()
        {
            World world = TestStalls.ParkedWorld(_town, out CartSpot spot, out Cart cart);
            Inventory camp = world.Warehouses.StockOf(TestTown.CampOf(world));
            TestStalls.Ask(world, spot, _town.Stone, ConsigneMode.KeepAtLeast, 15);
            TestGrids.StepUntil(world, () => cart.Stock.CountOf(_town.Stone) == 5, 4000, "cinq Pierre sur la charrette");

            TestTown.Send(world, new RemoveConsigneCommand(spot.Id, _town.Stone));
            TestGrids.StepUntil(world, () => camp.CountOf(_town.Stone) == 20, 4000, "la Pierre rentrée");
            Assert.AreEqual(0, cart.Stock.Total, "La charrette est vide.");
        }

        [Test]
        public void AnUntouchedConsigneNeverSendsAnythingBack()
        {
            // « Garder au moins 20 » : la charrette prend 10 Bois ; puis une cabane en consomme 10 au campement, qui tombe
            // sous 20 : la charrette garde les siens, pendant le chantier et bien après, quand la Chimère est revenue à
            // garnir (bâtir passe avant dans son poste).
            World world = TestStalls.ParkedWorld(_town, out CartSpot spot, out Cart cart);
            TestStalls.Ask(world, spot, _town.Wood, ConsigneMode.KeepAtLeast, 20);
            TestGrids.StepUntil(world, () => cart.Stock.CountOf(_town.Wood) == 10, 4000, "dix Bois sur la charrette");

            Structure cabin = TestTown.Place(world, _town.Cabin, 8f, 24f);
            int most = 10;
            TestGrids.StepUntil(world, () =>
            {
                int now = cart.Stock.CountOf(_town.Wood);
                Assert.GreaterOrEqual(now, most, "Une consigne qu'on ne touche pas ne rend jamais rien.");
                most = now;
                return cabin.State == StructureState.Built;
            }, 8000, "la cabane bâtie");
            for (int i = 0; i < 1500; i++)
            {
                world.Step();
                Assert.AreEqual(10, cart.Stock.CountOf(_town.Wood), "Une consigne qu'on ne touche pas ne rend jamais rien.");
            }
        }

        [Test]
        public void AnInterruptedTripGivesBackItsLoadAndPromises()
        {
            World world = TestStalls.ParkedWorld(_town, out CartSpot spot, out Cart cart);
            Villager chimera = world.Villagers.Chimera;
            Inventory camp = world.Warehouses.StockOf(TestTown.CampOf(world));
            TestStalls.Ask(world, spot, _town.Wood, ConsigneMode.Amount, 6);
            TestGrids.StepUntil(world, () => chimera.Load.CountOf(_town.Wood) > 0, 4000, "la Chimère chargée de Bois");

            TestTown.Send(world, new UnassignVillagerCommand(chimera.Id));
            Assert.AreEqual(0, chimera.Load.Total, "Sa charge est rentrée au campement.");
            Assert.AreEqual(30, camp.CountOf(_town.Wood) + cart.Stock.CountOf(_town.Wood), "Rien ne se perd.");
            Assert.AreEqual(0, world.Stalls.IncomingOf(cart, _town.Wood), "La charrette n'attend plus rien.");
            Assert.AreEqual(0, cart.Stock.SpaceReserved, "Ni aucune place retenue sur elle.");
            Assert.AreEqual(0, camp.SpaceReserved, "Ni au campement.");
        }
    }
}
