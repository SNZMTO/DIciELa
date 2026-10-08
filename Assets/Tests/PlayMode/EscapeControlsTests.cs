using System.Collections;
using Diciela.Controls;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le geste « Échap », joué par un clavier simulé sur les vraies actions du jeu (décisions 152 et 229), dans
    /// l'ordre du jeu : il ferme d'abord la fiche ouverte, puis, à l'appui suivant, lâche l'outil en main, puis éteint
    /// la prévision.
    /// </summary>
    public sealed class EscapeControlsTests : GestureFixture
    {
        private TestWorld _test;
        private EscapeControls _controls;
        private OpenSheet _sheet;
        private Hand _hand;
        private ForesightControls _foresightControls;
        private Foresight _foresight;

        /// <summary>Une fiche ouverte, qu'Échap ferme une fois.</summary>
        private sealed class OpenSheet : IEscapable
        {
            public bool IsOpen = true;

            public bool Escape()
            {
                if (!IsOpen)
                    return false;
                IsOpen = false;
                return true;
            }
        }

        public override void Setup()
        {
            base.Setup();
            _test = new TestWorld();
            GameObject holder = _test.Keep(new GameObject("Échap de test"));
            holder.SetActive(false);
            _controls = holder.AddComponent<EscapeControls>();
            _foresightControls = holder.AddComponent<ForesightControls>();
#if UNITY_EDITOR
            TestWorld.Fill(_controls, "_actions", _test.Keep(TestActions.CopyOfTheGame()));
            TestWorld.Fill(_foresightControls, "_actions", _test.Keep(TestActions.CopyOfTheGame()));
#endif
            holder.SetActive(true);
            _sheet = new OpenSheet();
            _hand = new Hand(_test.Tool);
            _foresight = new Foresight();
            _foresight.Set(true);
            _foresightControls.Bind(_foresight);
            _controls.Bind(_sheet, _hand, _foresightControls);
            _test.Tool.Begin(_test.Cabin);
        }

        public override void TearDown()
        {
            _controls.enabled = false;
            _foresightControls.enabled = false;
            _hand.Dispose();
            _test.Dispose();
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator EscapeClosesTheSheetThenPutsTheToolAsideThenEndsForesight()
        {
            yield return Click(Keyboard.escapeKey);
            Assert.IsFalse(_sheet.IsOpen, "La fiche se ferme d'abord.");
            Assert.IsTrue(_test.Tool.IsActive, "L'outil reste en main…");
            Assert.IsTrue(_foresight.IsOn, "… et la prévision dure.");

            yield return Click(Keyboard.escapeKey);
            Assert.IsFalse(_test.Tool.IsActive, "Puis l'outil se pose de côté…");
            Assert.IsTrue(_foresight.IsOn, "… la prévision dure encore.");

            yield return Click(Keyboard.escapeKey);
            Assert.IsFalse(_foresight.IsOn, "Enfin, la prévision s'éteint.");
        }
    }
}
