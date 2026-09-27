using System;
using Diciela.Data;
using Diciela.Simulation;
using NUnit.Framework;

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
            var world = new World(_settings, TestData.FlatMap());

            world.Step();
            world.Step();

            Assert.AreEqual(2, world.Clock.StepCount);
        }

        [Test]
        public void KeepsTheMapItReceived()
        {
            var map = TestData.FlatMap();

            var world = new World(_settings, map);

            Assert.AreSame(map, world.Map);
        }

        [Test]
        public void RefusesMissingSettingsOrMap()
        {
            Assert.Throws<ArgumentNullException>(() => new World(null, TestData.FlatMap()));
            Assert.Throws<ArgumentNullException>(() => new World(_settings, null));
        }
    }
}
