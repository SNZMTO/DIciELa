using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les arbres et les rochers de la carte (GDD, section 2 ; décision 157), par les ordres du joueur : debout, ils
    /// prennent leurs mailles (on ne les traverse pas, rien ne se pose dessus, et la pose dit pourquoi) ; marqués, ils le
    /// sont dans l'ordre des gestes, et chaque geste se dit ; deux pieds peints l'un contre l'autre se partagent une maille,
    /// et retirer l'un laisse l'autre en place.
    /// </summary>
    public sealed class NatureTests
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
        public void StandingTheyHoldTheirCellsAndNothingIsPlacedOnThem()
        {
            World world = _town.World(null, new[] { Spot(_town.Tree, 30f, 30f), Spot(_town.Rock, 10f, 30f) });
            NatureItem tree = world.Nature.Find(1);

            Vector2Int foot = world.Grid.CellOf(new Vector2(30f, 30f));
            Assert.IsFalse(world.Grid.IsWalkable(foot), "On ne traverse pas un arbre.");
            Assert.AreEqual(tree.Occupant, world.Grid.OccupantAt(foot));
            Assert.Less(tree.Occupant, 0, "Un arbre ne se confond jamais avec une construction.");
            Assert.AreEqual(PlacementResult.NatureInTheWay,
                world.PlacementRules.Check(_town.Cabin, new Placement(new Vector2(30f, 31f), 0f)));
            Assert.AreEqual(PlacementResult.NatureInTheWay,
                world.PlacementRules.Check(_town.Cabin, new Placement(new Vector2(10f, 30f), 0f)), "Le rocher aussi.");

            // Une construction sous l'empreinte l'emporte : retirer l'arbre n'y changerait rien.
            Assert.IsNotNull(TestTown.Place(world, _town.Cabin, 27f, 30f));
            Assert.AreEqual(PlacementResult.Overlaps,
                world.PlacementRules.Check(_town.Cabin, new Placement(new Vector2(29f, 30f), 0f)));
        }

        [Test]
        public void MarkingKeepsTheOrderOfTheGesturesAndEachGestureIsHeard()
        {
            World world = _town.World(null,
                new[] { Spot(_town.Tree, 10f, 10f), Spot(_town.Tree, 20f, 10f), Spot(_town.Rock, 30f, 10f) });
            var heard = new List<string>();
            world.Nature.ItemMarked += item => heard.Add("+" + item.Id);
            world.Nature.ItemUnmarked += item => heard.Add("-" + item.Id);

            TestTown.Send(world, new MarkForClearingCommand(new[] { 3, 1 }));
            TestTown.Send(world, new MarkForClearingCommand(new[] { 2, 3, 99 }));
            Assert.AreEqual(new[] { 3, 1, 2 }, IdsOf(world.Nature.Marked), "Dans l'ordre des gestes, sans doublon.");

            TestTown.Send(world, new UnmarkForClearingCommand(new[] { 1, 1 }));
            Assert.AreEqual(new[] { 3, 2 }, IdsOf(world.Nature.Marked));
            Assert.IsFalse(world.Nature.Find(1).IsMarked);
            Assert.AreEqual(new[] { "+3", "+1", "+2", "-1" }, heard.ToArray(), "Un événement par changement.");
        }

        [Test]
        public void ClearingOneOfTwoTouchingTreesLeavesTheOtherInPlace()
        {
            // Deux pieds de 1 m à 60 cm l'un de l'autre : une colonne de mailles est à l'un et à l'autre.
            World world = _town.WorkingWorld(new[] { Spot(_town.Tree, 28f, 22f), Spot(_town.Tree, 28.6f, 22f) });
            NatureItem first = world.Nature.Find(1);
            NatureItem second = world.Nature.Find(2);
            Vector2Int shared = world.Grid.CellOf(new Vector2(28.3f, 22f));
            Assert.AreEqual(second.Occupant, world.Grid.OccupantAt(shared), "Le dernier de la carte garde la maille.");

            TestTown.Send(world, new MarkForClearingCommand(new[] { first.Id }));
            TestGrids.StepUntil(world, () => first.IsRemoved, 3000, "le premier arbre retiré");

            Assert.AreEqual(second.Occupant, world.Grid.OccupantAt(shared), "La maille du second reste à lui.");
            Assert.IsTrue(world.Grid.IsWalkable(world.Grid.CellOf(new Vector2(27.6f, 22f))),
                "Celles du premier se libèrent.");
            Assert.AreEqual(PlacementResult.NatureInTheWay,
                world.PlacementRules.Check(_town.Cabin, new Placement(new Vector2(30f, 22f), 0f)));
        }

        [Test]
        public void ASpotOutsideTheMapIsRefused()
        {
            Assert.Throws<System.ArgumentException>(() => TestData.FlatMap(40f, null, new[] { Spot(_town.Tree, 41f, 5f) }));
        }

        internal static NatureSpot Spot(NatureDefinition definition, float x, float z)
        {
            return new NatureSpot(definition, new Placement(new Vector2(x, z), 0f));
        }

        private static int[] IdsOf(IReadOnlyList<NatureItem> items)
        {
            var ids = new int[items.Count];
            for (int i = 0; i < ids.Length; i++)
                ids[i] = items[i].Id;
            return ids;
        }
    }
}
