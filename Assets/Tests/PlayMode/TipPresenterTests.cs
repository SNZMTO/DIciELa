using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'astuce et son présentateur (GDD, section 16 ; décision 154), sur une vue qui note : c'est le jeu qui dit quoi faire
    /// ensuite ; installer le campement, puis y mettre la Chimère, puis poser une cabane ; ensuite, plus rien. Retirée de
    /// son poste, la Chimère y est renvoyée.
    /// </summary>
    public sealed class TipPresenterTests
    {
        private const string Texts = "clé,fr\nastuce.campement,Campement\nastuce.poste,Poste\nastuce.cabane,Cabane\n";

        private sealed class NotedView : ITipView
        {
            public string Tip { get; private set; } = "?";

            public void ShowTip(string text)
            {
                Tip = text;
            }
        }

        private TestCampTown _town;
        private NotedView _view;
        private TipPresenter _presenter;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestCampTown();
            _view = new NotedView();
            _presenter = new TipPresenter(_view, _town.World, TextTable.Parse(Texts, "fr"));
        }

        [TearDown]
        public void DestroyTown()
        {
            _presenter.Dispose();
            _town.Dispose();
        }

        private void Send(Command command)
        {
            _town.World.Submit(command);
            _town.World.Step();
        }

        [Test]
        public void TheTipSpeaksAsSoonAsTheChimeraArrives()
        {
            using (var town = new TestCampTown(withChimera: false))
            {
                var view = new NotedView();
                using (new TipPresenter(view, town.World, TextTable.Parse(Texts, "fr")))
                {
                    Assert.AreEqual("", view.Tip, "Avant la Chimère, rien à dire.");
                    town.CreateTheChimera();
                    Assert.AreEqual("Campement", view.Tip, "Elle arrive : installer le campement.");
                }
            }
        }

        [Test]
        public void TheTipLeadsFromTheCampToTheFirstCabinThenFallsSilent()
        {
            Assert.AreEqual("Campement", _view.Tip);

            _town.InstallTheCamp();
            Assert.AreEqual("Poste", _view.Tip);

            int chimera = _town.World.Villagers.Chimera.Id;
            Send(new AssignVillagerCommand(chimera, _town.PlacedCamp.Id));
            Assert.AreEqual("Cabane", _view.Tip);

            Send(new UnassignVillagerCommand(chimera));
            Assert.AreEqual("Poste", _view.Tip, "Retirée de son poste, l'astuce l'y renvoie.");

            Send(new AssignVillagerCommand(chimera, _town.PlacedCamp.Id));
            Send(new PlaceStructureCommand(_town.Cabin(new GoodAmount(_town.Wood, 10)),
                new Placement(new Vector2(28f, 20f), 0f)));
            Assert.AreEqual("", _view.Tip, "Une cabane posée : il n'y a plus rien à dire.");
        }
    }
}
