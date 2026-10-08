using System.Collections;
using Diciela.Controls;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le geste de la prévision, joué par un clavier simulé sur les vraies actions du jeu (décision 228) : V l'allume,
    /// V l'éteint, et sa touche s'écrit « V » ; délié, V ne fait plus rien.
    /// </summary>
    public sealed class ForesightControlsTests : GestureFixture
    {
        private GameObject _holder;
        private InputActionAsset _actions;
        private ForesightControls _controls;

        public override void Setup()
        {
            base.Setup();
            _holder = new GameObject("Prévision de test");
            _holder.SetActive(false);
            _controls = _holder.AddComponent<ForesightControls>();
            _actions = TestActions.CopyOfTheGame();
#if UNITY_EDITOR
            TestWorld.Fill(_controls, "_actions", _actions);
#endif
            _holder.SetActive(true);
        }

        public override void TearDown()
        {
            _controls.enabled = false;
            Object.Destroy(_holder);
            Object.Destroy(_actions);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator VTurnsForesightOnThenOff()
        {
            var foresight = new Foresight();
            _controls.Bind(foresight);
            Assert.AreEqual("V", _controls.Key, "Sa touche, telle que l'étiquette l'écrit.");

            yield return Click(Keyboard.vKey);
            Assert.IsTrue(foresight.IsOn, "V l'allume…");
            yield return Click(Keyboard.vKey);
            Assert.IsFalse(foresight.IsOn, "… V l'éteint.");

            _controls.Bind(null);
            yield return Click(Keyboard.vKey);
            Assert.IsFalse(foresight.IsOn, "Délié, V ne fait plus rien.");
        }
    }
}
