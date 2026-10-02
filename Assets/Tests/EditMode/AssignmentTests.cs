using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Affecter et retirer (GDD, section 6 ; décision 141), par les ordres du joueur : la Chimère prend le poste du campement,
    /// le quitte ; un ordre qui ne peut rien faire est refusé et dit pourquoi, sans rien changer.
    /// </summary>
    public sealed class AssignmentTests
    {
        private readonly List<AssignmentRefusal> _refusals = new List<AssignmentRefusal>();
        private TestTown _town;
        private World _world;
        private Villager _chimera;
        private Structure _camp;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            _world = _town.World();
            TestTown.Place(_world, _town.Camp, 20f, 14f);
            _chimera = _world.Villagers.Chimera;
            _camp = TestTown.CampOf(_world);
            _refusals.Clear();
            _world.WorkSlots.AssignmentRefused += _refusals.Add;
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        private void Send(Command command)
        {
            _world.Submit(command);
            _world.Step();
        }

        [Test]
        public void TheChimeraTakesThePostThenLeavesIt()
        {
            var assigned = new List<WorkSlot>();
            var unassigned = new List<WorkSlot>();
            _world.WorkSlots.VillagerAssigned += (villager, slot) => assigned.Add(slot);
            _world.WorkSlots.VillagerUnassigned += (villager, slot) => unassigned.Add(slot);

            Send(new AssignVillagerCommand(_chimera.Id, _camp.Id));
            Assert.AreEqual(1, assigned.Count);
            Assert.AreSame(assigned[0], _chimera.Slot);
            Assert.AreSame(_chimera, assigned[0].Holder);

            Send(new UnassignVillagerCommand(_chimera.Id));
            Assert.AreEqual(1, unassigned.Count);
            Assert.IsNull(_chimera.Slot);
            Assert.IsNull(unassigned[0].Holder);
            Assert.IsEmpty(_refusals);
        }

        [Test]
        public void OrdersThatCannotBeDoneAreRefusedWithTheirReason()
        {
            // Un bloc sans poste : ce n'est pas un lieu de travail.
            Structure block = TestTown.Place(_world, _town.KeepKnown(TestData.Structure("bloc", 2f, 2f, hasDoor: false)), 30f, 30f);
            Send(new AssignVillagerCommand(_chimera.Id, block.Id));
            Send(new AssignVillagerCommand(_chimera.Id, 999));
            Send(new AssignVillagerCommand(999, _camp.Id));
            Send(new UnassignVillagerCommand(_chimera.Id));
            Send(new AssignVillagerCommand(_chimera.Id, _camp.Id));
            Send(new AssignVillagerCommand(_chimera.Id, _camp.Id));

            CollectionAssert.AreEqual(new[]
            {
                AssignmentRefusal.NotAWorkplace, AssignmentRefusal.NotAWorkplace, AssignmentRefusal.NoVillager,
                AssignmentRefusal.NotAssigned, AssignmentRefusal.AlreadyThere
            }, _refusals);
            Assert.IsNotNull(_chimera.Slot, "Le seul ordre valable l'a mise au campement.");
        }
    }
}
