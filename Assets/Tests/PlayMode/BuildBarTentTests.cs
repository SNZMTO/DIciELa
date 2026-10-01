using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La tente d'abord, dans la barre de construction (GDD, section 6 ; décision 95) : tant que la Chimère n'a pas de
    /// logis, la barre ne propose que la tente ; une fois plantée, la tente s'en va (elle est unique), l'outil de pose la
    /// lâche, et le reste de la famille s'ouvre, avec le bouton de l'outil « Retirer » (décision 167). Sur le petit monde des tests, un catalogue d'une famille (Logement : la
    /// tente, puis la cabane) et une table des textes écrite ici.
    /// </summary>
    public sealed class BuildBarTentTests
    {
        private const string Texts = "clé,fr\n" +
                                     "famille.logement,Logement\n" +
                                     "construction.tente,Tente\n" +
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
            TestWorld.FillList(housing, "_structures", _test.Tent, _test.Cabin);
            TestWorld.FillList(catalog, "_families", housing);
#endif
            _view = new NotedBuildBarView();
            _presenter = new BuildBarPresenter(_view, catalog, TextTable.Parse(Texts, "fr"), _test.Tool, _test.Clearing,
                _test.World);
        }

        [TearDown]
        public void DestroyBar()
        {
            _presenter.Dispose();
            _test.Dispose();
        }

        [Test]
        public void BeforeTheTentTheBarOffersOnlyTheTent()
        {
            Assert.AreEqual(new[] { "Logement" }, _view.Families);
            Assert.AreEqual(new[] { "Tente" }, _view.Structures);
            Assert.IsFalse(_view.ClearingShown, "Avant la tente, la barre ne propose qu'elle, sans l'outil.");

            _view.Choose(0);
            Assert.AreSame(_test.Tent, _test.Tool.Current);
            Assert.AreEqual(0, _view.Selected);
        }

        [Test]
        public void OncePlantedTheTentLeavesTheBarAndTheCabinComes()
        {
            _view.Choose(0);
            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center));
            Assert.IsTrue(_test.Tool.Confirm(), "Le centre du petit monde est libre.");
            _test.World.Step();

            Assert.AreEqual(1, _test.World.Structures.All.Count);
            Assert.IsFalse(_test.Tool.IsActive, "L'outil lâche la tente : elle ne se pose plus nulle part.");
            Assert.AreEqual(new[] { "Cabane" }, _view.Structures);
            Assert.IsTrue(_view.ClearingShown, "La tente plantée, le bouton de l'outil « Retirer » paraît.");
            Assert.IsFalse(_view.ClearingHeld);
            Assert.AreEqual(-1, _view.Selected);
            Assert.AreEqual("", _view.Reason);

            _view.Choose(0);
            Assert.AreSame(_test.Cabin, _test.Tool.Current);
            Assert.AreEqual(0, _view.Selected);
        }
    }
}
