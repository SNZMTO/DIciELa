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
    /// Le présentateur de la carte de l'outil en main (décision 221), sur le petit monde des tests, dont la Place a
    /// 100 m² au plus et deux places de charrette, avec un arbre, et une vue qui note ce qu'on lui montre ; une table des
    /// textes écrite ici. La carte paraît l'outil en main, à l'icône et à la couleur de sa case dans la roue, et se cache
    /// les mains vides ; l'outil de la Place dit son mode, ce qu'il pose, la taille du pinceau, sa superficie et ses places
    /// de charrette, qui suivent la Place ; la forme, son style et la symétrie se disent en puces ; « Retirer » compte
    /// ce qui est marqué ; Déplacer dit ce qu'on tient et pourquoi il ne se pose pas là ; un ordre refusé en jouant le dit.
    /// </summary>
    public sealed class ToolCardPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "outil.retirer,Retirer\n" +
                                     "outil.deplacer,Déplacer\n" +
                                     "outil.place,La Place\n" +
                                     "place.outil.Brush,Pinceau\n" +
                                     "place.outil.Shape,Forme\n" +
                                     "place.forme.Disc,Cercle\n" +
                                     "place.forme.Polygon,Polygone\n" +
                                     "place.style.Straight,Droit\n" +
                                     "place.symetrie.Mirror,Miroir\n" +
                                     "carte.outil.etoile,Étoile à {branches} branches\n" +
                                     "place.zone,La zone\n" +
                                     "place.superficie,{superficie} / {max} m²\n" +
                                     "place.marques,{marques} / {max}\n" +
                                     "place.taille,Ø {taille} m\n" +
                                     "place.refus.UnknownMark,Cette place de charrette n'existe plus\n" +
                                     "carte.outil.marques,{nombre} à retirer\n" +
                                     "construction.cabane,Cabane\n" +
                                     "pose.refus.Overlaps,La place est prise\n";

        private NatureDefinition _tree;
        private TestWorld _test;
        private NotedToolCardView _view;
        private ToolCardPresenter _presenter;

        [SetUp]
        public void CreateCard()
        {
            _tree = ScriptableObject.CreateInstance<NatureDefinition>();
            _test = new TestWorld(withPlaza: true,
                nature: new[] { new NatureSpot(_tree, new Placement(new Vector2(30f, 70f), 0f)) });
            _view = new NotedToolCardView();
            _presenter = new ToolCardPresenter(_view, TextTable.Parse(Texts, "fr"), _test.Clearing, _test.Mover,
                _test.Paths, _test.Plaza, _test.World);
        }

        [TearDown]
        public void DestroyCard()
        {
            _presenter.Dispose();
            _test.Dispose();
            Object.Destroy(_tree);
        }

        [Test]
        public void TheCardShowsTheToolInHandAndHidesHandsEmpty()
        {
            Assert.IsFalse(_view.IsShown, "Les mains vides, pas de carte.");
            _test.Plaza.Take();
            Assert.IsTrue(_view.IsShown);
            Assert.AreEqual("La Place", _view.Name);
            Assert.AreEqual("roue-place", _view.Icon, "L'icône de sa case dans la roue…");
            Assert.AreEqual("peche", _view.Pastel, "… et sa couleur.");
            Assert.AreEqual("Pinceau", _view.Mode, "Un outil de sol dit son mode.");
            Assert.AreEqual("place-icone-brush", _view.ModeIcon);

            _test.Plaza.PutDown();
            Assert.IsFalse(_view.IsShown, "Lâché, la carte se cache.");
            _test.Clearing.Take();
            Assert.AreEqual("Retirer", _view.Name);
            Assert.AreEqual("", _view.Mode, "« Retirer » n'a pas de mode.");
            _test.Clearing.PutDown();
            _test.Mover.Take();
            Assert.AreEqual("Déplacer", _view.Name);
            Assert.IsEmpty(_view.Chips, "Rien de touché, rien à dire.");
        }

        [Test]
        public void ThePlazaChipsFollowItsZoneItsSpotsAndTheBrush()
        {
            _test.Plaza.Take();
            Assert.AreEqual(new[] { "La zone", "Ø 4 m", "0 / 100 m²", "0 / 2" }, _view.ChipTexts,
                "Ce qu'il pose, le pinceau de 2 m de rayon, la superficie, les places de charrette.");

            _test.World.Submit(new PaintPlazaCommand(new GroundDisc(new Vector2(50f, 50f), 5f)));
            _test.World.Step();
            StringAssert.EndsWith(" / 100 m²", _view.ChipTexts[2]);
            Assert.AreNotEqual("0 / 100 m²", _view.ChipTexts[2], "La superficie suit la Place.");
            _test.World.Submit(new AddCartSpotCommand(new Vector2(50f, 50f), 0f));
            _test.World.Step();
            Assert.AreEqual("1 / 2", _view.ChipTexts[3], "Les places de charrette aussi.");
            _test.Plaza.ResizeBrush(1);
            Assert.AreNotEqual("Ø 4 m", _view.ChipTexts[1], "La taille suit le pinceau.");

            _test.Plaza.Choose(PaintToolMode.CartSpot);
            Assert.AreEqual(2, _view.Chips.Count, "Au mode place de charrette, ni sol ni pinceau.");
        }

        [Test]
        public void TheChipsSayTheShapeItsStyleAndTheSymmetry()
        {
            _test.Plaza.Take();
            _test.Plaza.Choose(PaintToolMode.Shape);
            _test.Plaza.Options.Choose(PaintShape.Disc);
            CollectionAssert.Contains(_view.ChipTexts, "Cercle");
            CollectionAssert.DoesNotContain(_view.ChipTexts, "Droit", "Un cercle n'a pas de style.");
            _test.Plaza.Options.Choose(PaintShape.Polygon);
            CollectionAssert.IsSubsetOf(new[] { "Polygone", "Droit" }, _view.ChipTexts,
                "Un polygone a le style d'un tracé.");

            _test.Plaza.Options.Choose(PaintSymmetry.Mirror);
            CollectionAssert.Contains(_view.ChipTexts, "Miroir");
            _test.Plaza.Options.Choose(PaintSymmetry.Star);
            _test.Plaza.Options.ChooseStar(5);
            CollectionAssert.Contains(_view.ChipTexts, "Étoile à 5 branches");
        }

        [Test]
        public void ClearingCountsWhatIsMarkedToBeRemoved()
        {
            _test.Clearing.Take();
            Assert.AreEqual(new[] { "0 à retirer" }, _view.ChipTexts);

            _test.World.Submit(new MarkForClearingCommand(new[] { 1 }));
            _test.World.Step();
            Assert.AreEqual(new[] { "1 à retirer" }, _view.ChipTexts, "La carte suit les arbres marqués.");
        }

        [Test]
        public void MovingSaysWhatIsHeldAndWhyItCannotGoThere()
        {
            Vector3 other = TestWorld.Center + new Vector3(10f, 0f, 0f);
            TestMove.Place(_test, _test.Cabin, TestWorld.Center);
            TestMove.Place(_test, _test.Cabin, other);
            _test.Mover.Take();
            Assert.IsTrue(TestMove.Touch(_test, TestWorld.Center, 0.5f));
            Assert.AreEqual(new[] { "Cabane" }, _view.ChipTexts, "La carte dit ce qu'on tient.");

            _test.Mover.AimAt(_test.OnScreen(other + new Vector3(1f, 0f, 1f)));
            Assert.AreEqual("La place est prise", _view.Reason);
            Assert.IsTrue(_view.IsRefusal);
        }

        [Test]
        public void AnOrderRefusedWhenPlayedSaysWhy()
        {
            _test.Plaza.Take();
            _test.World.Submit(new RemoveCartSpotCommand(99));
            _test.World.Step();

            Assert.AreEqual("Cette place de charrette n'existe plus", _view.Reason);
            Assert.IsTrue(_view.IsRefusal);
        }
    }
}
