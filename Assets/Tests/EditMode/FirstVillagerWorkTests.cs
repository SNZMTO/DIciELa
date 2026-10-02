using System.Collections.Generic;
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
    /// Le travail des premiers habitants (GDD, sections 4, 6 et 7 ; décision 189), par l'ordre « poser » et le temps qui
    /// passe : à son arrivée, chacun prend un poste « à tout faire » ouvert pour lui au campement (celui du campement
    /// reste à la Chimère), et le lit libre le plus proche de ce poste ; il y travaille sans elle, garde son poste la nuit
    /// et le reprend au matin ; sans campement, il n'a pas de poste. Le banc de la ville au travail (une heure : 300 pas),
    /// son campement posé en (20 ; 14), et une maison de 4 m qui offre deux lits, dressée dès la pose.
    /// </summary>
    public sealed class FirstVillagerWorkTests
    {
        private const int MaxSteps = 20000;
        private TestTown _town;
        private StructureDefinition _house;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            FirstVillagerDefinition builder = _town.Keep(TestData.FirstVillager("batisseur", _town.Build.SubStatistic,
                _town.Keep(TestData.Character("homme"))));
            FirstVillagerDefinition carter = _town.Keep(TestData.FirstVillager("charretiere", _town.Carry.SubStatistic,
                _town.Keep(TestData.Character("femme"))));
            TestData.FirstVillagers(_town.Opening, builder, carter);
            TestData.FirstVillagersSlot(_town.Opening, _town.OddJobs);
            TestData.Ordinary(_town.Catalog, _town.Keep(TestData.OrdinaryKind()));
            _house = _town.KeepKnown(TestData.House("maison", 4f, 2));
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void ArrivingHeTakesAnOddJobsPostOpenedForHimAtTheCamp()
        {
            World world = _town.World();
            Structure camp = TestTown.Place(world, _town.Camp, 20f, 14f);

            TestTown.Place(world, _house, 30f, 20f);

            Villager builder = world.Villagers.All[1];
            Assert.AreEqual("habitant.batisseur", builder.NameKey, "Son nom, en attendant le sien : son métier.");
            Assert.IsNotNull(builder.Slot, "Il a un poste dès son arrivée.");
            Assert.AreSame(camp, builder.Slot.Workplace, "Au campement.");
            Assert.AreSame(_town.OddJobs, builder.Slot.Definition, "Le poste « à tout faire ».");
            List<WorkSlot> posts = PostsOf(world, camp);
            Assert.AreEqual(2, posts.Count, "Le poste du campement, et celui ouvert pour lui.");
            Assert.IsNull(posts[0].Holder, "Celui du campement reste à la Chimère, libre tant qu'elle n'y est pas.");
        }

        [Test]
        public void HeBuildsWithoutTheChimera()
        {
            World world = _town.World();
            TestTown.Place(world, _town.Camp, 20f, 14f);
            TestTown.Place(world, _house, 30f, 20f);

            Structure cabin = TestTown.Place(world, _town.Cabin, 12f, 24f);
            RunUntil(world, () => cabin.State == StructureState.Built);

            Assert.IsNull(world.Villagers.Chimera.Slot, "La Chimère n'a pas de poste : ce sont eux qui ont bâti.");
        }

        [Test]
        public void HeSleepsInTheHouseNearestHisPost()
        {
            // La maison A est plus près du campement, la maison B plus près du point d'arrivée, le centre (20 ; 20).
            World world = _town.World();
            TestTown.Place(world, _town.Camp, 20f, 14f);
            world.Submit(new PlaceStructureCommand(_house, new Placement(new Vector2(27f, 9f), 0f)));
            world.Submit(new PlaceStructureCommand(_house, new Placement(new Vector2(20f, 31f), 0f)));
            world.Step();

            Structure nearTheCamp = world.Structures.All[1];
            Assert.AreEqual(new Vector2(27f, 9f), nearTheCamp.Placement.Center);
            Assert.AreSame(nearTheCamp, world.Housing.HouseOf(world.Villagers.All[1]), "Au plus près de son poste.");
        }

        [Test]
        public void WithoutACampHeHasNoPost()
        {
            World world = _town.World();

            TestTown.Place(world, _house, 30f, 20f);

            Assert.IsNull(world.Villagers.All[1].Slot, "Pas de logis de la Chimère, pas de poste ouvert pour lui.");
        }

        [Test]
        public void HeKeepsHisPostThroughTheNight()
        {
            World world = _town.World();
            TestTown.Place(world, _town.Camp, 20f, 14f);
            TestTown.Place(world, _house, 30f, 20f);
            Villager builder = world.Villagers.All[1];
            WorkSlot post = builder.Slot;

            RunUntil(world, () => builder.IsAsleep);
            Assert.AreSame(post, builder.Slot, "Endormi, il garde son poste.");
            RunUntil(world, () => world.Clock.Phase == DayPhase.Work);
            Assert.AreSame(post, builder.Slot, "Au matin, il le reprend.");
        }

        private static List<WorkSlot> PostsOf(World world, Structure place)
        {
            var posts = new List<WorkSlot>();
            foreach (WorkSlot slot in world.WorkSlots.All)
            {
                if (ReferenceEquals(slot.Workplace, place))
                    posts.Add(slot);
            }
            return posts;
        }

        private static void RunUntil(World world, System.Func<bool> done)
        {
            for (int i = 0; i < MaxSteps && !done(); i++)
                world.Step();
            Assert.IsTrue(done(), "Le monde n'y est pas arrivé en " + MaxSteps + " pas.");
        }
    }
}
