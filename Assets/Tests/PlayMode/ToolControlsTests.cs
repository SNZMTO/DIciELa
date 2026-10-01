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
    /// choisissent tout droit ; sans roue, rien ne part. Les noms des touches se lisent sur les liaisons.
    /// </summary>
    public sealed class ToolControlsTests : GestureFixture
    {
        private GameObject _holder;
        private InputActionAsset _actions;
        private ToolControls _controls;
        private NotedWheel _wheel;

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
            _controls.Bind(_wheel);
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
        public IEnumerator EachToolHasItsOwnKey()
        {
            yield return Click(Keyboard.bKey);
            yield return Click(Keyboard.xKey);

            Assert.AreEqual(new[] { ToolChoice.Build, ToolChoice.Clear }, _wheel.Chosen);
            Assert.IsNull(_wheel.Opened, "Une touche d'outil n'ouvre pas la roue.");
        }

        [UnityTest]
        public IEnumerator WithoutAWheelNothingHappens()
        {
            _controls.Bind(null);

            yield return Click(Keyboard.tabKey);
            yield return Click(Keyboard.bKey);

            Assert.IsNull(_wheel.Opened);
            Assert.IsNull(_wheel.Released);
            Assert.IsEmpty(_wheel.Chosen);
        }

        [Test]
        public void TheKeysAreReadOnTheBindings()
        {
            Assert.AreEqual("Tab", _controls.WheelKey);
            Assert.AreEqual("B", _controls.KeyOf(ToolChoice.Build));
            Assert.AreEqual("X", _controls.KeyOf(ToolChoice.Clear));
        }
    }
}
