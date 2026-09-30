using Diciela.Data;
using Diciela.Simulation;
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
    /// min, 125 pas) et porte ses 3 Bois au tas, nés du défrichage. On retire dans l'ordre où l'on a marqué. Envoyée
    /// dormir avant d'abattre, elle laisse l'arbre avec le travail fait ; après, sa charge rentre au tas. Démarqué, elle
    /// le laisse. Sans place au tas pour ce qu'il rend, rien ne se retire.
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
        public void SheFellsAMarkedTreeAndCarriesItsWoodToThePile()
        {
            World world = _town.WorkingWorld(new[] { NatureTests.Spot(_town.Tree, 28f, 22f) });
            NatureItem tree = world.Nature.Find(1);
            Villager chimera = world.Villagers.Chimera;
            Inventory pile = world.Warehouses.StockOf(_town.PileOf(world));
            for (int i = 0; i < 200; i++)
                world.Step();
            Assert.IsFalse(tree.IsRemoved, "Rien n'est retiré sans être marqué.");

            Send(world, new MarkForClearingCommand(new[] { tree.Id }));
            Assert.AreEqual(3, pile.SpaceReserved, "La place de ses 3 Bois est retenue au tas.");
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
            TestGrids.StepUntil(world, () => pile.CountOf(_town.Wood) == 33, 2000, "le Bois rangé au tas");
            Assert.AreEqual(0, chimera.Load.Total);
            Assert.AreEqual(0, pile.SpaceReserved);
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

            Send(world, new MarkForClearingCommand(new[] { rock.Id, near.Id }));
            TestGrids.StepUntil(world, () => near.IsRemoved || rock.IsRemoved, 4000, "un premier retiré");

            Assert.IsTrue(rock.IsRemoved, "Le rocher, marqué le premier, part le premier, même plus loin.");
            Assert.IsFalse(near.IsRemoved);
        }

        [Test]
        public void SentToBedBeforeFellingSheLeavesTheTreeWithItsWorkDone()
        {
            World world = _town.WorkingWorld(new[] { NatureTests.Spot(_town.Tree, 28f, 22f) });
            NatureItem tree = world.Nature.Find(1);
            Inventory pile = world.Warehouses.StockOf(_town.PileOf(world));
            Send(world, new MarkForClearingCommand(new[] { tree.Id }));
            TestGrids.StepUntil(world, () => world.Clearings.ProgressOf(tree) > 0.2d, 3000, "l'arbre à moitié abattu");

            Send(world, new SendChimeraToBedCommand());
            double done = world.Clearings.ProgressOf(tree);
            for (int i = 0; i < 100; i++)
                world.Step();

            Assert.IsFalse(tree.IsRemoved);
            Assert.IsTrue(tree.IsMarked, "Il reste marqué.");
            Assert.AreEqual(done, world.Clearings.ProgressOf(tree), 1e-9, "Le travail fait se garde.");
            Assert.AreEqual(0, pile.SpaceReserved, "La place retenue est rendue.");

            Send(world, new WakeChimeraCommand());
            TestGrids.StepUntil(world, () => tree.IsRemoved, 3000, "l'arbre retiré au réveil");
        }

        [Test]
        public void SentToBedWithTheWoodSheGivesItBackToThePile()
        {
            World world = _town.WorkingWorld(new[] { NatureTests.Spot(_town.Tree, 28f, 22f) });
            NatureItem tree = world.Nature.Find(1);
            Villager chimera = world.Villagers.Chimera;
            Inventory pile = world.Warehouses.StockOf(_town.PileOf(world));
            Send(world, new MarkForClearingCommand(new[] { tree.Id }));
            TestGrids.StepUntil(world, () => tree.IsRemoved, 3000, "l'arbre retiré");

            Send(world, new SendChimeraToBedCommand());

            Assert.AreEqual(0, chimera.Load.Total, "Sa charge est rendue.");
            Assert.AreEqual(33, pile.CountOf(_town.Wood), "Le tas a le Bois de l'arbre.");
            Assert.AreEqual(0, pile.SpaceReserved);
        }

        [Test]
        public void UnmarkedWhileFellingTheTreeIsLeft()
        {
            World world = _town.WorkingWorld(new[] { NatureTests.Spot(_town.Tree, 28f, 22f) });
            NatureItem tree = world.Nature.Find(1);
            Inventory pile = world.Warehouses.StockOf(_town.PileOf(world));
            Send(world, new MarkForClearingCommand(new[] { tree.Id }));
            TestGrids.StepUntil(world, () => world.Clearings.ProgressOf(tree) > 0d, 3000, "l'arbre entamé");

            Send(world, new UnmarkForClearingCommand(new[] { tree.Id }));
            double done = world.Clearings.ProgressOf(tree);
            for (int i = 0; i < 300; i++)
                world.Step();

            Assert.IsFalse(tree.IsRemoved);
            Assert.AreEqual(done, world.Clearings.ProgressOf(tree), 1e-9, "Personne n'y travaille plus.");
            Assert.AreEqual(0, pile.SpaceReserved);
        }

        [Test]
        public void WithoutRoomAtThePileNothingIsCleared()
        {
            // Un tas qui tient juste ses 50 marchandises de départ : pas de place pour le Bois d'un arbre.
            StructureDefinition fullPile = _town.Keep(TestData.Pile("tas_plein", 2f, 50, _town.OddJobs));
            OpeningDefinition opening = _town.Keep(TestData.Opening(fullPile, 16f, new GoodAmount(_town.Wood, 30),
                new GoodAmount(_town.Stone, 20)));
            Catalog catalog = _town.Keep(TestData.TownCatalog(opening, new[] { _town.Wood, _town.Stone },
                new StatisticDefinition[0]));
            World world = _town.World(catalog, new[] { NatureTests.Spot(_town.Tree, 28f, 22f) });
            TestTown.Place(world, _town.Tent, 20f, 14f);
            Structure pile = world.Structures.All[1];
            Send(world, new AssignVillagerCommand(world.Villagers.Chimera.Id, pile.Id));
            NatureItem tree = world.Nature.Find(1);

            Send(world, new MarkForClearingCommand(new[] { tree.Id }));
            for (int i = 0; i < 1000; i++)
                world.Step();

            Assert.IsFalse(tree.IsRemoved, "Nulle part où ranger son Bois : il attend.");
            Assert.AreEqual(0d, world.Clearings.ProgressOf(tree));
        }

        private static void Send(World world, Command command)
        {
            world.Submit(command);
            world.Step();
        }
    }
}
