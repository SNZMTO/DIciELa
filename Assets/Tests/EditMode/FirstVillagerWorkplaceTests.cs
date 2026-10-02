using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le lieu de travail d'un premier habitant (GDD, section 7 ; décision 191), par l'ordre « poser » : quand sa loge est
    /// bâtie, le poste « à tout faire » ouvert pour lui au campement se ferme et il prend un poste de sa loge ; une
    /// seconde loge n'y change rien ; si le joueur l'a mis ailleurs, il y reste ; sans poste libre dans sa loge, il
    /// redevient libre. Le banc de la ville au travail, son campement posé en (20 ; 14), une maison de deux lits, et la
    /// loge du bâtisseur, 4 × 4 m, dressée dès la pose pour que les tests n'aient pas à la bâtir, avec deux postes
    /// d'ouvrier.
    /// </summary>
    public sealed class FirstVillagerWorkplaceTests
    {
        private TestTown _town;
        private StructureDefinition _house;
        private StructureDefinition _lodge;
        private WorkSlotDefinition _worker;
        private World _world;
        private Structure _camp;
        private Villager _builder;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            FirstVillagerDefinition builder = _town.Keep(TestData.FirstVillager("batisseur", _town.Build.SubStatistic,
                _town.Keep(TestData.Character("homme"))));
            _worker = _town.Keep(TestData.WorkSlot("ouvrier", _town.Build));
            _lodge = _town.Keep(TestData.House("loge", 4f, 0));
            TestData.Posts(_lodge, _worker, _worker);
            TestData.Workplace(builder, _lodge);
            TestData.FirstVillagers(_town.Opening, builder);
            TestData.FirstVillagersSlot(_town.Opening, _town.OddJobs);
            TestData.Ordinary(_town.Catalog, _town.Keep(TestData.OrdinaryKind()));
            _house = _town.KeepKnown(TestData.House("maison", 4f, 2));
            _world = _town.World();
            _camp = TestTown.Place(_world, _town.Camp, 20f, 14f);
            TestTown.Place(_world, _house, 30f, 20f);
            _builder = _world.Villagers.All[1];
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void WhenHisLodgeIsBuiltHeLeavesHisOddJobsPostForIt()
        {
            Assert.AreSame(_camp, _builder.Slot.Workplace, "En attendant sa loge, il est au campement.");

            Structure lodge = TestTown.Place(_world, _lodge, 10f, 28f);

            Assert.AreSame(lodge, _builder.Slot.Workplace, "Sa loge bâtie, il y prend un poste.");
            Assert.AreSame(_worker, _builder.Slot.Definition);
            Assert.AreEqual(1, PostsOf(lodge).FindAll(slot => slot.Holder == null).Count, "L'autre poste reste libre.");
            Assert.AreEqual(1, PostsOf(_camp).Count, "Le poste ouvert pour lui au campement s'est fermé.");
        }

        [Test]
        public void ASecondLodgeChangesNothing()
        {
            Structure first = TestTown.Place(_world, _lodge, 10f, 28f);

            TestTown.Place(_world, _lodge, 30f, 32f);

            Assert.AreSame(first, _builder.Slot.Workplace, "Il reste à la première.");
        }

        [Test]
        public void PutElsewhereByThePlayerHeStaysThere()
        {
            // Le joueur le met à l'atelier : le poste ouvert pour lui au campement se libère.
            StructureDefinition workshop = _town.KeepKnown(TestData.House("atelier", 3f, 0));
            TestData.Posts(workshop, _worker);
            Structure placed = TestTown.Place(_world, workshop, 10f, 10f);
            TestTown.Send(_world, new AssignVillagerCommand(_builder.Id, placed.Id));
            WorkSlot chosen = _builder.Slot;
            Assert.AreSame(placed, chosen.Workplace);

            TestTown.Place(_world, _lodge, 10f, 28f);

            Assert.AreSame(chosen, _builder.Slot, "Mis ailleurs par le joueur, il y reste.");
            Assert.AreEqual(1, PostsOf(_camp).Count, "Le poste ouvert pour lui, libre, s'est fermé.");
        }

        [Test]
        public void WithoutAFreePostInHisLodgeHeIsFree()
        {
            TestData.Posts(_lodge);

            TestTown.Place(_world, _lodge, 10f, 28f);

            Assert.IsNull(_builder.Slot, "Sa loge n'a pas de poste : il redevient libre, et flâne.");
            Assert.AreEqual(1, PostsOf(_camp).Count, "Le poste ouvert pour lui s'est fermé.");
        }

        private List<WorkSlot> PostsOf(Structure place)
        {
            var posts = new List<WorkSlot>();
            foreach (WorkSlot slot in _world.WorkSlots.All)
            {
                if (ReferenceEquals(slot.Workplace, place))
                    posts.Add(slot);
            }
            return posts;
        }
    }
}
