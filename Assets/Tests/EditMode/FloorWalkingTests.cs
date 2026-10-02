using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Marcher sur un sol qu'on traverse, comme la Place (GDD, sections 9 et 11 ; décision 192), par l'ordre « poser » :
    /// un trajet le traverse tout droit dès sa pose ; posé sous qui travaille, il ne le chasse pas ; on le bâtit depuis
    /// son bord, jamais dessus. Le banc de la ville au travail : carte plate de 40 m, deux mailles par mètre, le campement
    /// en (20 ; 14) pour les chantiers.
    /// </summary>
    public sealed class FloorWalkingTests
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
        public void APathCrossesAFloorStraightFromItsPlacement()
        {
            // Le sol de 4 × 4 m en (20 ; 30) couvre les mailles 36 à 43 en x, 56 à 63 en z. De la maille (40 ; 52) à la
            // maille (40 ; 68), tout droit : seize pas droits, 160, comme sur une carte vide.
            World world = _town.World();
            Structure floor = TestTown.Place(world, _town.KeepKnown(TestData.Floor("place", 4f, 4f)), 20f, 30f);
            Assert.AreEqual(StructureState.Planned, floor.State, "Le sol est encore un fantôme.");
            var from = new[] { new Vector2Int(40, 52) };
            var to = new[] { new Vector2Int(40, 68) };
            var path = new Path();

            Assert.IsTrue(world.PathFinder.TryFindPath(from, to, path));
            string fault = SlowPaths.Judge(new bool[world.Grid.Width, world.Grid.Depth], from, to, path.Cells,
                out int cost);
            Assert.IsNull(fault, fault);
            Assert.AreEqual(160, cost, "Le trajet traverse le sol tout droit.");
        }

        [Test]
        public void LaidUnderHerWhileSheBuildsItDoesNotSendHerAway()
        {
            World world = _town.WorkingWorld();
            Villager chimera = world.Villagers.Chimera;
            TestTown.Place(world, _town.Cabin, 20f, 24f);
            TestGrids.StepUntil(world, () => chimera.IsBuilding, 3000, "elle bâtit la cabane");
            Vector2 at = chimera.Position;

            // Un sol plus petit qu'une maille, posé sous elle : il ne touche que sa maille.
            float size = 0.8f * world.Grid.Metrics.CellSize;
            StructureDefinition floor = _town.KeepKnown(TestData.Floor("dalle", size, size));
            Vector2 center = world.Grid.CellCenter(world.Grid.CellOf(at));
            Assert.IsNotNull(TestTown.Place(world, floor, center.x, center.y), "Le sol se pose sous elle.");

            for (int step = 0; step < 30; step++)
            {
                Assert.IsTrue(chimera.IsBuilding, "Le sol posé sous elle ne la chasse pas : elle bâtit encore.");
                Assert.AreEqual(at, chimera.Position, "Elle bâtit sans bouger.");
                world.Step();
            }
        }

        [Test]
        public void AFloorIsBuiltFromItsEdges()
        {
            // Le sol de 4 × 4 m en (20 ; 24) couvre x de 18 à 22, z de 22 à 26 ; il coûte 5 Bois et 30 min de travail.
            World world = _town.WorkingWorld();
            Villager chimera = world.Villagers.Chimera;
            StructureDefinition plan = _town.KeepKnown(TestData.Floor("place", 4f, 4f));
            TestData.Cost(plan, 0.5f, new GoodAmount(_town.Wood, 5));
            Structure floor = TestTown.Place(world, plan, 20f, 24f);

            int building = 0;
            TestGrids.StepUntil(world, () =>
            {
                if (chimera.IsBuilding)
                {
                    building++;
                    Assert.AreNotEqual(floor.Id, world.Grid.OccupantAt(world.Grid.CellOf(chimera.Position)),
                        "Elle bâtit le sol depuis son bord, jamais dessus.");
                }
                return floor.State == StructureState.Built;
            }, 3000, "le sol bâti");
            Assert.Greater(building, 0, "Elle ne l'a jamais bâti.");
        }
    }
}
