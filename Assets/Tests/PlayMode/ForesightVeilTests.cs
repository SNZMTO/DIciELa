using System.Collections;
using System.Linq;
using Diciela.UI;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le voile de rêverie de la prévision (décisions 228 et 230), sur le HUD de test : son image couvre tout l'écran
    /// sans capter le pointeur ; effacé tant que la prévision est éteinte, il paraît en fondu quand elle s'allume et
    /// s'efface quand elle s'éteint ; débranché, il ne la suit plus.
    /// </summary>
    public sealed class ForesightVeilTests
    {
        private const float Tolerance = 0.5f;

        [UnityTest]
        public IEnumerator TheVeilFadesInWhileForesightLasts()
        {
            using (var test = new TestWorld())
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var veil = new ForesightVeil(document);
                var foresight = new Foresight();
                veil.Show(foresight);
                yield return TestHud.Settle();
                VisualElement root = document.rootVisualElement;
                VisualElement element = root.Q("reverie");
                try
                {
                    Rect room = root.panel.visualTree.layout;
                    Rect bound = element.worldBound;
                    Assert.That(Mathf.Abs(bound.width - room.width) < Tolerance &&
                                Mathf.Abs(bound.height - room.height) < Tolerance, "Il couvre tout l'écran…");
                    Assert.IsNotNull(element.resolvedStyle.backgroundImage.texture, "… de son image…");
                    Assert.AreEqual(0f, element.resolvedStyle.opacity, 1e-3f, "… effacé tant qu'elle est éteinte…");
                    Assert.AreNotSame(element, root.panel.Pick(room.center), "… sans capter le pointeur.");
                    Assert.IsTrue(element.resolvedStyle.transitionDuration.Any(time => time.value > 0f),
                        "Il paraît et s'efface en fondu…");

                    foresight.Set(true);
                    yield return TestHud.Settle();
                    Assert.AreEqual(1f, element.resolvedStyle.opacity, 1e-3f, "… quand elle s'allume…");
                    foresight.Set(false);
                    yield return TestHud.Settle();
                    Assert.AreEqual(0f, element.resolvedStyle.opacity, 1e-3f, "… et quand elle s'éteint.");
                }
                finally
                {
                    veil.Hide();
                }
                foresight.Set(true);
                yield return TestHud.Settle();
                Assert.AreEqual(0f, element.resolvedStyle.opacity, 1e-3f, "Débranché, il ne la suit plus.");
            }
        }
    }
}
