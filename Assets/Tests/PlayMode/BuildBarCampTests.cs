using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le campement d'abord, dans le panneau Construire (GDD, section 6 ; décision 95) : tant que la Chimère n'a pas de
    /// logis, le panneau ne propose que le campement ; une fois installé, le campement s'en va (il est unique), l'outil de pose la
    /// lâche, et le reste de la famille s'ouvre. Sur le petit monde des tests, un catalogue d'une famille (Logement : la
    /// campement, puis la cabane) et une table des textes écrite ici.
    /// </summary>
    public sealed class BuildBarCampTests
    {
        private const string Texts = "clé,fr\n" +
                                     "famille.logement,Logement\n" +
                                     "construction.campement,Campement\n" +
                                     "construction.cabane,Cabane\n" +
                                     "pose.refus.AlreadyPlaced,Déjà posée\n";

        private TestWorld _test;
        private NotedBuildBarView _view;
        private BuildBarPresenter _presenter;

        [SetUp]
        public void CreateBar()
        {
            _test = new TestWorld();
            var housing = _test.Keep(ScriptableObject.CreateInstance<StructureFamilyDefinition>());
            var catalog = _test.Keep(ScriptableObject.CreateInstance<Catalog>());
#if UNITY_EDITOR
            TestWorld.Fill(housing, "_nameKey", "famille.logement");
            TestWorld.FillList(housing, "_structures", _test.Camp, _test.Cabin);
            TestWorld.FillList(catalog, "_families", housing);
#endif
            _view = new NotedBuildBarView();
            _presenter = new BuildBarPresenter(_view, catalog, TextTable.Parse(Texts, "fr"), _test.Tool, _test.World);
        }

        [TearDown]
        public void DestroyBar()
        {
            _presenter.Dispose();
            _test.Dispose();
        }

        [Test]
        public void BeforeTheCampTheBarOffersOnlyTheCamp()
        {
            Assert.AreEqual(new[] { "Logement" }, _view.Families);
            Assert.AreEqual(new[] { "Campement" }, _view.Structures);

            _view.Choose(0);
            Assert.AreSame(_test.Camp, _test.Tool.Current);
            Assert.AreEqual(0, _view.Selected);
        }

        [Test]
        public void OncePlantedTheCampLeavesTheBarAndTheCabinComes()
        {
            _view.Choose(0);
            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center));
            Assert.IsTrue(_test.Tool.Confirm(), "Le centre du petit monde est libre.");
            _test.World.Step();

            Assert.AreEqual(1, _test.World.Structures.All.Count);
            Assert.IsFalse(_test.Tool.IsActive, "L'outil lâche le campement : il ne se pose plus nulle part.");
            Assert.AreEqual(new[] { "Cabane" }, _view.Structures);
            Assert.AreEqual(-1, _view.Selected);
            Assert.AreEqual("", _view.Reason);

            _view.Choose(0);
            Assert.AreSame(_test.Cabin, _test.Tool.Current);
            Assert.AreEqual(0, _view.Selected);
        }
    }
}
