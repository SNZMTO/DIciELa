using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le présentateur de la barre de construction, sur le petit monde des tests et une vue qui note ce qu'on lui montre
    /// et joue les choix du joueur. Un catalogue d'une famille (Logement : la cabane), une table des textes écrite ici.
    /// </summary>
    public sealed class BuildBarPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "famille.logement,Logement\n" +
                                     "construction.cabane,Cabane\n" +
                                     "pose.refus.Overlaps,La place est prise\n";

        private TestWorld _test;
        private NotedView _view;
        private BuildBarPresenter _presenter;

        [SetUp]
        public void CreateBar()
        {
            _test = new TestWorld();
            var housing = _test.Keep(ScriptableObject.CreateInstance<StructureFamilyDefinition>());
            var catalog = _test.Keep(ScriptableObject.CreateInstance<Catalog>());
#if UNITY_EDITOR
            TestWorld.Fill(housing, "_nameKey", "famille.logement");
            var family = new UnityEditor.SerializedObject(housing);
            family.FindProperty("_structures").arraySize = 1;
            family.FindProperty("_structures").GetArrayElementAtIndex(0).objectReferenceValue = _test.Cabin;
            family.ApplyModifiedPropertiesWithoutUndo();
            var root = new UnityEditor.SerializedObject(catalog);
            root.FindProperty("_families").arraySize = 1;
            root.FindProperty("_families").GetArrayElementAtIndex(0).objectReferenceValue = housing;
            root.ApplyModifiedPropertiesWithoutUndo();
#endif
            _view = new NotedView();
            _presenter = new BuildBarPresenter(_view, catalog, TextTable.Parse(Texts, "fr"), _test.Tool);
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
            Assert.AreEqual(new[] { "Logement" }, _view.Families);
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
        }

        [Test]
        public void AFamilyThatDoesNotExistChangesNothing()
        {
            _view.Open(3);

            Assert.AreEqual(0, _view.OpenFamily);
            Assert.AreEqual(new[] { "Cabane" }, _view.Structures);
        }

        [Test]
        public void CancellingClearsTheSelection()
        {
            _view.Choose(0);

            _test.Tool.Cancel();

            Assert.AreEqual(-1, _view.Selected);
        }

        /// <summary>Une vue qui note ce que le présentateur lui montre, et joue les clics du joueur.</summary>
        private sealed class NotedView : IBuildBarView
        {
            public event Action<int> FamilyChosen;
            public event Action<int> StructureChosen;

            public List<string> Families { get; } = new List<string>();
            public List<string> Structures { get; } = new List<string>();
            public int OpenFamily { get; private set; } = -1;
            public int Selected { get; private set; } = -1;
            public string Reason { get; private set; } = "";

            public void Choose(int structure)
            {
                StructureChosen?.Invoke(structure);
            }

            public void Open(int family)
            {
                FamilyChosen?.Invoke(family);
            }

            public void ShowFamilies(IReadOnlyList<string> names, int open)
            {
                Families.Clear();
                Families.AddRange(names);
                OpenFamily = open;
            }

            public void ShowStructures(IReadOnlyList<string> names)
            {
                Structures.Clear();
                Structures.AddRange(names);
            }

            public void SelectStructure(int index)
            {
                Selected = index;
            }

            public void ShowReason(string text)
            {
                Reason = text;
            }
        }
    }
}
