using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La fiche d'un lieu et son présentateur (GDD, section 16 ; décisions 141 et 152), sur une vue qui note : toucher le
    /// tas de départ ouvre sa fiche (son nom, son Bois et sa Pierre, son poste libre, « Y mettre la Chimère ») ; le bouton
    /// y met la Chimère, puis l'en retire, par les ordres ; toucher autre chose, ou rien, la ferme, comme la croix ; la
    /// fiche suit le tas quand la Chimère y prend du Bois.
    /// </summary>
    public sealed class PlaceSheetPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "construction.tas_de_depart,Tas de départ\n" +
                                     "marchandise.bois,Bois\n" +
                                     "marchandise.pierre,Pierre\n" +
                                     "poste.a_tout_faire,À tout faire\n" +
                                     "tache.porter,Porter\n" +
                                     "tache.defricher,Défricher\n" +
                                     "fiche.mettre_chimere,Y mettre la Chimère\n" +
                                     "fiche.retirer_chimere,Retirer la Chimère\n" +
                                     "fiche.la_chimere,La Chimère\n" +
                                     "fiche.poste_libre,Libre\n";

        private TestPileTown _town;
        private NotedPlaceSheetView _view;
        private PlaceSheetPresenter _presenter;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestPileTown();
            _town.PlantTheTent();
            _view = new NotedPlaceSheetView();
            _presenter = new PlaceSheetPresenter(_view, _town.World, TextTable.Parse(Texts, "fr"));
        }

        [TearDown]
        public void DestroyTown()
        {
            _presenter.Dispose();
            _town.Dispose();
        }

        private World World => _town.World;

        [Test]
        public void TouchingThePileOpensItsSheet()
        {
            Assert.IsFalse(_view.Shown, "Fermée tant qu'on n'a rien touché.");
            _presenter.Touch(_town.PlacedPile);

            Assert.IsTrue(_view.Shown);
            Assert.AreEqual("Tas de départ", _view.Title);
            Assert.AreEqual(2, _view.Lines.Count);
            Assert.AreEqual("Bois", _view.Lines[0].Name);
            Assert.AreEqual(30, _view.Lines[0].Count);
            Assert.AreEqual("Pierre", _view.Lines[1].Name);
            Assert.AreEqual(20, _view.Lines[1].Count);
            Assert.AreEqual("À tout faire", _view.SlotName);
            Assert.AreEqual(new[] { "Porter", "Défricher" }, _view.Tasks, "Les tâches du poste, dans son ordre.");
            Assert.AreEqual("Libre", _view.SlotHolder);
            Assert.AreEqual("Y mettre la Chimère", _view.Button);
        }

        [Test]
        public void TheButtonPutsTheChimeraThereThenTakesHerAway()
        {
            _presenter.Touch(_town.PlacedPile);
            _view.ClickButton();
            World.Step();
            Assert.AreEqual("La Chimère", _view.SlotHolder);
            Assert.AreEqual("Retirer la Chimère", _view.Button);

            _view.ClickButton();
            World.Step();
            Assert.AreEqual("Libre", _view.SlotHolder);
            Assert.AreEqual("Y mettre la Chimère", _view.Button);
            Assert.IsNull(World.Villagers.Chimera.Slot);
        }

        [Test]
        public void TouchingSomethingElseOrNothingClosesIt()
        {
            _presenter.Touch(_town.PlacedPile);
            _presenter.Touch(null);
            Assert.IsFalse(_view.Shown, "Toucher le sol ferme la fiche.");

            _presenter.Touch(_town.PlacedPile);
            _presenter.Touch(World.Structures.ChimeraHome);
            Assert.IsFalse(_view.Shown, "La tente n'est pas un lieu de travail.");

            _presenter.Touch(_town.PlacedPile);
            _view.ClickClose();
            Assert.IsFalse(_view.Shown, "La croix la ferme.");
            Assert.IsFalse(_presenter.Close(), "Déjà fermée : Échap passe à la suite.");
        }

        [Test]
        public void TheSheetFollowsThePile()
        {
            _presenter.Touch(_town.PlacedPile);
            World.Submit(new AssignVillagerCommand(World.Villagers.Chimera.Id, _town.PlacedPile.Id));
            World.Submit(new PlaceStructureCommand(_town.Cabin(new GoodAmount(_town.Wood, 10)),
                new Placement(new Vector2(28f, 20f), 0f)));
            for (int step = 0; step < 3000 && _view.Lines[0].Count == 30; step++)
                World.Step();

            Assert.AreEqual(25, _view.Lines[0].Count, "La Chimère a pris 5 Bois : la fiche le montre.");
        }
    }
}
