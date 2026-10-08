using System.Collections;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La vue de la roue (décisions 168 et 221), dans un HUD de test, montrée, cachée, puis montrée de nouveau, comme à
    /// une deuxième partie : elle ne s'est branchée qu'une fois, si bien qu'un clic sur l'icône ouvre la roue ; cachée,
    /// l'icône n'a plus d'infobulle ; ouverte, chaque outil montre son image ; un clic sur la carte de l'outil en main
    /// l'ouvre sur ses réglages, et la carte s'efface tant qu'elle est ouverte. Les clics et les survols sont des
    /// événements de la souris envoyés au panneau, au milieu de l'élément, dans le repère du panneau.
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
        private const string FadedClass = "carte-outil-effacee";
        private const string ToolClass = "roue-outil";
        private const string PictureClass = "roue-icone";
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
                wheel.Show(texts, new FixedToolKeys(), test.Catalog, test.World, test.Tool, test.Clearing, test.Mover,
                    test.Paths, test.Plaza, new Tooltips(document));
                wheel.Hide();
                wheel.Show(texts, new FixedToolKeys(), test.Catalog, test.World, test.Tool, test.Clearing, test.Mover,
                    test.Paths, test.Plaza, new Tooltips(document));
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
                wheel.Show(TextTable.Parse(Texts, "fr"), new FixedToolKeys(), test.Catalog, test.World, test.Tool,
                    test.Clearing, test.Mover, test.Paths, test.Plaza, tooltips);
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

        [UnityTest]
        public IEnumerator OpenedEachToolShowsItsPicture()
        {
            // Un catalogue avec une Place, sans logis à poser : la ville la propose déjà.
            using (var test = new TestWorld(withPlaza: true))
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var wheel = document.GetComponent<ToolWheel>();
                wheel.Show(TextTable.Parse(Texts, "fr"), new FixedToolKeys(), test.Catalog, test.World, test.Tool,
                    test.Clearing, test.Mover, test.Paths, test.Plaza, new Tooltips(document));
                for (int frame = 0; frame < SettleFrames; frame++)
                    yield return null;
                VisualElement root = document.rootVisualElement;
                try
                {
                    TestHud.Click(root.Q("outils"));
                    for (int frame = 0; frame < SettleFrames; frame++)
                        yield return null;
                    List<VisualElement> tools = root.Q("roue").Query(className: ToolClass).ToList();
                    Assert.AreEqual(System.Enum.GetValues(typeof(ToolChoice)).Length, tools.Count,
                        "La roue offre chaque outil.");
                    foreach (VisualElement tool in tools)
                    {
                        // Les icônes de l'interface sont des images vectorielles (InterfaceIcons) : une texture serait celle
                        // qu'Unity montre à la place d'une image introuvable.
                        Background picture = tool.Q(className: PictureClass).resolvedStyle.backgroundImage;
                        Assert.IsNotNull(picture.vectorImage,
                            "L'outil « " + tool.name + " » montre son icône (son url dans Hud.uss mène à elle).");
                    }
                }
                finally
                {
                    wheel.Hide();
                }
            }
        }

        [UnityTest]
        public IEnumerator TheCardOpensTheSettingsOfTheToolInHandAndFadesWhileTheWheelIsOpen()
        {
            using (var test = new TestWorld(withPlaza: true))
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var wheel = document.GetComponent<ToolWheel>();
                var card = new ToolCard(document);
                TextTable texts = TextTable.Parse(Texts, "fr");
                wheel.Show(texts, new FixedToolKeys(), test.Catalog, test.World, test.Tool, test.Clearing, test.Mover,
                    test.Paths, test.Plaza, new Tooltips(document));
                card.Show(texts, test.Tool, test.Clearing, test.Mover, test.Paths, test.Plaza, test.World);
                test.Plaza.Take();
                for (int frame = 0; frame < SettleFrames; frame++)
                    yield return null;
                VisualElement root = document.rootVisualElement;
                VisualElement onCard = root.Q("carte-outil");
                try
                {
                    Assert.IsFalse(onCard.ClassListContains(HiddenClass), "La Place en main, sa carte paraît.");
                    TestHud.Click(onCard);
                    Assert.IsFalse(root.Q("roue").ClassListContains(ClosedClass), "Un clic sur la carte ouvre la roue…");
                    Assert.IsNotNull(root.Q("roue-cases").Q(className: "place-icone-brush"),
                        "… sur les réglages de la Place.");
                    Assert.IsTrue(onCard.ClassListContains(FadedClass), "La carte s'efface tant que la roue est ouverte…");
                    Assert.IsTrue(wheel.Escape());
                    Assert.IsFalse(onCard.ClassListContains(FadedClass), "… et revient quand elle se ferme.");
                }
                finally
                {
                    card.Hide();
                    wheel.Hide();
                }
            }
        }
    }
}
