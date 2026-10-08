using System.Collections;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La vue de la carte de l'outil en main (décision 225), dans un HUD de test, menée comme son présentateur la mène :
    /// montrée, elle glisse en place ; un nombre qui change défile jusqu'à sa valeur, sa suite gardée ; lâchée, elle
    /// glisse hors de l'écran puis se cache ; reprise en partant, elle revient sans se cacher.
    /// </summary>
    public sealed class ToolCardTests
    {
        private const string HiddenClass = "bloc-cache";
        private const string EnterClass = "carte-outil-entree";

        [UnityTest]
        public IEnumerator TheCardSlidesInAndOutAndItsNumbersRoll()
        {
            using (var test = new TestWorld())
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var card = new ToolCard(document);
                VisualElement drawn = document.rootVisualElement.Q("carte-outil");
                try
                {
                    card.ShowTool(HudFace.Styled("roue-place"), "peche", "La Place", default, "");
                    card.ShowChips(new[] { new ToolCardChip(default, "10 / 100 m²") });
                    Assert.IsFalse(drawn.ClassListContains(HiddenClass), "Un outil en main : la carte paraît…");
                    Assert.IsTrue(drawn.ClassListContains(EnterClass), "… depuis son état de départ.");
                    yield return TestHud.Settle();
                    Assert.IsFalse(drawn.ClassListContains(EnterClass), "Elle a rejoint sa place.");

                    card.ShowChips(new[] { new ToolCardChip(default, "40 / 100 m²") });
                    Label area = document.rootVisualElement.Q("carte-outil-puces").Q<Label>();
                    Assert.AreEqual("10 / 100 m²", area.text, "Le nombre part de sa valeur d'avant…");
                    yield return TestHud.Settle();
                    Assert.AreEqual("40 / 100 m²", area.text, "… et défile jusqu'à la nouvelle.");

                    card.ShowNothing();
                    Assert.IsFalse(drawn.ClassListContains(HiddenClass), "Lâchée, elle glisse d'abord…");
                    card.ShowTool(HudFace.Styled("roue-place"), "peche", "La Place", default, "");
                    yield return TestHud.Settle();
                    Assert.IsFalse(drawn.ClassListContains(HiddenClass), "… et reprise en partant, elle reste.");
                    card.ShowNothing();
                    yield return TestHud.Settle();
                    Assert.IsTrue(drawn.ClassListContains(HiddenClass), "… puis se cache.");
                }
                finally
                {
                    card.Hide();
                }
            }
        }
    }
}
