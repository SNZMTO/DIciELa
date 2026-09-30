using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    public sealed class WorldTests
    {
        private SimulationSettings _settings;

        [SetUp]
        public void CreateSettings()
        {
            _settings = TestData.Settings(stepSeconds: 0.1f, dayLengthSeconds: 10f, workFraction: 0.5f,
                eveningFraction: 0.25f);
        }

        [TearDown]
        public void DestroySettings()
        {
            UnityEngine.Object.DestroyImmediate(_settings);
        }

        [Test]
        public void AStepAdvancesTheClockByOneStep()
        {
            var world = new World(_settings, TestData.FlatMap(), TestData.EmptyCatalog);

            world.Step();
            world.Step();

            Assert.AreEqual(2, world.Clock.StepCount);
        }

        [Test]
        public void KeepsTheMapItReceived()
        {
            var map = TestData.FlatMap();

            var world = new World(_settings, map, TestData.EmptyCatalog);

            Assert.AreSame(map, world.Map);
        }

        [Test]
        public void TheEventsOfTheOrdersLeaveOnceAllOrdersArePlayed()
        {
            // Trois poses dans le même pas, la deuxième sur la première : chaque événement part à la fin de la couche des
            // ordres, dans l'ordre des ordres (ARCHITECTURE, section 5). Tous voient donc les deux constructions posées.
            StructureDefinition block = TestData.Structure("bloc", 2f, 2f, hasDoor: false);
            try
            {
                var world = new World(_settings, TestData.FlatMap(), TestData.EmptyCatalog);
                var heard = new List<string>();
                world.Structures.StructurePlaced +=
                    structure => heard.Add("posée " + structure.Id + ", " + world.Structures.All.Count);
                world.Structures.PlacementRefused +=
                    (definition, placement, reason) => heard.Add("refusée, " + world.Structures.All.Count);

                world.Submit(new PlaceStructureCommand(block, new Placement(new Vector2(3f, 3f), 0f)));
                world.Submit(new PlaceStructureCommand(block, new Placement(new Vector2(3.5f, 3f), 0f)));
                world.Submit(new PlaceStructureCommand(block, new Placement(new Vector2(7f, 7f), 0f)));
                world.Step();

                Assert.AreEqual(new[] { "posée 1, 2", "refusée, 2", "posée 2, 2" }, heard);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(block);
            }
        }

        [Test]
        public void RefusesMissingSettingsMapOrCatalog()
        {
            Assert.Throws<ArgumentNullException>(() => new World(null, TestData.FlatMap(), TestData.EmptyCatalog));
            Assert.Throws<ArgumentNullException>(() => new World(_settings, null, TestData.EmptyCatalog));
            Assert.Throws<ArgumentNullException>(() => new World(_settings, TestData.FlatMap(), null));
        }
    }
}
