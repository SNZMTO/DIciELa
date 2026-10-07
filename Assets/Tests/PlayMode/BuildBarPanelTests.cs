using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le panneau Construire qui s'ouvre à la demande et se ferme quand on prend une carte (décision 168), et ses touches
    /// (décision 184), sur le petit monde des tests et une vue qui note ce qu'on lui montre et joue les choix du joueur.
    /// Un catalogue d'une famille, le Logement (le campement, la cabane) ; une table des textes écrite ici.
    /// </summary>
    public sealed class BuildBarPanelTests
    {
        private const string Texts = "clé,fr\n" +
                                     "famille.logement,Logement\n" +
                                     "construction.campement,Campement\n" +
                                     "construction.cabane,Cabane\n" +
                                     "carte.lits,{lits} lits\n";

        private TestWorld _test;
        private NotedBuildBarView _view;
        private BuildBarPresenter _presenter;

        [SetUp]
        public void CreateBar()
        {
            _test = new TestWorld();
            var housing = _test.Keep(ScriptableObject.CreateInstance<StructureFamilyDefinition>());
            var catalog = _test.Keep(ScriptableObject.CreateInstance<Catalog>());
#if UNITY_EDITOR
            TestWorld.Fill(housing, "_nameKey", "famille.logement");
            TestWorld.FillList(housing, "_structures", _test.Camp, _test.Cabin);
            TestWorld.FillList(catalog, "_families", housing);
#endif
            _view = new NotedBuildBarView();
            _presenter = new BuildBarPresenter(_view, catalog, TextTable.Parse(Texts, "fr"), _test.Tool, _test.World,
                new FixedToolKeys());
        }

        [TearDown]
        public void DestroyBar()
        {
            _presenter.Dispose();
            _test.Dispose();
        }

        // Le campement, par l'ordre du joueur : le reste du catalogue s'ouvre.
        private void PlaceTheCamp()
        {
            _test.World.Submit(new PlaceStructureCommand(_test.Camp, new Placement(new Vector2(30f, 30f), 0f)));
            _test.World.Step();
        }

        [Test]
        public void ThePanelStartsClosedAndOpensOnDemand()
        {
            Assert.IsFalse(_presenter.IsOpen);
            Assert.IsFalse(_view.IsShownOpen, "Au repos, le panneau est fermé : le bas de l'écran reste à la ville.");

            _presenter.Open();
            Assert.IsTrue(_view.IsShownOpen);

            _presenter.Close();
            Assert.IsFalse(_view.IsShownOpen);
        }

        [Test]
        public void TakingACardClosesThePanel()
        {
            PlaceTheCamp();
            _presenter.Open();

            _view.Choose(0);

            Assert.AreSame(_test.Cabin, _test.Tool.Current, "La carte prise est en main.");
            Assert.IsFalse(_view.IsShownOpen, "Le panneau se ferme pour dégager la vue pendant la pose.");
        }

        [Test]
        public void EscapeClosesThePanelOnlyWhenItIsOpen()
        {
            Assert.IsFalse(_presenter.Escape(), "Fermé, le panneau laisse Échap au cran suivant.");

            _presenter.Open();

            Assert.IsTrue(_presenter.Escape());
            Assert.IsFalse(_view.IsShownOpen);
        }

        [Test]
        public void AKeyTakesItsCardOnlyWhenThePanelIsOpen()
        {
            PlaceTheCamp();
            _presenter.TakeCard(0);
            Assert.IsNull(_test.Tool.Current, "Panneau fermé, la touche ne prend rien.");

            _presenter.Open();
            _presenter.TakeCard(1);
            Assert.IsNull(_test.Tool.Current, "Pas de deuxième carte au Logement, le campement posé.");

            _presenter.TakeCard(0);
            Assert.AreSame(_test.Cabin, _test.Tool.Current, "La touche 1 prend la cabane, comme un clic.");
            Assert.IsFalse(_view.IsShownOpen, "Et le panneau se ferme.");
        }
    }
}
