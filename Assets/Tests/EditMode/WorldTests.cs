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
            var world = new World(_settings);

            world.Step();
            world.Step();

            Assert.AreEqual(2, world.Clock.StepCount);
        }

        [Test]
        public void RefusesMissingSettings()
        {
            Assert.Throws<ArgumentNullException>(() => new World(null));
        }
    }
}
