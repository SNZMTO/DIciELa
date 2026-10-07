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
    /// Là où est le HUD pour les gestes (HudArea ; ARCHITECTURE, section 11), sur le vrai document : la roue ouverte par
    /// son icône couvre la ville de son fond ; un appui à côté des outils la congédie (décision 168), et cet appui reste à
    /// l'interface pendant son image, roue disparue : la ville ne le prend pas pour elle, ni pour poser la construction en
    /// main. À l'image suivante, le milieu de l'écran est de nouveau la ville. L'appui est envoyé au panneau comme UI
    /// Toolkit le distribue, avant les LateUpdate des gestes (SAVOIR, 2.8).
    /// </summary>
    public sealed class HudAreaTests
    {
        private const string Texts = "clé,fr\n" +
                                     "outil.construire,Construire\n" +
                                     "outil.construire.suite,Choisis une construction\n" +
                                     "outil.retirer,Retirer\n" +
                                     "outil.retirer.suite,Marque un arbre\n" +
                                     "outils.infobulle,Outils\n" +
                                     "outils.infobulle.suite,Ou tiens {touche}\n";
        private const string ClosedClass = "roue-fermee";
        private const int SettleFrames = 3;

        [UnityTest]
        public IEnumerator ThePressThatDismissesTheWheelStaysTheInterfaces()
        {
            using (var test = new TestWorld())
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var wheel = document.GetComponent<ToolWheel>();
                wheel.Show(TextTable.Parse(Texts, "fr"), new FixedToolKeys(), test.Clearing, new NotedBuildMenu(),
                    test.Mover, test.Paths, test.Plaza, new Tooltips(document));
                var area = new HudArea(document);
                for (int frame = 0; frame < SettleFrames; frame++)
                    yield return null;
                VisualElement root = document.rootVisualElement;
                VisualElement ring = root.Q("roue");
                try
                {
                    Vector2 middle = root.worldBound.center;
                    Vector2 middleOnScreen = TestHud.OnScreen(root.panel, middle);
                    Assert.IsFalse(area.Contains(middleOnScreen), "Roue fermée, le milieu de l'écran est la ville.");

                    TestHud.Click(root.Q("outils"));
                    Assert.IsFalse(ring.ClassListContains(ClosedClass), "L'icône ouvre la roue.");
                    Assert.IsTrue(area.Contains(middleOnScreen), "Ouverte, son fond couvre la ville.");

                    TestHud.Send(ring, PointerDownEvent.GetPooled(TestHud.MouseAt(middle, EventType.MouseDown)));
                    Assert.IsTrue(ring.ClassListContains(ClosedClass), "Un appui à côté des outils la congédie.");
                    Assert.IsTrue(area.Contains(middleOnScreen),
                        "L'appui qui la congédie reste à l'interface, roue disparue : la ville ne le voit pas.");

                    TestHud.Send(ring, PointerUpEvent.GetPooled(TestHud.MouseAt(middle, EventType.MouseUp)));
                    yield return null;
                    Assert.IsFalse(area.Contains(middleOnScreen),
                        "À l'image suivante, le milieu de l'écran est de nouveau la ville.");
                }
                finally
                {
                    wheel.Hide();
                }
            }
        }
    }
}
