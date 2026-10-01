using System.Collections;
using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La vue de la roue des outils (décision 168), dans un HUD de test, montrée, cachée, puis montrée de nouveau, comme à
    /// une deuxième partie : elle ne s'est branchée qu'une fois, si bien qu'un clic sur l'icône ouvre la roue ; cachée,
    /// l'icône n'a plus d'infobulle. Les clics et les survols sont des événements de la souris envoyés au panneau, au
    /// milieu de l'élément, dans le repère du panneau.
    /// </summary>
    public sealed class ToolWheelTests
    {
        private const string Texts = "clé,fr\n" +
                                     "outil.construire,Construire\n" +
                                     "outil.construire.suite,Choisis une construction\n" +
                                     "outil.retirer,Retirer\n" +
                                     "outil.retirer.suite,Marque un arbre\n" +
                                     "outils.infobulle,Outils\n" +
                                     "outils.infobulle.suite,Ou tiens {touche}\n";
        private const string ClosedClass = "roue-fermee";
        private const string HiddenClass = "bloc-cache";
        private const int SettleFrames = 3;

        [UnityTest]
        public IEnumerator ShownAgainTheIconOpensTheWheelInOneClick()
        {
            using (var test = new TestWorld())
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var wheel = document.GetComponent<ToolWheel>();
                TextTable texts = TextTable.Parse(Texts, "fr");
                var build = new NotedBuildMenu();
                wheel.Show(texts, new FixedToolKeys(), test.Clearing, build, new Tooltips(document));
                wheel.Hide();
                wheel.Show(texts, new FixedToolKeys(), test.Clearing, build, new Tooltips(document));
                for (int frame = 0; frame < SettleFrames; frame++)
                    yield return null;
                VisualElement root = document.rootVisualElement;
                try
                {
                    VisualElement icon = root.Q("outils");
                    TestHud.Click(icon);
                    Assert.IsFalse(root.Q("roue").ClassListContains(ClosedClass),
                        "Un seul clic ouvre la roue (icône en " + icon.worldBound + ").");
                    Assert.IsTrue(wheel.Escape(), "Échap la referme.");
                    Assert.IsTrue(root.Q("roue").ClassListContains(ClosedClass));
                }
                finally
                {
                    wheel.Hide();
                }
            }
        }

        [UnityTest]
        public IEnumerator HiddenTheIconHasNoTooltip()
        {
            using (var test = new TestWorld())
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var wheel = document.GetComponent<ToolWheel>();
                var tooltips = new Tooltips(document);
                wheel.Show(TextTable.Parse(Texts, "fr"), new FixedToolKeys(), test.Clearing, new NotedBuildMenu(),
                    tooltips);
                for (int frame = 0; frame < SettleFrames; frame++)
                    yield return null;
                VisualElement root = document.rootVisualElement;
                VisualElement icon = root.Q("outils");
                VisualElement bubble = root.Q("infobulle");

                TestHud.Send(icon,
                    PointerEnterEvent.GetPooled(TestHud.MouseAt(icon.worldBound.center, EventType.MouseMove)));
                Assert.IsFalse(bubble.ClassListContains(HiddenClass), "Montrée, l'icône a son infobulle.");
                Assert.AreEqual("Outils", root.Q<Label>("infobulle-titre").text);
                tooltips.Hide();

                wheel.Hide();
                TestHud.Send(icon,
                    PointerEnterEvent.GetPooled(TestHud.MouseAt(icon.worldBound.center, EventType.MouseMove)));
                Assert.IsTrue(bubble.ClassListContains(HiddenClass), "Cachée, elle n'en a plus.");
            }
        }
    }
}
