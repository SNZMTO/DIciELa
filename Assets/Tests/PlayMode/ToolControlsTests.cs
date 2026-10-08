using System.Collections;
using System.Collections.Generic;
using Diciela.Controls;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les gestes des outils, joués par un clavier et une souris simulés sur les vraies actions du jeu (décision 168) :
    /// tenir Tab ouvre la roue sous le pointeur et la fait suivre où il vise ; relâcher choisit là où il vise ; B et X
    /// choisissent tout droit ; les touches 1 à 9 prennent les cartes (décision 184) ; le curseur suit le pointeur,
    /// image après image (décision 221) ; sans roue, rien ne part. Les noms des touches se lisent sur les liaisons ; la
    /// table des outils suit l'ordre des choix et l'asset du jeu.
    /// </summary>
    public sealed class ToolControlsTests : GestureFixture
    {
        private GameObject _holder;
        private InputActionAsset _actions;
        private ToolControls _controls;
        private NotedWheel _wheel;
        private NotedCards _cards;
        private NotedCursor _cursor;

        /// <summary>Un curseur qui note où on le mène.</summary>
        private sealed class NotedCursor : IToolCursor
        {
            public Vector2? Followed;

            public void FollowTo(Vector2 screenPoint)
            {
                Followed = screenPoint;
            }
        }

        /// <summary>Des cartes qui notent celles qu'on prend.</summary>
        private sealed class NotedCards : ICardPicker
        {
            public readonly List<int> Taken = new List<int>();

            public void TakeCard(int index)
            {
                Taken.Add(index);
            }
        }

        /// <summary>Une roue qui note ce que les gestes lui disent.</summary>
        private sealed class NotedWheel : IToolWheel
        {
            public readonly List<ToolChoice> Chosen = new List<ToolChoice>();
            public Vector2? Opened;
            public Vector2? Released;
            public Vector2? Pointed;

            public void OpenAt(Vector2 screenPoint)
            {
                Opened = screenPoint;
            }

            public void PointAt(Vector2 screenPoint)
            {
                Pointed = screenPoint;
            }

            public void ReleaseAt(Vector2 screenPoint)
            {
                Released = screenPoint;
            }

            public void Choose(ToolChoice choice)
            {
                Chosen.Add(choice);
            }
        }

        public override void Setup()
        {
            base.Setup();
            _holder = new GameObject("Outils de test");
            _holder.SetActive(false);
            _controls = _holder.AddComponent<ToolControls>();
            _actions = TestActions.CopyOfTheGame();
#if UNITY_EDITOR
            TestWorld.Fill(_controls, "_actions", _actions);
#endif
            _holder.SetActive(true);
            _wheel = new NotedWheel();
            _cards = new NotedCards();
            _cursor = new NotedCursor();
            _controls.Bind(_wheel, _cards, _cursor);
        }

        public override void TearDown()
        {
            Object.Destroy(_holder);
            Object.Destroy(_actions);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator HoldingTheWheelKeyOpensItUnderThePointerAndReleasingChooses()
        {
            Set(Mouse.position, new Vector2(300f, 200f));
            yield return Played();
            Press(Keyboard.tabKey);
            yield return Played();
            Assert.AreEqual(new Vector2(300f, 200f), _wheel.Opened, "La roue s'ouvre sous le pointeur.");

            Set(Mouse.position, new Vector2(300f, 330f));
            yield return Played();
            Assert.AreEqual(new Vector2(300f, 330f), _wheel.Pointed, "Touche tenue, la roue suit où vise le pointeur.");

            Release(Keyboard.tabKey);
            yield return Played();
            Assert.AreEqual(new Vector2(300f, 330f), _wheel.Released, "Relâcher choisit là où l'on vise.");
        }

        [UnityTest]
        public IEnumerator TheCursorFollowsThePointer()
        {
            Set(Mouse.position, new Vector2(300f, 200f));
            yield return Played();
            Assert.AreEqual(new Vector2(300f, 200f), _cursor.Followed, "Le curseur suit le pointeur…");
            Set(Mouse.position, new Vector2(420f, 260f));
            yield return Played();
            Assert.AreEqual(new Vector2(420f, 260f), _cursor.Followed, "… image après image.");
        }

        [UnityTest]
        public IEnumerator EachToolHasItsOwnKey()
        {
            yield return Click(Keyboard.bKey);
            yield return Click(Keyboard.xKey);

            Assert.AreEqual(new[] { ToolChoice.Build, ToolChoice.Clear }, _wheel.Chosen);
            Assert.IsNull(_wheel.Opened, "Une touche d'outil n'ouvre pas la roue.");
        }

        [UnityTest]
        public IEnumerator TheDigitKeysTakeTheCards()
        {
            yield return Click(Keyboard.digit1Key);
            yield return Click(Keyboard.digit9Key);

            Assert.AreEqual(new[] { 0, 8 }, _cards.Taken, "1 prend la première carte, 9 la neuvième.");
            Assert.IsEmpty(_wheel.Chosen, "Une touche de carte ne choisit pas d'outil.");
        }

        [UnityTest]
        public IEnumerator WithoutAWheelNothingHappens()
        {
            _controls.Bind(null, null, null);
            _cursor.Followed = null;

            yield return Click(Keyboard.tabKey);
            yield return Click(Keyboard.bKey);
            yield return Click(Keyboard.digit1Key);

            Assert.IsNull(_wheel.Opened);
            Assert.IsNull(_wheel.Released);
            Assert.IsEmpty(_wheel.Chosen);
            Assert.IsEmpty(_cards.Taken);
            Assert.IsNull(_cursor.Followed, "Sans curseur, rien ne le mène.");
        }

        [Test]
        public void TheKeysAreReadOnTheBindings()
        {
            Assert.AreEqual("Tab", _controls.WheelKey);
            Assert.AreEqual("B", _controls.KeyOf(ToolChoice.Build));
            Assert.AreEqual("X", _controls.KeyOf(ToolChoice.Clear));
            Assert.AreEqual("1", _controls.KeyOfCard(0));
            Assert.AreEqual("9", _controls.KeyOfCard(8));
            Assert.AreEqual("", _controls.KeyOfCard(9), "Dix cartes : la dixième n'a pas de touche.");
        }

        [Test]
        public void TheToolsTableFollowsTheChoicesAndTheGameActions()
        {
            Assert.AreEqual(System.Enum.GetValues(typeof(ToolChoice)).Length, ToolAction.All.Count, "Un outil par choix.");
            for (int i = 0; i < ToolAction.All.Count; i++)
            {
                ToolAction tool = ToolAction.All[i];
                Assert.AreEqual((ToolChoice)i, tool.Choice, "La table suit l'ordre des choix.");
                InputAction action = _actions.FindAction(ToolControls.MapName + "/" + tool.Name);
                Assert.IsNotNull(action, tool.Name);
                Assert.AreEqual(tool.DefaultBinding, action.bindings[0].path, "L'asset du jeu a sa touche d'origine.");
            }
        }
    }
}
