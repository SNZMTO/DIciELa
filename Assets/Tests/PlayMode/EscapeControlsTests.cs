using System.Collections;
using Diciela.Controls;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le geste « Échap », joué par un clavier simulé sur les vraies actions du jeu (décision 152) : il ferme d'abord la
    /// fiche ouverte, puis, à l'appui suivant, lâche l'outil en main.
    /// </summary>
    public sealed class EscapeControlsTests : GestureFixture
    {
        private TestWorld _test;
        private EscapeControls _controls;
        private OpenSheet _sheet;

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
#if UNITY_EDITOR
            TestWorld.Fill(_controls, "_actions", _test.Keep(TestActions.CopyOfTheGame()));
#endif
            holder.SetActive(true);
            _sheet = new OpenSheet();
            _controls.Bind(_sheet, _test.Tool);
            _test.Tool.Begin(_test.Cabin);
        }

        public override void TearDown()
        {
            _controls.enabled = false;
            _test.Dispose();
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator EscapeClosesTheSheetFirstThenPutsTheToolAside()
        {
            yield return Click(Keyboard.escapeKey);
            Assert.IsFalse(_sheet.IsOpen, "La fiche se ferme d'abord.");
            Assert.IsTrue(_test.Tool.IsActive, "L'outil reste en main.");

            yield return Click(Keyboard.escapeKey);
            Assert.IsFalse(_test.Tool.IsActive, "Puis l'outil se pose de côté.");
        }
    }
}
