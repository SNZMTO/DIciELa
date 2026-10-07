using Diciela.Controls;
using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le présentateur de la roue (décisions 168 et 221), sur le petit monde des tests : ouverte sur les outils, chacun à
    /// sa place et avec sa touche, la Place seulement une fois proposée ; Construire ouvre le panneau, ou le ferme ;
    /// Retirer prend l'outil en main, ou le lâche, et ferme le panneau ; la bulle nomme ce qu'on vise ; un outil actif
    /// reste enfoncé. Une vue et un panneau qui notent, des touches et une table des textes écrites ici.
    /// </summary>
    public sealed class ToolWheelPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "outil.construire,Construire\n" +
                                     "outil.construire.suite,Choisis une construction\n" +
                                     "outil.retirer,Retirer\n" +
                                     "outil.retirer.suite,Marque un arbre\n" +
                                     "outil.place,La Place\n" +
                                     "outils.infobulle,Outils\n" +
                                     "outils.infobulle.suite,Ou tiens {touche}\n";

        private TestWorld _test;
        private NotedToolWheelView _view;
        private NotedBuildMenu _build;
        private ToolWheelPresenter _presenter;

        [SetUp]
        public void CreateWheel()
        {
            _test = new TestWorld();
            _view = new NotedToolWheelView();
            _build = new NotedBuildMenu();
            _presenter = new ToolWheelPresenter(_view, TextTable.Parse(Texts, "fr"), new FixedToolKeys(), _test.Clearing,
                _build, _test.Mover, _test.Paths, _test.Plaza);
        }

        [TearDown]
        public void DestroyWheel()
        {
            _test.Dispose();
        }

        [Test]
        public void OnTheToolsEachHasItsPlaceAndItsKey()
        {
            _presenter.Open(true);
            Assert.AreEqual("Tab", _view.WheelKey);
            Assert.AreEqual(("Outils", "Ou tiens Tab"), _presenter.IconTooltip());
            Assert.AreEqual(5, _view.Ring.Count, "Construire, Déplacer, Retirer, Chemins et la Place.");
            Assert.AreEqual("B", _view.At(0).Key, "Construire en haut.");
            Assert.AreEqual("M", _view.At(2).Key, "Déplacer à droite.");
            Assert.AreEqual("X", _view.At(4).Key, "Retirer en bas.");
            Assert.AreEqual("C", _view.At(6).Key, "Chemins à gauche.");
            Assert.AreEqual("P", _view.At(7).Key, "La Place en haut à gauche.");
            Assert.AreEqual("La Place", _view.At(7).Name);
        }

        [Test]
        public void ThePlazaWaitsUntilTheTownOffersIt()
        {
            _build.OffersPlaza = false;
            _presenter.Open(true);
            Assert.AreEqual(4, _view.Ring.Count, "Avant le campement, pas de Place dans la roue…");
            Assert.IsNull(_view.At(7));
            _presenter.Choose(ToolChoice.Plaza);
            Assert.IsFalse(_test.Plaza.IsActive, "… ni par sa touche.");

            _build.OffersPlaza = true;
            _presenter.Choose(ToolChoice.Plaza);
            Assert.IsTrue(_test.Plaza.IsActive);
        }

        [Test]
        public void BuildOpensThePanelAndASecondBuildClosesIt()
        {
            _presenter.Open(true);
            Assert.IsFalse(_presenter.Pick(_view.IndexAt(0), -1), "Prendre un outil ferme la roue.");
            Assert.IsTrue(_build.IsOpen);
            _presenter.Open(true);
            Assert.IsTrue(_view.At(0).IsOn, "Le panneau ouvert, Construire reste enfoncé.");

            _presenter.Choose(ToolChoice.Build);
            Assert.IsFalse(_build.IsOpen);
        }

        [Test]
        public void ClearTakesTheToolClosesThePanelAndASecondClearDropsIt()
        {
            _build.Open();

            _presenter.Choose(ToolChoice.Clear);
            Assert.IsTrue(_test.Clearing.IsActive, "Retirer prend l'outil en main.");
            Assert.IsFalse(_build.IsOpen, "Et ferme le panneau.");
            _presenter.Open(true);
            Assert.IsTrue(_view.At(4).IsOn, "En main, Retirer reste enfoncé.");

            _presenter.Choose(ToolChoice.Clear);
            Assert.IsFalse(_test.Clearing.IsActive, "Un second choix le lâche.");
        }

        [Test]
        public void MoveTakesItsToolAndPutsTheAxeDown()
        {
            _presenter.Choose(ToolChoice.Clear);

            _presenter.Choose(ToolChoice.Move);
            Assert.IsTrue(_test.Mover.IsActive, "Déplacer prend son outil en main.");
            Assert.IsFalse(_test.Clearing.IsActive, "Et pose la hache.");

            _presenter.Choose(ToolChoice.Build);
            Assert.IsFalse(_test.Mover.IsActive, "Construire le pose aussi.");
            Assert.IsTrue(_build.IsOpen);
        }

        [Test]
        public void TheBubbleNamesTheAimedTool()
        {
            _presenter.Open(true);
            _presenter.Aim(_view.IndexAt(4), -1);
            Assert.AreEqual(("Retirer", "Marque un arbre"), _view.Label);

            _presenter.Aim(_view.IndexAt(0), -1);
            Assert.AreEqual(("Construire", "Choisis une construction"), _view.Label);

            _presenter.Aim(-1, -1);
            Assert.AreEqual(("", ""), _view.Label);
        }
    }
}
