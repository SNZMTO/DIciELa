using System;
using Diciela.Data;
using Diciela.Simulation.Core;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// L'accumulateur du pas fixe : un pas de 0,1 s, au plus 10 pas par image. Le pas vient d'un float de la fiche
    /// (0,1f vaut un peu plus que 0,1) : les temps choisis ici restent loin des frontières entre deux pas.
    /// </summary>
    public sealed class FixedStepAccumulatorTests
    {
        private SimulationSettings _settings;

        [SetUp]
        public void CreateSettings()
        {
            _settings = TestData.Settings(stepSeconds: 0.1f, dayLengthSeconds: 10f, workFraction: 0.5f,
                eveningFraction: 0.25f, maxStepsPerFrame: 10);
        }

        [TearDown]
        public void DestroySettings()
        {
            UnityEngine.Object.DestroyImmediate(_settings);
        }

        [Test]
        public void PlaysOneStepPerStepOfRealTime()
        {
            var accumulator = new FixedStepAccumulator(_settings);

            Assert.AreEqual(2, accumulator.Accumulate(0.25));
        }

        [Test]
        public void KeepsTheRemainderForTheNextFrame()
        {
            var accumulator = new FixedStepAccumulator(_settings);

            Assert.AreEqual(0, accumulator.Accumulate(0.05));
            Assert.AreEqual(1, accumulator.Accumulate(0.06));
        }

        [Test]
        public void TheFractionOfTheNextStepIsWhatRemains()
        {
            // 0,25 s : deux pas joués, 0,05 s en attente, la moitié du pas suivant. Le retard abandonné ne laisse rien.
            var accumulator = new FixedStepAccumulator(_settings);
            Assert.AreEqual(0f, accumulator.StepFraction);

            Assert.AreEqual(2, accumulator.Accumulate(0.25));
            Assert.AreEqual(0.5f, accumulator.StepFraction, 1e-4f);

            Assert.AreEqual(10, accumulator.Accumulate(5.0));
            Assert.AreEqual(0f, accumulator.StepFraction);
        }

        [Test]
        public void NeverPlaysMoreThanTheBoundAndDropsTheBacklog()
        {
            var accumulator = new FixedStepAccumulator(_settings);

            Assert.AreEqual(10, accumulator.Accumulate(5.0));
            Assert.AreEqual(0, accumulator.Accumulate(0.05));
            Assert.AreEqual(1, accumulator.Accumulate(0.06));
        }

        [Test]
        public void IgnoresNegativeOrUndefinedTime()
        {
            var accumulator = new FixedStepAccumulator(_settings);

            Assert.AreEqual(0, accumulator.Accumulate(-1.0));
            Assert.AreEqual(0, accumulator.Accumulate(double.NaN));
            Assert.AreEqual(1, accumulator.Accumulate(0.15));
        }

        [Test]
        public void TenSecondsGiveAHundredStepsAtAnyFrameRate()
        {
            int at30 = StepsDuring(10f, 30);
            int at144 = StepsDuring(10f, 144);

            Assert.That(at30, Is.InRange(99, 100));
            Assert.That(at144, Is.InRange(99, 100));
        }

        [Test]
        public void RefusesSettingsThatCannotStep()
        {
            Assert.Throws<ArgumentNullException>(() => new FixedStepAccumulator(null));

            SimulationSettings noStep = TestData.Settings(0f, 10f, 0.5f, 0.25f);
            SimulationSettings noFrame = TestData.Settings(0.1f, 10f, 0.5f, 0.25f, maxStepsPerFrame: 0);
            try
            {
                Assert.Throws<ArgumentException>(() => new FixedStepAccumulator(noStep));
                Assert.Throws<ArgumentException>(() => new FixedStepAccumulator(noFrame));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(noStep);
                UnityEngine.Object.DestroyImmediate(noFrame);
            }
        }

        /// <summary>Joue <paramref name="seconds"/> secondes en images égales, comme Time.deltaTime (un float).</summary>
        private int StepsDuring(float seconds, int framesPerSecond)
        {
            var accumulator = new FixedStepAccumulator(_settings);
            float frameSeconds = 1f / framesPerSecond;
            int frames = (int)(seconds * framesPerSecond);
            int steps = 0;
            for (int i = 0; i < frames; i++)
                steps += accumulator.Accumulate(frameSeconds);
            return steps;
        }
    }
}
