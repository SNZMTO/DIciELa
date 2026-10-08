using Diciela.Controls;
using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le présentateur de la roue (décisions 221 et 226), sur le petit monde des tests, dont le catalogue a une Place
    /// et le Logement (le campement, puis la cabane) : ouverte sur les outils, chacun à sa place et avec sa touche, la
    /// Place seulement une fois le campement posé ; Construire met en main une construction, ou la lâche ; Retirer
    /// prend l'outil en main, ou le lâche, et lâche la construction en main ; la bulle nomme ce qu'on vise ; un outil
    /// en main reste enfoncé. Une vue qui note, des touches et une table des textes écrites ici.
    /// </summary>
    public sealed class ToolWheelPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "outil.construire,Construire\n" +
                                     "outil.construire.suite,Prends une construction\n" +
                                     "outil.retirer,Retirer\n" +
                                     "outil.retirer.suite,Marque un arbre\n" +
                                     "outil.place,La Place\n" +
                                     "outils.infobulle,Outils\n" +
                                     "outils.infobulle.suite,Ou tiens {touche}\n";

        private TestWorld _test;
        private NotedToolWheelView _view;
        private ToolWheelPresenter _presenter;

        [SetUp]
        public void CreateWheel()
        {
            _test = new TestWorld(withPlaza: true);
            _test.Housing();
            _view = new NotedToolWheelView();
            _presenter = new ToolWheelPresenter(_view, TextTable.Parse(Texts, "fr"), new FixedToolKeys(), _test.Catalog,
                _test.World, _test.Tool, _test.Clearing, _test.Mover, _test.Paths, _test.Plaza);
        }

        [TearDown]
        public void DestroyWheel()
        {
            _test.Dispose();
        }

        [Test]
        public void OnTheToolsEachHasItsPlaceAndItsKey()
        {
            _test.PlantTheCamp(_presenter);
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
        public void ThePlazaWaitsUntilTheCampIsPlanted()
        {
            _presenter.Open(true);
            Assert.AreEqual(4, _view.Ring.Count, "Avant le campement, pas de Place dans la roue…");
            Assert.IsNull(_view.At(7));
            _presenter.Choose(ToolChoice.Plaza);
            Assert.IsFalse(_test.Plaza.IsActive, "… ni par sa touche.");

            _test.PlantTheCamp(_presenter);
            _presenter.Choose(ToolChoice.Plaza);
            Assert.IsTrue(_test.Plaza.IsActive);
        }

        [Test]
        public void BuildTakesAConstructionAndASecondBuildDropsIt()
        {
            _presenter.Open(true);
            Assert.IsFalse(_presenter.Pick(_view.IndexAt(0), -1), "Prendre un outil ferme la roue.");
            Assert.AreSame(_test.Camp, _test.Tool.Current, "Construire met en main le campement, proposé d'abord.");
            _presenter.Open(true);
            Assert.IsTrue(_view.At(0).IsOn, "Une construction en main, Construire reste enfoncé.");

            _presenter.Choose(ToolChoice.Build);
            Assert.IsFalse(_test.Tool.IsActive, "Un second choix la lâche.");
        }

        [Test]
        public void ClearTakesTheToolPutsTheConstructionDownAndASecondClearDropsIt()
        {
            _presenter.Choose(ToolChoice.Build);

            _presenter.Choose(ToolChoice.Clear);
            Assert.IsTrue(_test.Clearing.IsActive, "Retirer prend l'outil en main…");
            Assert.IsFalse(_test.Tool.IsActive, "… et lâche la construction.");
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
            Assert.IsTrue(_test.Tool.IsActive);
        }

        [Test]
        public void TheBubbleNamesTheAimedTool()
        {
            _presenter.Open(true);
            _presenter.Aim(_view.IndexAt(4), -1);
            Assert.AreEqual(("Retirer", "Marque un arbre"), _view.Label);

            _presenter.Aim(_view.IndexAt(0), -1);
            Assert.AreEqual(("Construire", "Prends une construction"), _view.Label);

            _presenter.Aim(-1, -1);
            Assert.AreEqual(("", ""), _view.Label);
        }
    }
}
