using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Un fantôme posé sur la Chimère, par l'ordre « poser » (décisions 73 et 74) : elle en sort à pied, tout droit vers
    /// la maille libre la plus proche, puis flâne sans y rentrer ; sans aucune maille libre, elle attend. Sur le banc de
    /// TestChimera, carte de 40 m : elle est née en (20,25 ; 20,25), dans la maille (40 ; 40), et y fait sa pause.
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
    }
}
