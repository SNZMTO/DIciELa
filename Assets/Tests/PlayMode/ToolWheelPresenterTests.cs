using Diciela.Controls;
using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le présentateur de la roue des outils (décision 168), sur le petit monde des tests : Construire ouvre le panneau,
    /// ou le ferme ; Retirer prend l'outil en main, ou le lâche, et ferme le panneau ; la bulle nomme l'outil visé ; un
    /// outil actif reste enfoncé. Une vue et un panneau qui notent, des touches et une table des textes écrites ici.
    /// </summary>
    public sealed class ToolWheelPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "outil.construire,Construire\n" +
                                     "outil.construire.suite,Choisis une construction\n" +
                                     "outil.retirer,Retirer\n" +
                                     "outil.retirer.suite,Marque un arbre\n" +
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
                _build);
        }

        [TearDown]
        public void DestroyWheel()
        {
            _presenter.Dispose();
            _test.Dispose();
        }

        [Test]
        public void TheWheelShowsTheKeysAndTheIconNamesTheWheelKey()
        {
            Assert.AreEqual(("Tab", "B", "X"), _view.Keys);
            Assert.AreEqual(("Outils", "Ou tiens Tab"), _presenter.IconTooltip());
        }

        [Test]
        public void BuildOpensThePanelAndASecondBuildClosesIt()
        {
            _view.Choose(ToolChoice.Build);
            Assert.IsTrue(_build.IsOpen);
            Assert.IsTrue(_view.Building, "Le panneau ouvert, Construire reste enfoncé.");

            _view.Choose(ToolChoice.Build);
            Assert.IsFalse(_build.IsOpen);
            Assert.IsFalse(_view.Building);
        }

        [Test]
        public void ClearTakesTheToolClosesThePanelAndASecondClearDropsIt()
        {
            _build.Open();

            _view.Choose(ToolChoice.Clear);
            Assert.IsTrue(_test.Clearing.IsActive, "Retirer prend l'outil en main.");
            Assert.IsFalse(_build.IsOpen, "Et ferme le panneau.");
            Assert.IsTrue(_view.Clearing, "En main, Retirer reste enfoncé.");

            _view.Choose(ToolChoice.Clear);
            Assert.IsFalse(_test.Clearing.IsActive, "Un second choix le lâche.");
            Assert.IsFalse(_view.Clearing);
        }

        [Test]
        public void BuildPutsTheAxeDown()
        {
            _view.Choose(ToolChoice.Clear);

            _view.Choose(ToolChoice.Build);

            Assert.IsFalse(_test.Clearing.IsActive, "Construire pose la hache.");
            Assert.IsTrue(_build.IsOpen);
        }

        [Test]
        public void TheBubbleNamesTheAimedTool()
        {
            _view.Point(ToolChoice.Clear);
            Assert.AreEqual(("Retirer", "Marque un arbre"), _view.Label);

            _view.Point(ToolChoice.Build);
            Assert.AreEqual(("Construire", "Choisis une construction"), _view.Label);

            _view.Point(null);
            Assert.AreEqual(("", ""), _view.Label);
        }
    }
}
