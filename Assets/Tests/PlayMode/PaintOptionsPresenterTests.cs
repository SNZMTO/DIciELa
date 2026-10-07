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
    /// La rangée d'options de la barre des outils de sol (décision 216), par le présentateur de la barre, sur le petit
    /// monde des tests, et une vue qui note ce qu'on lui montre et joue les clics du joueur ; une table des textes écrite
    /// ici. Les formes ne paraissent qu'au mode Forme, les styles au tracé et au polygone, les symétries partout, les
    /// nombres de l'étoile à l'étoile ; un clic prend ce qu'il montre, chez les options de l'outil.
    /// </summary>
    public sealed class PaintOptionsPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "place.outil.Brush,Pinceau\n" +
                                     "place.outil.Eraser,Gomme\n" +
                                     "place.outil.Trace,Tracé\n" +
                                     "place.outil.Shape,Forme\n" +
                                     "place.forme.Disc,Cercle\n" +
                                     "place.forme.Box,Rectangle\n" +
                                     "place.forme.Polygon,Polygone\n" +
                                     "place.style.Straight,Droit\n" +
                                     "place.style.Curved,Courbe\n" +
                                     "place.symetrie.None,Sans symétrie\n" +
                                     "place.symetrie.Star,Étoile\n" +
                                     "place.etoile.suite,{branches} branches\n" +
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
        public void TheShapesShowOnlyInTheShapeModeAndAClickTakesOne()
        {
            _test.Plaza.Take();
            Assert.IsEmpty(_view.Options.Shapes, "Au pinceau, pas de formes.");
            _view.Choose(PaintToolMode.Shape);
            Assert.AreEqual("Cercle", _view.Options.Shapes[0].Name);
            Assert.AreEqual("Rectangle", _view.Options.Shapes[1].Name);
            Assert.AreEqual(PaintShape.Disc, _view.Options.SelectedShape, "Au départ, le cercle.");
            Assert.AreEqual("", _view.BrushSize, "Une forme n'a pas de pinceau.");
            _view.Options.ChooseShape(PaintShape.Box);
            Assert.AreEqual(PaintShape.Box, _test.Plaza.Options.Shape, "Un clic sur une forme la prend…");
            Assert.AreEqual(PaintShape.Box, _view.Options.SelectedShape, "… et l'enfonce.");
            _view.Choose(PaintToolMode.Trace);
            Assert.IsEmpty(_view.Options.Shapes, "Le tracé n'a pas de formes…");
            Assert.AreEqual("Ø 4 m", _view.BrushSize, "… mais la largeur du pinceau.");
        }

        [Test]
        public void TheStylesShowForATraceAndAPolygonAndAClickTakesOne()
        {
            _test.Plaza.Take();
            Assert.IsEmpty(_view.Options.Styles, "Au pinceau, pas de styles.");
            _view.Choose(PaintToolMode.Trace);
            Assert.AreEqual("Droit", _view.Options.Styles[0].Name);
            Assert.AreEqual("Courbe", _view.Options.Styles[1].Name);
            _view.Options.ChooseStyle(PaintTraceStyle.Curved);
            Assert.AreEqual(PaintTraceStyle.Curved, _test.Plaza.Options.Style, "Un clic sur un style le prend…");
            Assert.AreEqual(PaintTraceStyle.Curved, _view.Options.SelectedStyle, "… et l'enfonce.");
            _view.Choose(PaintToolMode.Shape);
            Assert.IsEmpty(_view.Options.Styles, "Le cercle n'a pas de style…");
            _view.Options.ChooseShape(PaintShape.Polygon);
            Assert.AreEqual(2, _view.Options.Styles.Count, "… le polygone, si.");
        }

        [Test]
        public void TheSymmetriesShowInEveryModeAndTheStarItsCopies()
        {
            _test.Plaza.Take();
            Assert.AreEqual(4, _view.Options.Symmetries.Count, "Aucune, miroir, double miroir, étoile.");
            Assert.AreEqual("Sans symétrie", _view.Options.Symmetries[0].Name);
            Assert.IsEmpty(_view.Options.Stars, "Sans étoile, pas de nombre.");
            _view.Options.ChooseSymmetry(PaintSymmetry.Star);
            Assert.AreEqual(PaintSymmetry.Star, _test.Plaza.Options.Symmetry, "Un clic sur une symétrie la prend…");
            Assert.AreEqual(PaintSymmetry.Star, _view.Options.SelectedSymmetry, "… et l'enfonce.");
            Assert.AreEqual("3", _view.Options.Stars[0].Name, "L'étoile montre ses nombres, de 3…");
            Assert.AreEqual("8", _view.Options.Stars[_view.Options.Stars.Count - 1].Name, "… à 8.");
            Assert.AreEqual("6 branches", _view.Options.Stars[3].Detail);
            _view.Options.ChooseStar(6);
            Assert.AreEqual(6, _test.Plaza.Options.StarBranches);
            Assert.AreEqual(6, _view.Options.SelectedStar);
            _view.Choose(PaintToolMode.CartSpot);
            Assert.AreEqual(4, _view.Options.Symmetries.Count, "Les places de charrette aussi.");
        }
    }
}
