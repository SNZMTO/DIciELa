using Diciela.Controls;
using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La barre partagée des outils de sol (décision 210), sur le petit monde des tests et une vue qui note : l'outil de la
    /// Place montre devant ses cinq modes la zone, puis la terre ; cliquer la terre la prend ; changer la taille du
    /// pinceau ne refait pas les encres ; l'outil Chemins montre ses quatre modes, la terre seule, la taille de son
    /// pinceau (2 m de large) et ni superficie ni places de charrette.
    /// </summary>
    public sealed class GroundBarPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "place.outil.Brush,Pinceau\nplace.outil.suite.Brush,Peint\n" +
                                     "place.outil.Eraser,Gomme\nplace.outil.suite.Eraser,Gomme\n" +
                                     "place.outil.Disc,Cercle\nplace.outil.suite.Disc,Trace\n" +
                                     "place.outil.Box,Rectangle\nplace.outil.suite.Box,Trace\n" +
                                     "place.outil.CartSpot,Place de charrette\nplace.outil.suite.CartSpot,Pose\n" +
                                     "place.zone,La zone\nplace.zone.suite,Trace la zone\nsol.suite,Un chantier\n" +
                                     "place.superficie,{superficie} / {max} m²\nplace.marques,{marques} / {max}\n" +
                                     "place.taille,Ø {taille} m\n";

        private TestWorld _test;
        private NotedPlaceBarView _view;
        private PlaceBarPresenter _presenter;

        [SetUp]
        public void CreateBar()
        {
            _test = new TestWorld(withPlace: true, withFloor: true);
            _view = new NotedPlaceBarView();
            _presenter = new PlaceBarPresenter(_view, TextTable.Parse(Texts, "fr"), _test.Place, _test.Paths,
                _test.World, new FixedToolKeys());
        }

        [TearDown]
        public void DestroyBar()
        {
            _presenter.Dispose();
            _test.Dispose();
        }

        [Test]
        public void ThePlaceToolShowsTheZoneThenTheFloorsBeforeItsModes()
        {
            _test.Place.Take();
            Assert.AreEqual(2, _view.Inks.Count, "La zone, puis la terre.");
            Assert.IsNull(_view.Inks[0].Floor);
            Assert.AreEqual("La zone", _view.Inks[0].Name);
            Assert.AreSame(_test.Floor, _view.Inks[1].Floor);
            Assert.IsNull(_view.SelectedInk, "Au départ, la zone est prise.");

            _view.ChooseInk(_test.Floor);
            Assert.AreSame(_test.Floor, _test.Place.Inks.Floor, "Un clic sur la terre la prend…");
            Assert.AreSame(_test.Floor, _view.SelectedInk, "… et l'enfonce.");

            int shown = _view.InksShown;
            _test.Place.ResizeBrush(1);
            Assert.AreEqual(shown, _view.InksShown, "Le pinceau change de taille : les encres restent.");
        }

        [Test]
        public void ThePathsToolShowsItsFourModesItsFloorsAndItsBrush()
        {
            _test.Paths.Take();
            Assert.AreEqual(4, _view.Modes.Count, "Pinceau, gomme, cercle, rectangle.");
            Assert.AreEqual(PlaceToolMode.Box, _view.Modes[3].Mode);
            Assert.AreEqual(1, _view.Inks.Count, "La terre seule : pas de zone aux Chemins.");
            Assert.AreSame(_test.Floor, _view.SelectedInk);
            Assert.AreEqual("", _view.Area, "Ni superficie…");
            Assert.AreEqual("", _view.Marks, "… ni places de charrette.");
            Assert.AreEqual("Ø 2 m", _view.BrushSize, "Un chemin de 2 m de large.");

            _test.Paths.PutDown();
            Assert.IsFalse(_view.IsShown, "Lâché, la barre se cache.");
        }
    }
}
