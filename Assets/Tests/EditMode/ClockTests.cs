using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// L'horloge. Une journée de 10 s à 0,1 s le pas fait 100 pas : moitié de travail (50 pas), un quart de soirée
    /// (25 pas), le reste de nuit. Les valeurs attendues sont écrites à la main.
    /// </summary>
    public sealed class ClockTests
    {
        private readonly List<ScriptableObject> _created = new List<ScriptableObject>();

        [TearDown]
        public void DestroyCreatedAssets()
        {
            foreach (ScriptableObject asset in _created)
                UnityEngine.Object.DestroyImmediate(asset);
            _created.Clear();
        }

        private SimulationSettings Settings(float step = 0.1f, float day = 10f, float work = 0.5f, float evening = 0.25f)
        {
            SimulationSettings settings = TestData.Settings(step, day, work, evening);
            _created.Add(settings);
            return settings;
        }

        private static void AdvanceSteps(Clock clock, int steps)
        {
            for (int i = 0; i < steps; i++)
                clock.Advance();
        }

        [Test]
        public void StartsOnDayOneAtWork()
        {
            var clock = new Clock(Settings());

            Assert.AreEqual(1, clock.Day);
            Assert.AreEqual(DayPhase.Work, clock.Phase);
            Assert.AreEqual(0.0, clock.HourOfDay);
            Assert.AreEqual(100, clock.StepsPerDay);
        }

        [Test]
        public void PhasesFollowEachOtherAtTheirSteps()
        {
            var clock = new Clock(Settings());
            var phases = new List<DayPhase>();
            clock.PhaseChanged += phases.Add;

            AdvanceSteps(clock, 49);
            Assert.AreEqual(DayPhase.Work, clock.Phase);
            Assert.IsEmpty(phases);

            AdvanceSteps(clock, 1);
            Assert.AreEqual(new[] { DayPhase.Evening }, phases);

            AdvanceSteps(clock, 25);
            Assert.AreEqual(new[] { DayPhase.Evening, DayPhase.Night }, phases);

            AdvanceSteps(clock, 25);
            Assert.AreEqual(new[] { DayPhase.Evening, DayPhase.Night, DayPhase.Work }, phases);
        }

        [Test]
        public void NewDayIsAnnouncedBeforeTheWorkPhase()
        {
            var clock = new Clock(Settings());
            var log = new List<string>();
            clock.DayStarted += day => log.Add("jour " + day);
            clock.PhaseChanged += phase => log.Add(phase.ToString());

            AdvanceSteps(clock, 100);

            Assert.AreEqual(new[] { "Evening", "Night", "jour 2", "Work" }, log);
            Assert.AreEqual(2, clock.Day);
        }

        [Test]
        public void TenDaysDoNotDrift()
        {
            var clock = new Clock(Settings());
            int daysStarted = 0;
            clock.DayStarted += _ => daysStarted++;

            AdvanceSteps(clock, 1000);

            Assert.AreEqual(11, clock.Day);
            Assert.AreEqual(10, daysStarted);
            Assert.AreEqual(0.0, clock.HourOfDay);
        }

        [Test]
        public void HourOfDayCountsFromTheStartOfWork()
        {
            var clock = new Clock(Settings());

            AdvanceSteps(clock, 25);

            Assert.AreEqual(6.0, clock.HourOfDay, 1e-9);
        }

        [Test]
        public void ADayOfRealTimeIsTwentyFourGameHours()
        {
            var clock = new Clock(Settings());

            Assert.AreEqual(24.0, clock.RealSecondsToGameHours(10.0), 1e-6);
            Assert.AreEqual(12.0, clock.RealSecondsToGameHours(5.0), 1e-6);
        }

        [Test]
        public void ADayIsAWholeNumberOfSteps()
        {
            var clock = new Clock(Settings(step: 0.3f, day: 1f, work: 0.5f, evening: 0f));

            Assert.AreEqual(3, clock.StepsPerDay);
        }

        [Test]
        public void RefusesWorkAndEveningLongerThanTheDay()
        {
            Assert.Throws<ArgumentException>(() => new Clock(Settings(work: 0.8f, evening: 0.3f)));
        }

        [Test]
        public void RefusesADayShorterThanAStep()
        {
            Assert.Throws<ArgumentException>(() => new Clock(Settings(step: 5f, day: 1f)));
        }

        [Test]
        public void RefusesMissingSettings()
        {
            Assert.Throws<ArgumentNullException>(() => new Clock(null));
        }
    }
}
