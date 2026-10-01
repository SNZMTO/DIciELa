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
    /// Le poste « à tout faire » tenu par la Chimère (GDD, sections 6, 7 et 9 ; décisions 137 à 140), par les ordres du
    /// joueur : elle porte d'abord tous les matériaux de la cabane, 5 par voyage, puis la bâtit ; à 10 partout, elle y
    /// apporte ×1,2, et les 2 h d'un débutant lui prennent 1 h 40 (500 pas) ; l'avancement du chantier (que suit sa
    /// ligne qui monte) va de 0 à 1 au fil de ces pas. Le Bois consommé disparaît du bilan. Le soir, elle cesse de bâtir
    /// tout de suite, ne prend plus rien, et flâne près du tas ; le matin, elle reprend.
    /// </summary>
    public sealed class OddJobsTests
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
        public void SheCarriesEverythingThenBuildsTheCabin()
        {
            World world = _town.WorkingWorld();
            Structure cabin = TestTown.Place(world, _town.Cabin, 28f, 20f);
            ConstructionSite site = world.ConstructionSites.SiteOf(cabin);
            Villager chimera = world.Villagers.Chimera;
            long firstBuildingStep = -1;
            bool builtBeforeSupplied = false;
            double progressAtStart = -1d;
            double progressHalfway = -1d;

            TestGrids.StepUntil(world, () =>
            {
                if (chimera.IsBuilding && firstBuildingStep < 0)
                {
                    firstBuildingStep = world.Clock.StepCount;
                    builtBeforeSupplied = !site.IsSupplied;
                    progressAtStart = site.Progress;
                }
                if (firstBuildingStep >= 0 && world.Clock.StepCount == firstBuildingStep + 250)
                    progressHalfway = site.Progress;
                return cabin.State == StructureState.Built;
            }, 4000, "la cabane bâtie");

            Assert.IsFalse(builtBeforeSupplied, "On ne bâtit qu'une fois tous les matériaux livrés.");
            Assert.AreEqual(500, world.Clock.StepCount - firstBuildingStep, 1, "2 h d'un débutant, à ×1,2 : 1 h 40.");
            Assert.AreEqual(0d, progressAtStart, 0.01d, "Rien n'est bâti au premier pas.");
            Assert.AreEqual(0.5d, progressHalfway, 0.01d, "À 250 pas sur 500, la moitié.");
            Assert.AreEqual(1d, site.Progress, "Fini.");
            Assert.AreEqual(20, world.Warehouses.CityStockOf(_town.Wood));
            Assert.AreEqual(15, world.Warehouses.CityStockOf(_town.Stone));
            Assert.AreEqual(10, world.Ledger.DestroyedOf(_town.Wood), "Le Bois de la cabane est pris dans la construction.");
            Assert.AreEqual(world.Ledger.CreatedOf(_town.Wood) - world.Ledger.DestroyedOf(_town.Wood),
                TestTown.EverywhereOf(world, _town.Wood), "Le bilan égale la somme des causes.");
            Assert.AreEqual(0, chimera.Load.Total);
            Assert.IsNull(world.ConstructionSites.SiteOf(cabin), "Le chantier a quitté la file.");
        }

        [Test]
        public void EachTripCarriesOneGoodFiveAtATime()
        {
            World world = _town.WorkingWorld();
            Structure cabin = TestTown.Place(world, _town.Cabin, 28f, 20f);
            Villager chimera = world.Villagers.Chimera;
            int trips = 0;
            bool carrying = false;

            TestGrids.StepUntil(world, () =>
            {
                Inventory load = chimera.Load;
                Assert.LessOrEqual(load.LineCount == 0 ? 0 : CountLines(load), 1, "Une seule marchandise par voyage.");
                if (load.Total > 0 && !carrying)
                    trips++;
                carrying = load.Total > 0;
                return world.ConstructionSites.SiteOf(cabin).IsSupplied;
            }, 4000, "la cabane livrée");

            Assert.AreEqual(3, trips, "10 Bois et 5 Pierre : trois voyages de 5.");
        }

        [Test]
        public void TheEveningStopsTheWorkAndTheMorningResumesIt()
        {
            _town.Dispose();
            _town = new TestTown(dayLengthSeconds: 100f);
            // 1 000 pas par jour, 667 de travail : une cabane de 10 h de travail ne se finit pas le premier jour.
            StructureDefinition longCabin = _town.Keep(TestData.Structure("grande_cabane", 4f, 4f));
            TestData.Cost(longCabin, 10f, new GoodAmount(_town.Wood, 10),
                new GoodAmount(_town.Stone, 5));
            World world = _town.WorkingWorld();
            Structure cabin = TestTown.Place(world, longCabin, 30f, 28f);
            Villager chimera = world.Villagers.Chimera;
            Structure pile = _town.PileOf(world);
            Inventory pileStock = world.Warehouses.StockOf(pile);
            int lastTotal = pileStock.Total;
            bool tookOffHours = false;
            bool builtInTheEvening = false;
            float lastNightDistance = -1f;

            TestGrids.StepUntil(world, () =>
            {
                DayPhase phase = world.Clock.Phase;
                if (phase != DayPhase.Work && pileStock.Total < lastTotal)
                    tookOffHours = true;
                lastTotal = pileStock.Total;
                if (phase != DayPhase.Work && chimera.IsBuilding)
                    builtInTheEvening = true;
                if (world.Clock.StepCount % world.Clock.StepsPerDay == world.Clock.StepsPerDay - 1)
                    lastNightDistance = Vector2.Distance(chimera.Position, pile.Placement.Center);
                return cabin.State == StructureState.Built;
            }, 5000, "la grande cabane bâtie");

            Assert.IsFalse(tookOffHours, "Hors des heures de travail, elle ne prend rien au tas.");
            Assert.IsFalse(builtInTheEvening, "Le soir, elle cesse de bâtir tout de suite.");
            Assert.AreEqual(2, world.Clock.Day, "Elle a fini le lendemain.");
            Assert.GreaterOrEqual(lastNightDistance, 0f);
            Assert.Less(lastNightDistance, 9f, "La nuit, elle flâne près du tas (6 m autour de ses abords).");
        }

        [Test]
        public void PushedOffTheSiteSheWalksBackBeforeBuildingAgain()
        {
            World world = _town.WorkingWorld();
            // La porte à l'est : elle arrive de l'ouest ou du sud, où un mur peut se poser.
            Structure cabin = TestTown.Place(world, _town.Cabin, 28f, 20f, 90f);
            Villager chimera = world.Villagers.Chimera;
            TestGrids.StepUntil(world, () => chimera.IsBuilding, 4000, "la Chimère au travail");

            // Un mur de 50 cm tout le long du côté où elle bâtit, posé sur elle : elle en sort du côté opposé au chantier.
            Vector2 at = chimera.Position;
            bool west = at.x < 26f;
            StructureDefinition wall = _town.Keep(TestData.Structure("mur", west ? 0.5f : 5f, west ? 5f : 0.5f,
                hasDoor: false));
            Structure placed = TestTown.Place(world, wall, west ? 25.75f : 28f, west ? 20f : 17.75f);
            Assert.IsNotNull(placed, "Le mur se pose sur elle, contre le chantier.");

            bool farAndBuilding = false;
            TestGrids.StepUntil(world, () =>
            {
                Vector2Int cell = world.Grid.CellOf(chimera.Position);
                if (chimera.IsBuilding && world.Grid.IsWalkable(cell) && !world.Grid.Borders(cell, cabin.Id))
                    farAndBuilding = true;
                return cabin.State == StructureState.Built;
            }, 8000, "la cabane bâtie");
            Assert.IsFalse(farAndBuilding, "Elle ne bâtit qu'aux abords du chantier.");
        }

        private static int CountLines(Inventory load)
        {
            int lines = 0;
            for (int i = 0; i < load.LineCount; i++)
            {
                if (load.CountAt(i) > 0)
                    lines++;
            }
            return lines;
        }
    }
}
