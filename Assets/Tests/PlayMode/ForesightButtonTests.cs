using System.Collections;
using Diciela.Data;
using Diciela.UI;
using Diciela.View;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le bouton de la prévision (décision 228), sur le HUD de test : il paraît avec son icône vectorielle et sa
    /// touche ; un clic allume la prévision et il reste enfoncé, puis remonte quand elle s'éteint par V ; caché à la
    /// sortie, il ne fait plus rien. Le clic est un événement de la souris envoyé au panneau, comme UI Toolkit le
    /// distribue.
    /// </summary>
    public sealed class ForesightButtonTests
    {
        private const string Texts = "clé,fr\n" +
                                     "prevision.infobulle,Prévision\n" +
                                     "prevision.infobulle.suite,Ou appuie sur {touche}\n";

        [UnityTest]
        public IEnumerator TheButtonTogglesForesightAndStaysPressedWhileItLasts()
        {
            using (var test = new TestWorld())
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var button = new ForesightButton(document);
                var foresight = new Foresight();
                button.Show(foresight, TextTable.Parse(Texts, "fr"), new Tooltips(document), "V");
                yield return null;
                VisualElement element = document.rootVisualElement.Q("prevision");
                try
                {
                    Assert.IsFalse(element.ClassListContains("bloc-cache"), "Le bouton paraît…");
                    Background picture = element.Q(className: "prevision-icone").resolvedStyle.backgroundImage;
                    Assert.IsNotNull(picture.vectorImage, "… avec son icône vectorielle…");
                    Assert.AreEqual("V", document.rootVisualElement.Q<Label>("prevision-touche").text, "… sa touche.");
                    TestHud.Click(element);
                    Assert.IsTrue(foresight.IsOn, "Un clic l'allume…");
                    Assert.IsTrue(element.ClassListContains("ouvert"), "… le bouton reste enfoncé…");
                    foresight.Toggle();
                    Assert.IsFalse(element.ClassListContains("ouvert"), "… et remonte quand elle s'éteint par V.");
                }
                finally
                {
                    button.Hide();
                }
                Assert.IsTrue(element.ClassListContains("bloc-cache"), "À la sortie, il se cache…");
                TestHud.Click(element);
                Assert.IsFalse(foresight.IsOn, "… débranché.");
            }
        }
    }
}
