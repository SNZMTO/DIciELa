using System.Collections;
using Diciela.Controls;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le geste « valider », joué par un clavier simulé sur les vraies actions du jeu : Entrée, celle du pavé numérique
    /// comprise, appelle ce qu'on lui a relié ; délié, il se tait.
    /// </summary>
    public sealed class ConfirmControlsTests : GestureFixture
    {
        private InputActionAsset _actions;
        private GameObject _holder;
        private ConfirmControls _controls;
        private int _confirmed;

        public override void Setup()
        {
            base.Setup();
            _actions = TestActions.CopyOfTheGame();
            _holder = new GameObject("Geste de test");
            _holder.SetActive(false);
            _controls = _holder.AddComponent<ConfirmControls>();
#if UNITY_EDITOR
            TestWorld.Fill(_controls, "_actions", _actions);
#endif
            _holder.SetActive(true);
            _confirmed = 0;
            _controls.Bind(() => _confirmed++);
        }

        public override void TearDown()
        {
            _controls.enabled = false;
            Object.DestroyImmediate(_holder);
            Object.DestroyImmediate(_actions);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator EnterConfirmsOncePerPress()
        {
            yield return Click(Keyboard.enterKey);
            Assert.AreEqual(1, _confirmed);
            yield return Click(Keyboard.numpadEnterKey);
            Assert.AreEqual(2, _confirmed, "L'Entrée du pavé numérique valide aussi.");
        }

        [UnityTest]
        public IEnumerator UnboundItStaysSilent()
        {
            _controls.Bind(null);
            yield return Click(Keyboard.enterKey);
            Assert.AreEqual(0, _confirmed);
        }
    }
}
