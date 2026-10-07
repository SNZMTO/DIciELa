using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le présentateur du panneau Construire, sur le petit monde des tests et une vue qui note ce qu'on lui montre et joue
    /// les choix du joueur. Un catalogue de deux familles (Logement : la cabane, qui offre deux lits ; Décor : un banc),
    /// une table des textes écrite ici. Ce qu'il montre et ce qu'une carte met en main ; qu'il s'ouvre et se ferme, et ses
    /// touches : BuildBarPanelTests.
    /// </summary>
    public sealed class BuildBarPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "famille.logement,Logement\n" +
                                     "famille.decor,Décor\n" +
                                     "construction.cabane,Cabane\n" +
                                     "construction.banc,Banc\n" +
                                     "carte.lits,{lits} lits\n" +
                                     "pose.refus.Overlaps,La place est prise\n";

        private TestWorld _test;
        private StructureDefinition _bench;
        private NotedBuildBarView _view;
        private BuildBarPresenter _presenter;

        [SetUp]
        public void CreateBar()
        {
            _test = new TestWorld();
            _bench = _test.Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            var housing = _test.Keep(ScriptableObject.CreateInstance<StructureFamilyDefinition>());
            var decor = _test.Keep(ScriptableObject.CreateInstance<StructureFamilyDefinition>());
            var catalog = _test.Keep(ScriptableObject.CreateInstance<Catalog>());
#if UNITY_EDITOR
            TestWorld.Fill(_bench, "_id", "banc");
            TestWorld.Fill(_bench, "_nameKey", "construction.banc");
            TestWorld.Fill(_test.Cabin, "_beds", 2);
            TestWorld.Fill(housing, "_nameKey", "famille.logement");
            TestWorld.Fill(decor, "_nameKey", "famille.decor");
            TestWorld.FillList(housing, "_structures", _test.Cabin);
            TestWorld.FillList(decor, "_structures", _bench);
            TestWorld.FillList(catalog, "_families", housing, decor);
#endif
            _view = new NotedBuildBarView();
            _presenter = new BuildBarPresenter(_view, catalog, TextTable.Parse(Texts, "fr"), _test.Tool,
                _test.World, new FixedToolKeys());
        }

        [TearDown]
        public void DestroyBar()
        {
            _presenter.Dispose();
            _test.Dispose();
        }

        [Test]
        public void TheBarShowsTheFamiliesAndTheOpenOne()
        {
            Assert.AreEqual(new[] { "Logement", "Décor" }, _view.Families);
            Assert.AreEqual(0, _view.OpenFamily);
            Assert.AreEqual(new[] { "Cabane" }, _view.Structures);
            Assert.AreEqual(-1, _view.Selected);
            Assert.AreEqual("", _view.Reason);
        }

        [Test]
        public void ChoosingAStructurePutsItInHand()
        {
            _view.Choose(0);

            Assert.AreSame(_test.Cabin, _test.Tool.Current);
            Assert.AreEqual(0, _view.Selected);
        }

        [Test]
        public void ARefusedPlaceSaysWhy()
        {
            _view.Choose(0);
            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center));
            _test.Tool.Confirm();
            _test.World.Step();

            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center + new Vector3(1f, 0f, 1f)));
            Assert.AreEqual("La place est prise", _view.Reason);

            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center + new Vector3(0f, 0f, -20f)));
            Assert.AreEqual("", _view.Reason);
            Assert.IsTrue(_test.Tool.Confirm(), "La place libre est visée, et la pose y est permise.");
        }

        [Test]
        public void OpeningAnotherFamilyShowsItsStructures()
        {
            _view.Open(1);

            Assert.AreEqual(1, _view.OpenFamily);
            Assert.AreEqual(new[] { "Banc" }, _view.Structures);
            _view.Choose(0);
            Assert.AreSame(_bench, _test.Tool.Current);
        }

        [Test]
        public void TheStructureInHandIsMarkedOnlyInItsOwnFamily()
        {
            _view.Choose(0);

            _view.Open(1);
            Assert.AreEqual(-1, _view.Selected, "La cabane n'est pas dans le Décor.");
            _view.Open(0);
            Assert.AreEqual(0, _view.Selected);
        }

        [Test]
        public void AFamilyThatDoesNotExistChangesNothing()
        {
            _view.Open(3);

            Assert.AreEqual(0, _view.OpenFamily);
            Assert.AreEqual(new[] { "Cabane" }, _view.Structures);
        }

        [Test]
        public void EachCardCarriesItsKey()
        {
            Assert.AreEqual(new[] { "1" }, _view.Keys, "La cabane, première carte du Logement : la touche 1.");

            _view.Open(1);
            Assert.AreEqual(new[] { "1" }, _view.Keys, "Le banc, première carte du Décor.");
        }

        [Test]
        public void EachCardSaysTheBedsItsStructureOffers()
        {
            Assert.AreEqual(new[] { "2 lits" }, _view.Beds, "La cabane offre deux lits.");

            _view.Open(1);
            Assert.AreEqual(new[] { "" }, _view.Beds, "Le banc n'en offre pas : sa carte n'en dit rien.");
        }

        [Test]
        public void CancellingClearsTheSelection()
        {
            _view.Choose(0);

            _test.Tool.Cancel();

            Assert.AreEqual(-1, _view.Selected);
        }
    }
}
