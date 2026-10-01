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
    /// Défricher au poste « à tout faire » (GDD, sections 2 et 7 ; décisions 157 à 160), par les ordres du joueur : un
    /// arbre marqué, la Chimère y va quand il n'y a rien à porter ni à bâtir, l'abat (30 min d'un débutant, à ×1,2 : 25
    /// min, 125 pas) et porte ses 3 Bois au campement, nés du défrichage. On retire dans l'ordre où l'on a marqué. Sans place
    /// au campement pour ce qu'il rend, rien ne se retire. Un bosquet entier se défriche en traversant les soirs, et le bilan
    /// des marchandises tombe juste. Ce qui l'interrompt est dans ClearingInterruptionTests.
    /// </summary>
    public sealed class ClearingTests
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
        public void SheFellsAMarkedTreeAndCarriesItsWoodToTheCamp()
        {
            World world = _town.WorkingWorld(new[] { NatureTests.Spot(_town.Tree, 28f, 22f) });
            NatureItem tree = world.Nature.Find(1);
            Villager chimera = world.Villagers.Chimera;
            Inventory stock = world.Warehouses.StockOf(TestTown.CampOf(world));
            for (int i = 0; i < 200; i++)
                world.Step();
            Assert.IsFalse(tree.IsRemoved, "Rien n'est retiré sans être marqué.");

            TestTown.Send(world, new MarkForClearingCommand(new[] { tree.Id }));
            Assert.AreEqual(3, stock.SpaceReserved, "La place de ses 3 Bois est retenue au campement.");
            long firstWork = -1;
            TestGrids.StepUntil(world, () =>
            {
                if (firstWork < 0 && world.Clearings.ProgressOf(tree) > 0d)
                    firstWork = world.Clock.StepCount;
                return tree.IsRemoved;
            }, 3000, "l'arbre retiré");

            Assert.AreEqual(125, world.Clock.StepCount - firstWork, 1, "30 min d'un débutant, à ×1,2 : 25 min.");
            Assert.AreEqual(3, chimera.Load.CountOf(_town.Wood), "Elle porte ce qu'il rend.");
            Assert.AreEqual(PlacementResult.Ok,
                world.PlacementRules.Check(_town.Cabin, new Placement(new Vector2(28f, 22f), 0f)), "La place est libre.");
            TestGrids.StepUntil(world, () => stock.CountOf(_town.Wood) == 33, 2000, "le Bois rangé au campement");
            Assert.AreEqual(0, chimera.Load.Total);
            Assert.AreEqual(0, stock.SpaceReserved);
            Assert.AreEqual(33, world.Ledger.CreatedOf(_town.Wood), "30 nés du départ, 3 du défrichage.");
            Assert.AreEqual(world.Ledger.CreatedOf(_town.Wood), TestTown.EverywhereOf(world, _town.Wood));
        }

        [Test]
        public void TreesAreClearedInTheOrderTheyWereMarked()
        {
            World world = _town.WorkingWorld(new[]
            {
                NatureTests.Spot(_town.Tree, 22f, 18f), NatureTests.Spot(_town.Rock, 32f, 30f)
            });
            NatureItem near = world.Nature.Find(1);
            NatureItem rock = world.Nature.Find(2);

            TestTown.Send(world, new MarkForClearingCommand(new[] { rock.Id, near.Id }));
            TestGrids.StepUntil(world, () => near.IsRemoved || rock.IsRemoved, 4000, "un premier retiré");

            Assert.IsTrue(rock.IsRemoved, "Le rocher, marqué le premier, part le premier, même plus loin.");
            Assert.IsFalse(near.IsRemoved);
        }

        [Test]
        public void WithoutRoomAtTheCampNothingIsCleared()
        {
            // Un campement qui tient juste ses 50 marchandises de départ : pas de place pour le Bois d'un arbre.
            StructureDefinition fullCamp = _town.Keep(TestData.Home("campement_plein", 3f, true, 50, _town.OddJobs));
            OpeningDefinition opening = _town.Keep(TestData.Opening(new GoodAmount(_town.Wood, 30),
                new GoodAmount(_town.Stone, 20)));
            Catalog catalog = _town.Keep(TestData.TownCatalog(opening, new[] { _town.Wood, _town.Stone },
                new StatisticDefinition[0]));
            World world = _town.World(catalog, new[] { NatureTests.Spot(_town.Tree, 28f, 22f) });
            TestTown.Place(world, fullCamp, 20f, 14f);
            Structure camp = TestTown.CampOf(world);
            TestTown.Send(world, new AssignVillagerCommand(world.Villagers.Chimera.Id, camp.Id));
            NatureItem tree = world.Nature.Find(1);

            TestTown.Send(world, new MarkForClearingCommand(new[] { tree.Id }));
            for (int i = 0; i < 1000; i++)
                world.Step();

            Assert.IsFalse(tree.IsRemoved, "Nulle part où ranger son Bois : il attend.");
            Assert.AreEqual(0d, world.Clearings.ProgressOf(tree));
        }

        [Test]
        public void SheClearsAWholeGroveAcrossTheEveningsAndTheBooksBalance()
        {
            // Une journée de 1 000 pas, 667 de travail : le bosquet prend plusieurs jours. Douze arbres et quatre rochers
            // autour du campement : 36 Bois et 8 Pierre de plus, 94 marchandises au campement (100).
            _town.Dispose();
            _town = new TestTown(dayLengthSeconds: 100f);
            var spots = new List<NatureSpot>();
            for (int i = 0; i < 12; i++)
                spots.Add(NatureTests.Spot(_town.Tree, 6f + 2.5f * (i % 6), 26f + 3f * (i / 6)));
            for (int i = 0; i < 4; i++)
                spots.Add(NatureTests.Spot(_town.Rock, 30f + 3f * (i % 2), 8f + 3f * (i / 2)));
            World world = _town.WorkingWorld(spots);
            Villager chimera = world.Villagers.Chimera;
            Inventory stock = world.Warehouses.StockOf(TestTown.CampOf(world));
            var ids = new List<int>();
            foreach (NatureItem item in world.Nature.All)
                ids.Add(item.Id);
            TestTown.Send(world, new MarkForClearingCommand(ids));

            bool sawTheEvening = false;
            TestGrids.StepUntil(world, () =>
            {
                sawTheEvening |= world.Clock.Phase != DayPhase.Work;
                return world.Nature.Marked.Count == 0 && chimera.Load.Total == 0 && stock.SpaceReserved == 0;
            }, 40000, "le bosquet défriché et rangé");

            Assert.IsTrue(sawTheEvening, "Le travail a traversé au moins un soir.");
            foreach (NatureItem item in world.Nature.All)
                Assert.IsTrue(item.IsRemoved, "Chacun est retiré : n° " + item.Id);
            Assert.AreEqual(66, stock.CountOf(_town.Wood));
            Assert.AreEqual(28, stock.CountOf(_town.Stone));
            Assert.AreEqual(world.Ledger.CreatedOf(_town.Wood), TestTown.EverywhereOf(world, _town.Wood));
            Assert.AreEqual(world.Ledger.CreatedOf(_town.Stone), TestTown.EverywhereOf(world, _town.Stone));
            for (int x = 0; x < world.Grid.Width; x++)
            {
                for (int z = 0; z < world.Grid.Depth; z++)
                    Assert.GreaterOrEqual(world.Grid.OccupantAt(new Vector2Int(x, z)), 0, "Plus aucune maille à un arbre.");
            }
        }
    }
}
