using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les couleurs des faces de l'interface (décision 145), sur le HUD de test : chaque pastel est nommé une fois dans
    /// le bloc :root de la feuille du thème (Fusion.uss), et les faces le lisent de là. Une applique lavande et une
    /// applique ciel, le feutre sable de l'icône des outils, le feutre lin de la carte, le feutre lavande de la
    /// prévision et un pommeau de bois se lisent chacun de sa couleur.
    /// </summary>
    public sealed class FacetColorsTests
    {
        private static readonly CustomStyleProperty<Color> Face = new CustomStyleProperty<Color>("--face");

        [UnityTest]
        public IEnumerator FacesReadTheirPastelsFromTheTheme()
        {
            using (var test = new TestWorld())
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                yield return null;
                VisualElement root = document.rootVisualElement;
                AssertFace(root.Q(className: "astuce-tuile"), "#DBC1EA", "L'applique lavande de l'astuce");
                AssertFace(root.Q(className: "journee-tuile"), "#B7D9EE", "L'applique ciel de la journée");
                AssertFace(root.Q("outils"), "#EFD3A4", "Le feutre sable de l'icône des outils");
                AssertFace(root.Q("carte-outil"), "#FBF3E4", "Le feutre lin de la carte");
                AssertFace(root.Q("prevision"), "#DBC1EA", "Le feutre lavande de la prévision");
                AssertFace(root.Q(className: "pommeau"), "#A96F3F", "Le pommeau de bois");
            }
        }

        private static void AssertFace(VisualElement element, string hex, string what)
        {
            Assert.IsNotNull(element, what + " est dans le HUD.");
            Assert.IsTrue(element.customStyle.TryGetValue(Face, out Color face), what + " : sa face se lit.");
            Assert.IsTrue(ColorUtility.TryParseHtmlString(hex, out Color expected));
            Assert.That(Vector4.Distance(face, expected), Is.LessThan(0.01f), what + " : " + face + ".");
        }
    }
}
