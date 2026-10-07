using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La fiche d'un lieu et son présentateur (GDD, section 16 ; décisions 141, 152 et 172), sur une vue qui note : toucher
    /// le campement ouvre sa fiche (son nom, son Bois et sa Pierre, son poste libre, « Y mettre la Chimère » en menthe et
    /// « Aller dormir » en lavande) ; ses postes et qui les tient, chacun par son nom, quand un premier habitant y a le
    /// sien ; le premier bouton met la Chimère au poste, puis l'en retire ; le second l'envoie
    /// dormir, puis la réveille, même en chemin ; par les ordres. Un logis sans poste ni stock a sa fiche, pour dormir ;
    /// toucher autre chose, ou rien, la ferme, comme la croix ; la fiche suit le stock quand la Chimère y prend du Bois.
    /// </summary>
    public sealed class LocationSheetPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "construction.campement,Campement\n" +
                                     "marchandise.bois,Bois\n" +
                                     "marchandise.pierre,Pierre\n" +
                                     "poste.a_tout_faire,À tout faire\n" +
                                     "fiche.mettre_chimere,Y mettre la Chimère\n" +
                                     "fiche.retirer_chimere,Retirer la Chimère\n" +
                                     "habitant.chimere,La Chimère\n" +
                                     "habitant.batisseur,Le bâtisseur\n" +
                                     "fiche.poste_libre,Libre\n" +
                                     "fiche.dormir,Aller dormir\n" +
                                     "fiche.reveiller,Réveiller\n";

        private TestCampTown _town;
        private NotedLocationSheetView _view;
        private LocationSheetPresenter _presenter;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestCampTown();
            _town.InstallTheCamp();
            _view = new NotedLocationSheetView();
            _presenter = new LocationSheetPresenter(_view, _town.World, TextTable.Parse(Texts, "fr"));
        }

        [TearDown]
        public void DestroyTown()
        {
            _presenter.Dispose();
            _town.Dispose();
        }

        private World World => _town.World;

        [Test]
        public void TouchingTheCampOpensItsSheet()
        {
            Assert.IsFalse(_view.Shown, "Fermée tant qu'on n'a rien touché.");
            _presenter.Touch(_town.PlacedCamp);

            Assert.IsTrue(_view.Shown);
            Assert.AreEqual("Campement", _view.Title);
            Assert.AreEqual(2, _view.Lines.Count);
            Assert.AreEqual("Bois", _view.Lines[0].Name);
            Assert.AreEqual(30, _view.Lines[0].Count);
            Assert.AreEqual("Pierre", _view.Lines[1].Name);
            Assert.AreEqual(20, _view.Lines[1].Count);
            Assert.AreEqual(1, _view.Slots.Count);
            Assert.AreEqual("À tout faire", _view.Slots[0].Name);
            Assert.AreEqual("Libre", _view.Slots[0].Holder);
            Assert.AreEqual(new[] { "Y mettre la Chimère", "Aller dormir" }, _view.ActionTexts());
            Assert.AreEqual(Pastel.Menthe, _view.Actions[0].Pastel);
            Assert.AreEqual(Pastel.Lavande, _view.Actions[1].Pastel);
        }

        [Test]
        public void ThePostButtonPutsTheChimeraThereThenTakesHerAway()
        {
            _presenter.Touch(_town.PlacedCamp);
            Assert.IsTrue(_view.Click("Y mettre la Chimère"));
            World.Step();
            Assert.AreEqual(new[] { "La Chimère" }, _view.Holders());
            Assert.AreEqual("Retirer la Chimère", _view.ActionTexts()[0]);

            Assert.IsTrue(_view.Click("Retirer la Chimère"));
            World.Step();
            Assert.AreEqual(new[] { "Libre" }, _view.Holders());
            Assert.AreEqual("Y mettre la Chimère", _view.ActionTexts()[0]);
            Assert.IsNull(World.Villagers.Chimera.Slot);
        }

        [Test]
        public void TheSheetListsEveryPostAndWhoHoldsItByName()
        {
            // Le bâtisseur arrive avec sa maison : un poste « à tout faire » s'ouvre pour lui au campement (décision 189).
            World.Submit(new PlaceStructureCommand(_town.HouseWithABed(), new Placement(new Vector2(30f, 30f), 0f)));
            World.Step();
            _presenter.Touch(_town.PlacedCamp);
            Assert.IsTrue(_view.Click("Y mettre la Chimère"));
            World.Step();

            Assert.AreEqual(2, _view.Slots.Count, "Le poste du campement, et celui ouvert pour lui.");
            Assert.AreEqual("À tout faire", _view.Slots[1].Name);
            Assert.AreEqual(new[] { "La Chimère", "Le bâtisseur" }, _view.Holders(), "Chacun par son nom.");
        }

        [Test]
        public void TheSleepButtonSendsHerToBedThenWakesHerEvenOnTheWay()
        {
            _presenter.Touch(_town.PlacedCamp);
            Assert.IsTrue(_view.Click("Aller dormir"));
            World.Step();
            Assert.IsTrue(World.Villagers.Chimera.IsGoingToBed, "Elle part se coucher.");
            Assert.AreEqual("Réveiller", _view.ActionTexts()[1], "En chemin, on peut déjà la réveiller.");

            Assert.IsTrue(_view.Click("Réveiller"));
            World.Step();
            Assert.IsFalse(World.Villagers.Chimera.IsGoingToBed, "Réveillée en chemin, elle reste debout.");
            Assert.AreEqual("Aller dormir", _view.ActionTexts()[1]);
        }

        [Test]
        public void AHomeWithNeitherPostNorStockHasItsSheetToSleep()
        {
            // Un logis qui ne tient rien et n'ouvre aucun poste, comme le campement une fois son stock et son poste fermés.
            using (var town = new TestCampTown())
            {
#if UNITY_EDITOR
                TestWorld.Fill(town.Camp, "_storageCapacity", 0);
                TestWorld.FillList(town.Camp, "_workSlots");
#endif
                town.InstallTheCamp();
                var view = new NotedLocationSheetView();
                var presenter = new LocationSheetPresenter(view, town.World, TextTable.Parse(Texts, "fr"));
                presenter.Touch(town.PlacedCamp);

                Assert.IsTrue(view.Shown, "Sans sa fiche, on ne pourrait plus la coucher.");
                Assert.AreEqual(new[] { "Aller dormir" }, view.ActionTexts());
                presenter.Dispose();
            }
        }

        [Test]
        public void TouchingSomethingElseOrNothingClosesIt()
        {
            _presenter.Touch(_town.PlacedCamp);
            _presenter.Touch(null);
            Assert.IsFalse(_view.Shown, "Toucher le sol ferme la fiche.");

            StructureDefinition cabin = _town.Cabin();
#if UNITY_EDITOR
            TestWorld.Fill(cabin, "_builtWhenPlaced", true);
#endif
            World.Submit(new PlaceStructureCommand(cabin, new Placement(new Vector2(30f, 30f), 0f)));
            World.Step();
            _presenter.Touch(_town.PlacedCamp);
            _presenter.Touch(World.Structures.All[1]);
            Assert.IsFalse(_view.Shown, "Une cabane sans poste ni stock n'a pas de fiche.");

            _presenter.Touch(_town.PlacedCamp);
            _view.ClickClose();
            Assert.IsFalse(_view.Shown, "La croix la ferme.");
            Assert.IsFalse(_presenter.Close(), "Déjà fermée : Échap passe à la suite.");
        }

        [Test]
        public void TheSheetFollowsTheStock()
        {
            _presenter.Touch(_town.PlacedCamp);
            World.Submit(new AssignVillagerCommand(World.Villagers.Chimera.Id, _town.PlacedCamp.Id));
            World.Submit(new PlaceStructureCommand(_town.Cabin(new GoodAmount(_town.Wood, 10)),
                new Placement(new Vector2(28f, 20f), 0f)));
            for (int step = 0; step < 3000 && _view.Lines[0].Count == 30; step++)
                World.Step();

            Assert.AreEqual(25, _view.Lines[0].Count, "La Chimère a pris 5 Bois : la fiche le montre.");
        }
    }
}
