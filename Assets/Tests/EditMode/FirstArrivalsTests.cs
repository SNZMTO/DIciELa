using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les premiers habitants (GDD, sections 3, 4 et 6), par l'ordre « poser » et le temps qui passe : ils arrivent dans
    /// l'ordre du déroulé, un par un, chacun quand une maison bâtie a un lit libre, une heure au moins après le précédent,
    /// jamais la nuit ; au point d'arrivée de la carte, forts dans leur métier (40), le reste tiré de 5 à 20 ; chacun a
    /// son lit, rentre dormir chez lui la nuit et se lève au matin. Le banc de la ville au travail (une heure : 300 pas ;
    /// le travail jusqu'à 16 h, la soirée jusqu'à 18 h, puis la nuit), un monde de 40 m dont le point d'arrivée est le
    /// centre, et une maison de 4 m qui offre deux lits, dressée dès la pose.
    /// </summary>
    public sealed class FirstArrivalsTests
    {
        private const int StepsPerHour = 300;
        private const int MaxSteps = 20000;
        private TestTown _town;
        private StructureDefinition _house;
        private FirstVillagerDefinition _builder;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            _builder = _town.Keep(TestData.FirstVillager("batisseur", _town.Build.SubStatistic,
                _town.Keep(TestData.Character("homme"))));
            FirstVillagerDefinition carter = _town.Keep(TestData.FirstVillager("charretiere", _town.Carry.SubStatistic,
                _town.Keep(TestData.Character("femme"))));
            FirstVillagerDefinition landscaper = _town.Keep(TestData.FirstVillager("paysagiste",
                _town.Clear.SubStatistic, _town.Keep(TestData.Character("autre"))));
            TestData.FirstVillagers(_town.Opening, _builder, carter, landscaper);
            TestData.Ordinary(_town.Catalog, _town.Keep(TestData.OrdinaryKind()));
            _house = _town.KeepKnown(TestData.House("maison", 4f, 2));
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void NobodyArrivesBeforeAHouseHasAFreeBed()
        {
            World world = _town.World();

            Run(world, 2 * StepsPerHour);

            Assert.AreEqual(1, world.Villagers.All.Count, "Seule la Chimère : aucune maison n'offre de lit.");
        }

        [Test]
        public void AHouseNotYetBuiltOffersNoBed()
        {
            TestData.Beds(_town.Cabin, 2);
            World world = _town.World();

            Assert.IsNotNull(TestTown.Place(world, _town.Cabin, 30f, 20f), "La cabane est posée : un fantôme.");
            Run(world, 2 * StepsPerHour);

            Assert.AreEqual(1, world.Villagers.All.Count, "Une cabane qui attend ses bâtisseurs n'offre pas encore ses lits.");
        }

        [Test]
        public void TheyArriveOneAnHourAtTheArrivalPointWhileBedsAreFree()
        {
            World world = _town.World();

            Structure house = TestTown.Place(world, _house, 30f, 20f);

            Assert.AreEqual(2, world.Villagers.All.Count, "Le bâtisseur arrive dès que la maison offre ses lits.");
            Villager builder = world.Villagers.All[1];
            Assert.Less(Vector2.Distance(builder.Position, new Vector2(20f, 20f)), 0.5f, "Au point d'arrivée de la carte.");
            Assert.AreSame(house, world.Housing.HouseOf(builder), "Il a son lit dans la maison.");

            Run(world, StepsPerHour - 1);
            Assert.AreEqual(2, world.Villagers.All.Count, "Une heure au moins entre deux arrivées.");
            Run(world, 1);
            Assert.AreEqual(3, world.Villagers.All.Count, "La charretière arrive une heure après.");

            Run(world, 2 * StepsPerHour);
            Assert.AreEqual(3, world.Villagers.All.Count, "Plus de lit libre : le troisième attend.");
            TestTown.Place(world, _house, 30f, 30f);
            Assert.AreEqual(4, world.Villagers.All.Count, "Une nouvelle maison, et il arrive.");
        }

        [Test]
        public void EachIsStrongInHisTradeAndOrdinaryElsewhere()
        {
            World world = _town.World();
            TestTown.Place(world, _house, 30f, 20f);

            VillagerStatistics statistics = world.Villagers.All[1].Statistics;

            Assert.AreEqual(40f, statistics.ValueOf(_town.Build.SubStatistic), "Le bâtisseur commence à 40 en construction.");
            foreach (SubStatisticDefinition other in new[] { _town.Carry.SubStatistic, _town.Clear.SubStatistic })
            {
                float value = statistics.ValueOf(other);
                Assert.That(value, Is.InRange(5f, 20f), "Ailleurs, un arrivant commence entre 5 et 20.");
            }
        }

        [Test]
        public void NobodyArrivesAtNight()
        {
            World world = _town.World();
            RunUntil(world, () => world.Clock.Phase == DayPhase.Night);

            TestTown.Place(world, _house, 30f, 20f);
            Run(world, StepsPerHour);
            Assert.AreEqual(1, world.Villagers.All.Count, "La nuit, la ville dort : personne n'arrive.");

            RunUntil(world, () => world.Clock.Phase == DayPhase.Work);
            Assert.AreEqual(2, world.Villagers.All.Count, "Au matin, le bâtisseur arrive.");
        }

        [Test]
        public void AtNightTheyGoHomeToSleepAndGetUpInTheMorning()
        {
            World world = _town.World();
            Structure house = TestTown.Place(world, _house, 30f, 20f);
            Villager builder = world.Villagers.All[1];
            Villager chimera = world.Villagers.Chimera;

            RunUntil(world, () => world.Clock.Phase == DayPhase.Night);
            Assert.IsTrue(builder.IsGoingToBed, "Au début de la nuit, il rentre chez lui.");
            RunUntil(world, () => builder.IsAsleep);
            Assert.AreSame(house, world.Villagers.HomeOf(builder));
            Assert.Less(Vector2.Distance(builder.Position, world.Housing.DoorOf(house)), 0.5f, "Il dort devant sa porte.");
            Assert.IsFalse(chimera.IsAsleep || chimera.IsGoingToBed, "La Chimère ne dort que si le joueur l'y envoie.");

            RunUntil(world, () => world.Clock.Phase == DayPhase.Work);
            Assert.IsFalse(builder.IsAsleep || builder.IsGoingToBed, "Au matin, il se lève.");
        }

        private static void Run(World world, int steps)
        {
            for (int i = 0; i < steps; i++)
                world.Step();
        }

        private static void RunUntil(World world, System.Func<bool> done)
        {
            for (int i = 0; i < MaxSteps && !done(); i++)
                world.Step();
            Assert.IsTrue(done(), "Le monde n'y est pas arrivé en " + MaxSteps + " pas.");
        }
    }
}
