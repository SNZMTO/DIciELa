using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le présentateur de la barre de la Place (décision 206), sur le petit monde des tests, dont la Place a 100 m² au plus
    /// et deux places de charrette, et une vue qui note ce qu'on lui montre et joue les clics du joueur ; une table des
    /// textes écrite ici. La barre se montre l'outil en main ; ses modes, dans l'ordre des touches ; la superficie et les
    /// places de charrette suivent la Place ; la taille du pinceau ne se dit qu'au pinceau et à la gomme ; un clic sur un
    /// mode le prend ; un ordre refusé en jouant le dit.
    /// </summary>
    public sealed class PaintBarPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "place.outil.Brush,Pinceau\n" +
                                     "place.outil.Eraser,Gomme\n" +
                                     "place.outil.Disc,Cercle\n" +
                                     "place.outil.Box,Rectangle\n" +
                                     "place.outil.CartSpot,Place de charrette\n" +
                                     "place.superficie,{superficie} / {max} m²\n" +
                                     "place.marques,{marques} / {max}\n" +
                                     "place.taille,Ø {taille} m\n" +
                                     "place.refus.UnknownMark,Cette place de charrette n'existe plus\n";

        private TestWorld _test;
        private NotedPaintBarView _view;
        private PaintBarPresenter _presenter;

        [SetUp]
        public void CreateBar()
        {
            _test = new TestWorld(withPlaza: true);
            _view = new NotedPaintBarView();
            _presenter = new PaintBarPresenter(_view, TextTable.Parse(Texts, "fr"), _test.Plaza, _test.Paths,
                _test.World, new FixedToolKeys());
        }

        [TearDown]
        public void DestroyBar()
        {
            _presenter.Dispose();
            _test.Dispose();
        }

        [Test]
        public void TheBarShowsWhileTheToolIsInHand()
        {
            Assert.IsFalse(_view.IsShown, "Au repos, pas de barre.");
            _test.Plaza.Take();
            Assert.IsTrue(_view.IsShown);
            _test.Plaza.PutDown();
            Assert.IsFalse(_view.IsShown);
        }

        [Test]
        public void ItsModesComeInTheOrderOfTheirKeys()
        {
            _test.Plaza.Take();
            Assert.AreEqual(5, _view.Modes.Count);
            Assert.AreEqual("Pinceau", _view.Modes[0].Name);
            Assert.AreEqual("1", _view.Modes[0].Key);
            Assert.AreEqual("Place de charrette", _view.Modes[4].Name);
            Assert.AreEqual("5", _view.Modes[4].Key);

            _view.Choose(PaintToolMode.Disc);
            Assert.AreEqual(PaintToolMode.Disc, _test.Plaza.Mode, "Un clic sur un mode le prend.");
            Assert.AreEqual(PaintToolMode.Disc, _view.Selected);
        }

        [Test]
        public void TheValuesFollowThePlaza()
        {
            _test.Plaza.Take();
            Assert.AreEqual("0 / 100 m²", _view.Area);
            Assert.AreEqual("0 / 2", _view.Marks);
            Assert.AreEqual("Ø 4 m", _view.BrushSize, "Le pinceau de 2 m de rayon.");

            _test.World.Submit(new PaintPlazaCommand(new GroundDisc(new Vector2(50f, 50f), 5f)));
            _test.World.Step();
            StringAssert.EndsWith(" / 100 m²", _view.Area);
            Assert.AreNotEqual("0 / 100 m²", _view.Area, "La superficie suit la Place.");
            _test.World.Submit(new AddCartSpotCommand(new Vector2(50f, 50f), 0f));
            _test.World.Step();
            Assert.AreEqual("1 / 2", _view.Marks);

            _test.Plaza.Choose(PaintToolMode.CartSpot);
            Assert.AreEqual("", _view.BrushSize, "Sans pinceau, pas de taille.");
        }

        [Test]
        public void AnOrderRefusedWhenPlayedSaysWhy()
        {
            _test.Plaza.Take();
            _test.World.Submit(new RemoveCartSpotCommand(99));
            _test.World.Step();

            Assert.AreEqual("Cette place de charrette n'existe plus", _view.Reason);
        }
    }
}
