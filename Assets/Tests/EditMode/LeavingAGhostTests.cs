using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Un fantôme posé sur la Chimère, par l'ordre « poser » (décisions 73 et 88) : elle en sort à pied, tout droit vers
    /// la maille libre qui le borde, du côté du plus grand îlot, au plus près ; jamais dans une poche qu'il ferme. Puis
    /// elle flâne sans y rentrer ; sans maille libre au bord, elle attend. Sur le banc de TestChimera, carte de 40 m :
    /// elle est née en (20,25 ; 20,25), dans la maille (40 ; 40), et y fait sa pause. Les blocs de ces tests ont leurs
    /// bords sur des limites de mailles : ils prennent les mêmes mailles quelle que soit la règle du tramage.
    /// </summary>
    public sealed class LeavingAGhostTests
    {
        private TestChimera _test;

        [SetUp]
        public void CreateData()
        {
            _test = new TestChimera();
        }

        [TearDown]
        public void DestroyData()
        {
            _test.Dispose();
        }

        [Test]
        public void SheWalksStraightOutToTheNearestFreeCell()
        {
            // Un bloc de 4 × 4,2 m centré en (21 ; 20,25) : il couvre les mailles 38 à 45 en x, 36 à 44 en z, et la sienne.
            // La maille libre la plus proche est à l'ouest, (37 ; 40), de milieu (18,75 ; 20,25) : à 1,5 m, soit dix pas
            // de 15 cm ; au nord et au sud, 2,5 m ; à l'est, 3 m.
            World world = _test.World();
            Villager chimera = world.Villagers.Chimera;
            world.Submit(new PlaceStructureCommand(_test.Block(4f, 4.2f), new Placement(new Vector2(21f, 20.25f), 0f)));

            var wayOut = new Vector2(18.75f, 20.25f);
            int steps = TestGrids.StepUntil(world, () =>
            {
                Assert.AreEqual(20.25f, chimera.Position.y, 1e-4f, "Elle sort tout droit, vers l'ouest.");
                Assert.IsFalse(chimera.IsWalking, "En sortant, elle ne suit aucun trajet du maillage.");
                return chimera.Position == wayOut && world.Structures.All.Count == 1;
            }, 12, "la sortie");
            Assert.GreaterOrEqual(steps, 10, "Plus vite que 15 cm par pas.");

            // Dehors, elle flâne sans jamais y rentrer.
            for (int step = 0; step < 3000; step++)
            {
                world.Step();
                Assert.IsTrue(world.Grid.IsWalkable(world.Grid.CellOf(chimera.Position)), "Elle rentre dans le bloc.");
            }
        }

        [Test]
        public void SheNeverLeavesIntoAPocketTheGhostCloses()
        {
            // Trois blocs dessinent une poche à l'est de sa maille, les mailles (42 ; 39 à 41) : un mur en x 43 (z 38 à
            // 42) et deux bouchons, (42 ; 38) et (42 ; 42). Le fantôme posé sur elle couvre x 37 à 41 et z 37 à 43 : il
            // ferme la poche à l'ouest. La maille libre la plus proche est dans la poche, (42 ; 40), à 1 m ; les plus
            // proches du reste de la carte, au bord du fantôme, sont (42 ; 37) et (42 ; 43), à 1,803 m (√(1² + 1,5²)).
            World world = _test.World();
            Villager chimera = world.Villagers.Chimera;
            Vector2 start = chimera.Position;
            Place(world, 0.5f, 2.5f, 21.75f, 20.25f);
            Place(world, 0.5f, 0.5f, 21.25f, 21.25f);
            Place(world, 0.5f, 0.5f, 21.25f, 19.25f);
            Place(world, 2.5f, 3.5f, 19.75f, 20.25f);
            world.Step();
            Assert.AreEqual(4, world.Structures.All.Count);
            int land = world.Islands.IslandOf(new Vector2Int(0, 0));
            Assert.AreNotEqual(land, world.Islands.IslandOf(new Vector2Int(42, 40)), "La poche n'est pas fermée.");

            TestGrids.StepUntil(world, () => world.Grid.IsWalkable(world.Grid.CellOf(chimera.Position)) &&
                                             chimera.Position == chimera.PreviousPosition, 100, "la sortie");

            Assert.AreEqual(land, world.Islands.IslandOf(world.Grid.CellOf(chimera.Position)), "Elle sort dans la poche.");
            Assert.AreEqual(1.803f, Vector2.Distance(start, chimera.Position), 1e-3f, "Pas la sortie la plus proche.");
        }

        [Test]
        public void AGhostOnHerWayOutMakesHerChooseAnother()
        {
            // Le bloc du premier test la fait sortir vers l'ouest, par la maille (37 ; 40). Trois pas plus tard, elle est
            // en (19,8 ; 20,25), encore dans le bloc, quand un second bloc prend cette maille : elle en choisit une
            // autre au bord du premier, sans jamais entrer dans le second.
            World world = _test.World();
            Villager chimera = world.Villagers.Chimera;
            Place(world, 4f, 4.2f, 21f, 20.25f);
            for (int step = 0; step < 3; step++)
                world.Step();
            Assert.AreEqual(19.8f, chimera.Position.x, 1e-4f);

            Place(world, 0.5f, 0.5f, 18.75f, 20.25f);
            var taken = new Vector2Int(37, 40);
            for (int step = 0; step < 60; step++)
            {
                world.Step();
                Assert.AreNotEqual(taken, world.Grid.CellOf(chimera.Position), "Elle entre dans le second bloc.");
            }
            Assert.AreEqual(2, world.Structures.All.Count);
            Assert.IsTrue(world.Grid.IsWalkable(world.Grid.CellOf(chimera.Position)), "Elle n'est pas sortie.");
        }

        [Test]
        public void WhenAllAroundWhereSheWandersIsCoveredSheWandersElsewhere()
        {
            // Un bloc de 13 × 13 m centré en (20 ; 20) couvre les mailles 27 à 52 dans les deux sens : tout le disque de
            // 6 m (12 mailles) autour de (40 ; 40), où elle flânait. Elle en sort à 6,5 m (44 pas), puis doit flâner de
            // nouveau, autour d'où elle est sortie : 44 pas, une pause de 90 pas au plus, un tirage.
            World world = _test.World();
            Villager chimera = world.Villagers.Chimera;
            Place(world, 13f, 13f, 20f, 20f);

            TestGrids.StepUntil(world, () => chimera.IsWalking, 1000, "une nouvelle flânerie");
            Assert.AreEqual(1, world.Structures.All.Count);
        }

        [Test]
        public void EnclosedSheWaitsWithoutFailing()
        {
            // Une construction couvre toute la carte : aucune maille libre, elle attend là où elle est.
            World world = _test.World();
            Villager chimera = world.Villagers.Chimera;
            Vector2 where = chimera.Position;
            world.Submit(new PlaceStructureCommand(_test.Block(40f, 40f), new Placement(new Vector2(20f, 20f), 0f)));

            for (int step = 0; step < 200; step++)
                world.Step();

            Assert.AreEqual(1, world.Structures.All.Count);
            Assert.AreEqual(where, chimera.Position);
            Assert.IsFalse(chimera.IsWalking);
        }

        [Test]
        public void EnclosedInTheMiddleOfAWalkSheIsNoLongerWalking()
        {
            // En pleine marche, tout est couvert : elle s'arrête là où elle est, et ne se dit plus en marche.
            World world = _test.World();
            Villager chimera = TestChimera.StartALongWalk(world);
            world.Step();
            Assert.IsTrue(chimera.IsWalking);
            world.Submit(new PlaceStructureCommand(_test.Block(40f, 40f), new Placement(new Vector2(20f, 20f), 0f)));
            world.Step();
            Vector2 where = chimera.Position;

            for (int step = 0; step < 50; step++)
            {
                world.Step();
                Assert.IsFalse(chimera.IsWalking, "Enfermée, elle se dit en marche au pas " + step + ".");
            }
            Assert.AreEqual(where, chimera.Position);
        }

        /// <summary>Envoie l'ordre de poser un bloc de width × depth mètres, droit, centré en (x ; z).</summary>
        private void Place(World world, float width, float depth, float x, float z)
        {
            world.Submit(new PlaceStructureCommand(_test.Block(width, depth), new Placement(new Vector2(x, z), 0f)));
        }
    }
}
