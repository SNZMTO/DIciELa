using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les idées des premiers habitants (GDD, sections 6 et 13 ; décision 190), par l'ordre « poser » : le bâtisseur
    /// apporte l'idée de son lieu de travail, la loge ; avant lui, la ville ne sait pas la bâtir et sa pose est refusée ;
    /// dès son arrivée, elle la sait, le dit une fois, et la loge se pose. Une idée déjà sue ne s'apprend pas deux fois.
    /// Le banc de la ville au travail, son campement posé en (20 ; 14), une maison de deux lits, et une loge de 5 × 4 m.
    /// </summary>
    public sealed class FirstVillagerIdeasTests
    {
        private readonly List<StructureDefinition> _learned = new List<StructureDefinition>();
        private TestTown _town;
        private FirstVillagerDefinition _builder;
        private StructureDefinition _house;
        private StructureDefinition _lodge;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            _builder = _town.Keep(TestData.FirstVillager("batisseur", _town.Build.SubStatistic,
                _town.Keep(TestData.Character("homme"))));
            _lodge = _town.Keep(TestData.Structure("loge", 5f, 4f));
            TestData.Workplace(_builder, _lodge);
            TestData.FirstVillagers(_town.Opening, _builder);
            TestData.FirstVillagersSlot(_town.Opening, _town.OddJobs);
            TestData.Ordinary(_town.Catalog, _town.Keep(TestData.OrdinaryKind()));
            _house = _town.KeepKnown(TestData.House("maison", 4f, 2));
            _learned.Clear();
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        private World NewWorld()
        {
            World world = _town.World();
            world.Knowledge.Learned += structure => _learned.Add(structure);
            TestTown.Place(world, _town.Camp, 20f, 14f);
            return world;
        }

        [Test]
        public void TheBuilderBringsTheIdeaOfHisLodge()
        {
            World world = NewWorld();
            var refusals = new List<PlacementResult>();
            world.Structures.PlacementRefused += (definition, placement, reason) => refusals.Add(reason);

            Assert.IsNull(TestTown.Place(world, _lodge, 10f, 28f), "Avant lui, personne ne sait bâtir la loge.");
            Assert.AreEqual(new[] { PlacementResult.Unknown }, refusals);

            TestTown.Place(world, _house, 30f, 20f);
            Assert.IsTrue(world.Knowledge.Knows(_lodge), "Arrivé, le bâtisseur en apporte l'idée.");
            Assert.AreEqual(new[] { _lodge }, _learned, "La ville le dit, une fois.");
            Assert.IsNotNull(TestTown.Place(world, _lodge, 10f, 28f), "Et la loge se pose.");
        }

        [Test]
        public void AnIdeaTheTownAlreadyHasIsNotLearnedAgain()
        {
            TestData.AlsoKnows(_town.Opening, _lodge);
            World world = NewWorld();

            TestTown.Place(world, _house, 30f, 20f);

            Assert.AreEqual(2, world.Villagers.All.Count, "Le bâtisseur est arrivé.");
            Assert.IsEmpty(_learned, "La loge, la Chimère la savait déjà : rien de neuf.");
        }
    }
}
