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
    /// ensuite ; planter la tente, puis mettre la Chimère au tas, puis poser une cabane ; ensuite, plus rien. Retirée du
    /// tas, la Chimère y est renvoyée.
    /// </summary>
    public sealed class TipPresenterTests
    {
        private const string Texts = "clé,fr\nastuce.tente,Tente\nastuce.tas,Tas\nastuce.cabane,Cabane\n";

        private sealed class NotedView : ITipView
        {
            public string Tip { get; private set; } = "?";

            public void ShowTip(string text)
            {
                Tip = text;
            }
        }

        private TestPileTown _town;
        private NotedView _view;
        private TipPresenter _presenter;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestPileTown();
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
            using (var town = new TestPileTown(withChimera: false))
            {
                var view = new NotedView();
                using (new TipPresenter(view, town.World, TextTable.Parse(Texts, "fr")))
                {
                    Assert.AreEqual("", view.Tip, "Avant la Chimère, rien à dire.");
                    town.CreateTheChimera();
                    Assert.AreEqual("Tente", view.Tip, "Elle arrive : planter la tente.");
                }
            }
        }

        [Test]
        public void TheTipLeadsFromTheTentToTheFirstCabinThenFallsSilent()
        {
            Assert.AreEqual("Tente", _view.Tip);

            _town.PlantTheTent();
            Assert.AreEqual("Tas", _view.Tip);

            int chimera = _town.World.Villagers.Chimera.Id;
            Send(new AssignVillagerCommand(chimera, _town.PlacedPile.Id));
            Assert.AreEqual("Cabane", _view.Tip);

            Send(new UnassignVillagerCommand(chimera));
            Assert.AreEqual("Tas", _view.Tip, "Retirée du tas, l'astuce l'y renvoie.");

            Send(new AssignVillagerCommand(chimera, _town.PlacedPile.Id));
            Send(new PlaceStructureCommand(_town.Cabin(new GoodAmount(_town.Wood, 10)),
                new Placement(new Vector2(28f, 20f), 0f)));
            Assert.AreEqual("", _view.Tip, "Une cabane posée : il n'y a plus rien à dire.");
        }
    }
}
