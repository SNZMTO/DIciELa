using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La roue d'un outil de sol (décision 221), par le présentateur de la roue, comme sa vue l'appelle, sur le petit monde
    /// des tests (une Place, la terre) : ouverte l'outil en main, elle montre ses cases à leur place, la place de charrette
    /// pour la Place seulement ; une option prend son mode et le règle ; le même chiffre deux fois passe à l'option
    /// suivante ; la symétrie et le sol se règlent au chiffre ; la molette change la taille du pinceau et les branches de
    /// l'étoile ; la case Outils retourne la roue sur les outils. Les textes du jeu ont tout ce que la roue nomme.
    /// </summary>
    public sealed class PaintRingTests
    {
        private TestWorld _test;
        private NotedToolWheelView _view;
        private ToolWheelPresenter _presenter;

        [SetUp]
        public void CreateWheel()
        {
            _test = new TestWorld(withPlaza: true, withFloor: true);
            _view = new NotedToolWheelView();
            _presenter = new ToolWheelPresenter(_view, TextTable.Parse("clé,fr\n", "fr"), new FixedToolKeys(),
                _test.Catalog, _test.World, _test.Tool, _test.Clearing, _test.Mover, _test.Paths, _test.Plaza);
        }

        [TearDown]
        public void DestroyWheel()
        {
            _test.Dispose();
        }

        [Test]
        public void OpenedToolInHandItShowsTheToolsRingWithItsKeys()
        {
            _test.Plaza.Take();
            _presenter.Open(false);
            Assert.AreEqual(8, _view.Ring.Count, "La Place : ses huit cases.");
            for (int place = 0; place < 8; place++)
                Assert.AreEqual((place + 1).ToString(), _view.At(place).Key, "La case " + place + " a son chiffre.");
            Assert.IsTrue(_view.At(0).IsOn, "Au départ, le pinceau.");

            _test.Plaza.PutDown();
            _test.Paths.Take();
            _presenter.Open(false);
            Assert.AreEqual(7, _view.Ring.Count, "Chemins : pas de place de charrette…");
            Assert.IsNull(_view.At(4));
            Assert.AreEqual(1, _view.At(7).Options.Count, "… et la terre pour seul sol (la Place y ajoute la zone).");
        }

        [Test]
        public void AnOptionTakesItsModeAndTheSameDigitTwiceTakesTheNextOption()
        {
            _test.Plaza.Take();
            _presenter.Open(false);
            Assert.IsFalse(_presenter.Pick(_view.IndexAt(3), 2), "Prendre une option ferme la roue.");
            Assert.AreEqual(PaintToolMode.Shape, _test.Plaza.Mode, "L'option du polygone prend le mode Forme…");
            Assert.AreEqual(PaintShape.Polygon, _test.Plaza.Options.Shape, "… et le polygone.");

            Assert.IsFalse(_presenter.TakeSlot(3), "Le chiffre 4 de nouveau…");
            Assert.AreEqual(PaintShape.Disc, _test.Plaza.Options.Shape, "… passe à la forme suivante : le cercle.");
            Assert.IsFalse(_presenter.TakeSlot(2));
            Assert.AreEqual(PaintToolMode.Trace, _test.Plaza.Mode, "Le chiffre 3 prend le tracé.");
            _presenter.TakeSlot(2);
            Assert.AreEqual(PaintTraceStyle.Curved, _test.Plaza.Options.Style, "Une seconde fois : en courbe.");
        }

        [Test]
        public void TheSymmetryAndTheGroundTurnOnTheirDigitWithoutChangingTheMode()
        {
            _test.Plaza.Take();
            _presenter.TakeSlot(6);
            Assert.AreEqual(PaintSymmetry.Mirror, _test.Plaza.Options.Symmetry, "Le chiffre 7 : la symétrie suivante.");
            Assert.AreEqual(PaintInk.Zone, _test.Plaza.Inks.Chosen);
            _presenter.TakeSlot(7);
            Assert.AreEqual(PaintInk.Of(_test.Floor), _test.Plaza.Inks.Chosen, "Le chiffre 8 : le sol suivant, la terre.");
            Assert.AreEqual(PaintToolMode.Brush, _test.Plaza.Mode, "Le pinceau reste en main.");
        }

        [Test]
        public void TheWheelScrollsTheBrushSizeAndTheStarBranches()
        {
            _test.Plaza.Take();
            _presenter.Open(false);
            float radius = _test.Plaza.Options.BrushRadius;
            _presenter.Aim(_view.IndexAt(0), -1);
            _presenter.Scroll(1);
            Assert.AreEqual(radius + _test.Settings.BrushRadiusStepMeters, _test.Plaza.Options.BrushRadius, 1e-4f,
                "La molette sur le pinceau : un cran de plus.");

            int branches = _test.Plaza.Options.StarBranches;
            _presenter.Aim(_view.IndexAt(6), (int)PaintSymmetry.Star);
            _presenter.Scroll(1);
            Assert.AreEqual(branches + 1, _test.Plaza.Options.StarBranches, "Sur l'étoile : une branche de plus.");
            Assert.AreEqual((branches + 1).ToString(), _view.At(6).Options[(int)PaintSymmetry.Star].Badge,
                "Sa pastille le dit.");
        }

        [Test]
        public void TheToolsSlotTurnsTheWheelBackToTheTools()
        {
            _test.Paths.Take();
            _presenter.Open(false);
            // La case Outils est la sixième, en bas à gauche : sa place est 5.
            Assert.IsTrue(_presenter.Pick(_view.IndexAt(5), -1), "La case Outils garde la roue ouverte…");
            Assert.AreEqual(5, _view.Ring.Count, "… retournée sur les outils.");
            Assert.IsTrue(_view.At(6).IsOn, "Chemins, en main, y est enfoncé.");
            Assert.IsTrue(_presenter.TakeSlot(5), "Au clavier, le chiffre 6 demande la roue des outils.");
        }

        [Test]
        public void TheGameTextsNameEverythingTheWheelShows()
        {
#if UNITY_EDITOR
            var settings = TestAssets.LoadSingle<InterfaceSettings>();
            TextTable texts = TextTable.Parse(settings.Texts.text, settings.Language);
            var keys = new List<string>
            {
                TextKeys.PlazaTool, TextKeys.PlazaToolDetail, TextKeys.WheelBack, TextKeys.WheelBackDetail,
                TextKeys.WheelSymmetry, TextKeys.WheelSymmetryDetail, TextKeys.WheelInk, TextKeys.WheelInkDetail,
                TextKeys.PaintStarDetail
            };
            foreach (PaintToolMode mode in System.Enum.GetValues(typeof(PaintToolMode)))
                keys.AddRange(new[] { TextKeys.PaintMode(mode.ToString()), TextKeys.PaintModeDetail(mode.ToString()) });
            foreach (PaintShape shape in System.Enum.GetValues(typeof(PaintShape)))
                keys.AddRange(new[] { TextKeys.PaintShape(shape.ToString()), TextKeys.PaintShapeDetail(shape.ToString()) });
            foreach (PaintTraceStyle style in System.Enum.GetValues(typeof(PaintTraceStyle)))
                keys.AddRange(new[] { TextKeys.PaintStyle(style.ToString()), TextKeys.PaintStyleDetail(style.ToString()) });
            foreach (PaintSymmetry symmetry in System.Enum.GetValues(typeof(PaintSymmetry)))
                keys.AddRange(new[]
                    { TextKeys.PaintSymmetry(symmetry.ToString()), TextKeys.PaintSymmetryDetail(symmetry.ToString()) });
            foreach (string key in keys)
                Assert.IsTrue(texts.Has(key), "Le texte « " + key + " » manque.");
#endif
        }
    }
}
