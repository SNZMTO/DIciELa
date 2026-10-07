using Diciela.Controls;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le présentateur du curseur des outils (décision 221), sur le petit monde des tests (une Place, la terre) et une vue
    /// qui note ce qu'on lui montre. Les mains vides, la flèche ; chaque outil sa silhouette, à la couleur de sa case dans
    /// la roue ; l'outil de la Place montre en pastilles ce qu'il pose, sa forme ou son style, sa symétrie ; la croix
    /// rouge dit qu'un geste serait refusé : une place de charrette hors de la Place, une construction sur une autre.
    /// </summary>
    public sealed class ToolCursorPresenterTests
    {
        private TestWorld _test;
        private NotedToolCursorView _view;
        private ToolCursorPresenter _presenter;

        [SetUp]
        public void CreateCursor()
        {
            _test = new TestWorld(withPlaza: true, withFloor: true);
            _view = new NotedToolCursorView();
            _presenter = new ToolCursorPresenter(_view, _test.Tool, _test.Clearing, _test.Mover, _test.Paths,
                _test.Plaza);
        }

        [TearDown]
        public void DestroyCursor()
        {
            _presenter.Dispose();
            _test.Dispose();
        }

        [Test]
        public void EachToolHasItsSilhouetteInItsColour()
        {
            Assert.IsNull(_view.Look, "Les mains vides, la flèche.");
            _test.Clearing.Take();
            Assert.AreEqual("curseur-hache", _view.Look?.Silhouette);
            Assert.AreEqual("menthe", _view.Look?.Pastel, "La couleur de « Retirer » dans la roue.");
            _test.Clearing.PutDown();
            _test.Mover.Take();
            Assert.AreEqual("curseur-main", _view.Look?.Silhouette);
            _test.Mover.PutDown();
            _test.Tool.Begin(_test.Cabin);
            Assert.AreEqual("curseur-marteau", _view.Look?.Silhouette, "Une construction en main, le marteau.");
            _test.Tool.Cancel();
            Assert.IsNull(_view.Look, "Lâchée, la flèche.");
        }

        [Test]
        public void ThePlazaToolShowsWhatItLaysItsFormAndItsSymmetry()
        {
            _test.Plaza.Take();
            Assert.AreEqual("curseur-pinceau", _view.Look?.Silhouette);
            Assert.AreEqual("peche", _view.Look?.Pastel);
            Assert.AreEqual("place-icone-superficie", _view.Look?.InkIcon, "La zone, au départ.");
            Assert.AreEqual("", _view.Look?.FormIcon);
            Assert.AreEqual("", _view.Look?.SymmetryIcon, "Sans symétrie, pas de pastille.");

            _test.Plaza.Choose(PaintToolMode.Shape);
            Assert.AreEqual("curseur-viseur", _view.Look?.Silhouette);
            Assert.AreEqual("place-forme-disc", _view.Look?.FormIcon, "La forme du mode Forme.");
            _test.Plaza.Choose(PaintToolMode.Trace);
            Assert.AreEqual("curseur-plume", _view.Look?.Silhouette);
            Assert.AreEqual("place-style-straight", _view.Look?.FormIcon, "Le style d'un tracé.");
            _test.Plaza.Options.Choose(PaintSymmetry.Mirror);
            Assert.AreEqual("place-symetrie-mirror", _view.Look?.SymmetryIcon);

            _test.Plaza.Choose(PaintToolMode.CartSpot);
            Assert.AreEqual("curseur-place-charrette", _view.Look?.Silhouette);
            Assert.AreEqual("", _view.Look?.InkIcon, "Une place de charrette ne pose ni zone ni sol.");
        }

        [Test]
        public void TheRedCrossSaysAGestureWouldBeRefused()
        {
            _test.Plaza.Take();
            _test.Plaza.Choose(PaintToolMode.CartSpot);
            _test.Plaza.AimAt(_test.OnScreen(TestWorld.Center));
            Assert.IsTrue(_view.IsRefused, "Une place de charrette hors de la Place est refusée.");
            _test.Plaza.PutDown();
            Assert.IsFalse(_view.IsRefused, "Lâché, plus de croix.");

            TestMove.Place(_test, _test.Cabin, TestWorld.Center);
            _test.Tool.Begin(_test.Cabin);
            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center + new Vector3(1f, 0f, 1f)));
            Assert.IsTrue(_view.IsRefused, "Une cabane sur une autre est refusée.");
            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center + new Vector3(15f, 0f, 0f)));
            Assert.IsFalse(_view.IsRefused, "À côté, elle se pose.");
        }
    }
}
