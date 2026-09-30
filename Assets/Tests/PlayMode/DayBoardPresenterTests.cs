using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le présentateur de la journée (GDD, sections 3 et 16) sur un petit monde en mémoire, aux réglages par défaut :
    /// il écrit le jour et la phase quand ils changent, donne à la jauge les bornes des phases lues dans les réglages, et
    /// place le repère entre deux pas. Les textes sont ceux du test.
    /// </summary>
    public sealed class DayBoardPresenterTests
    {
        private const string Texts = "clé,fr\njournee.jour,Jour {jour}\njournee.Work,Travail\njournee.Evening,Soirée\n" +
                                     "journee.Night,Nuit\n";

        private SimulationSettings _settings;
        private Catalog _catalog;
        private World _world;
        private NotedDayBoardView _view;
        private DayBoardPresenter _presenter;

        [SetUp]
        public void CreateDay()
        {
            _settings = ScriptableObject.CreateInstance<SimulationSettings>();
            var map = new MapDescription(new Relief(100f, 100f, 2, 2, new[] { 10f, 10f, 10f, 10f }));
            _catalog = ScriptableObject.CreateInstance<Catalog>();
            _world = new World(_settings, map, _catalog);
            _view = new NotedDayBoardView();
            _presenter = new DayBoardPresenter(_view, _world.Clock, TextTable.Parse(Texts, "fr"));
        }

        [TearDown]
        public void DestroyDay()
        {
            _presenter.Dispose();
            Object.Destroy(_settings);
            Object.Destroy(_catalog);
        }

        [Test]
        public void TheFirstDayOpensAtWork()
        {
            Assert.AreEqual("Jour 1", _view.Day);
            Assert.AreEqual("Travail", _view.Phase);
            float step = 1f / _world.Clock.StepsPerDay;
            Assert.AreEqual(_settings.WorkFraction, _view.WorkEnd, step, "La fin du travail vient des réglages.");
            Assert.AreEqual(_settings.WorkFraction + _settings.EveningFraction, _view.EveningEnd, step,
                "La fin de la soirée vient des réglages.");
        }

        [Test]
        public void TheEveningIsWrittenWhenWorkEnds()
        {
            long bound = _world.Clock.StepsPerDay;
            for (long i = 0; i < bound && _world.Clock.Phase == DayPhase.Work; i++)
                _world.Step();
            Assert.AreEqual(DayPhase.Evening, _world.Clock.Phase, "Le travail finit dans la journée.");
            Assert.AreEqual("Soirée", _view.Phase);
            Assert.AreEqual("Jour 1", _view.Day);
        }

        [Test]
        public void ANewDayIsWrittenAfterTheNight()
        {
            long bound = _world.Clock.StepsPerDay;
            for (long i = 0; i < bound; i++)
                _world.Step();
            Assert.AreEqual("Jour 2", _view.Day);
            Assert.AreEqual("Travail", _view.Phase);
        }

        [Test]
        public void TheMarkMovesBetweenSteps()
        {
            _presenter.Follow(0.5f);
            Assert.AreEqual(0.5f / _world.Clock.StepsPerDay, _view.Progress, 1e-6f);
            _world.Step();
            _presenter.Follow(0f);
            Assert.AreEqual(1f / _world.Clock.StepsPerDay, _view.Progress, 1e-6f);
        }
    }
}
