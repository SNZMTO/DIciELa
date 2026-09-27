using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// L'horloge, par le chemin du jeu : un monde sur une petite carte plate, qui joue ses pas. Une journée de 10 s à
    /// 0,1 s le pas fait 100 pas : moitié de travail (50 pas), un quart de soirée (25 pas), le reste de nuit. Les
    /// valeurs attendues sont écrites à la main.
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

        private World NewWorld(float step = 0.1f, float day = 10f, float work = 0.5f, float evening = 0.25f)
        {
            SimulationSettings settings = TestData.Settings(step, day, work, evening);
            _created.Add(settings);
            return new World(settings, TestData.FlatMap());
        }

        private static void PlaySteps(World world, int steps)
        {
            for (int i = 0; i < steps; i++)
                world.Step();
        }

        [Test]
        public void StartsOnDayOneAtWork()
        {
            Clock clock = NewWorld().Clock;

            Assert.AreEqual(1, clock.Day);
            Assert.AreEqual(DayPhase.Work, clock.Phase);
            Assert.AreEqual(0.0, clock.HourOfDay);
            Assert.AreEqual(100, clock.StepsPerDay);
        }

        [Test]
        public void PhasesFollowEachOtherAtTheirSteps()
        {
            World world = NewWorld();
            var phases = new List<DayPhase>();
            world.Clock.PhaseChanged += phases.Add;

            PlaySteps(world, 49);
            Assert.AreEqual(DayPhase.Work, world.Clock.Phase);
            Assert.IsEmpty(phases);

            PlaySteps(world, 1);
            Assert.AreEqual(new[] { DayPhase.Evening }, phases);

            PlaySteps(world, 25);
            Assert.AreEqual(new[] { DayPhase.Evening, DayPhase.Night }, phases);

            PlaySteps(world, 25);
            Assert.AreEqual(new[] { DayPhase.Evening, DayPhase.Night, DayPhase.Work }, phases);
        }

        [Test]
        public void NewDayIsAnnouncedBeforeTheWorkPhase()
        {
            World world = NewWorld();
            var log = new List<string>();
            world.Clock.DayStarted += day => log.Add("jour " + day);
            world.Clock.PhaseChanged += phase => log.Add(phase.ToString());

            PlaySteps(world, 100);

            Assert.AreEqual(new[] { "Evening", "Night", "jour 2", "Work" }, log);
            Assert.AreEqual(2, world.Clock.Day);
        }

        [Test]
        public void TenDaysDoNotDrift()
        {
            World world = NewWorld();
            int daysStarted = 0;
            world.Clock.DayStarted += _ => daysStarted++;

            PlaySteps(world, 1000);

            Assert.AreEqual(11, world.Clock.Day);
            Assert.AreEqual(10, daysStarted);
            Assert.AreEqual(0.0, world.Clock.HourOfDay);
        }

        [Test]
        public void HourOfDayCountsFromTheStartOfWork()
        {
            World world = NewWorld();

            PlaySteps(world, 25);

            Assert.AreEqual(6.0, world.Clock.HourOfDay, 1e-9);
        }

        [Test]
        public void ADayOfRealTimeIsTwentyFourGameHours()
        {
            Clock clock = NewWorld().Clock;

            Assert.AreEqual(24.0, clock.RealSecondsToGameHours(10.0), 1e-6);
            Assert.AreEqual(12.0, clock.RealSecondsToGameHours(5.0), 1e-6);
        }

        [Test]
        public void ADayIsAWholeNumberOfSteps()
        {
            Clock clock = NewWorld(step: 0.3f, day: 1f, work: 0.5f, evening: 0f).Clock;

            Assert.AreEqual(3, clock.StepsPerDay);
        }

        [Test]
        public void RefusesWorkAndEveningLongerThanTheDay()
        {
            Assert.Throws<ArgumentException>(() => NewWorld(work: 0.8f, evening: 0.3f));
        }

        [Test]
        public void RefusesADayShorterThanAStep()
        {
            Assert.Throws<ArgumentException>(() => NewWorld(step: 5f, day: 1f));
        }
    }
}
