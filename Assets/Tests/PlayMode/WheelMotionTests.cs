using System.Collections;
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
    /// Les mouvements de la roue (décision 225), dans un HUD de test : ouverte d'un clic sur l'icône des outils, elle
    /// part de son état de départ, son plateau et ses cases, puis se pose ; une case prise, son icône part de la roue
    /// vers le curseur, et la roue se ferme ; ouverte sur la Place, la case Outils la retourne, puis elle se pose de
    /// nouveau. Une case survolée déplie son éventail depuis elle, et un clic aussitôt sur elle la prend : ses jetons,
    /// qui la couvrent en partant, ne volent pas le clic.
    /// </summary>
    public sealed class WheelMotionTests
    {
        private const string EnterClass = "roue-entree";
        private const string FlipClass = "roue-retournee";
        private const string FlightStartClass = "roue-vol-depart";
        private const string ClosedClass = "roue-fermee";

        [UnityTest]
        public IEnumerator TheWheelSpringsOpenSettlesFlipsAndLetsItsIconFly()
        {
            using (var test = new TestWorld(withPlaza: true))
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var wheel = document.GetComponent<ToolWheel>();
                wheel.Show(TextTable.Parse("clé,fr\n", "fr"), new FixedToolKeys(), test.Catalog, test.World, test.Tool,
                    test.Clearing, test.Mover, test.Paths, test.Plaza, new Tooltips(document));
                yield return TestHud.Settle();
                VisualElement root = document.rootVisualElement;
                VisualElement center = root.Q("roue-centre");
                try
                {
                    TestHud.Click(root.Q("outils"));
                    Assert.IsTrue(center.ClassListContains(EnterClass), "Ouverte, la roue part petite et penchée…");
                    Assert.Greater(root.Q("roue-cases").Query(className: EnterClass).ToList().Count, 0,
                        "… et ses cases aussi.");
                    yield return TestHud.Settle();
                    Assert.IsFalse(center.ClassListContains(EnterClass), "Puis elle se pose…");
                    Assert.AreEqual(0, root.Q("roue-cases").Query(className: EnterClass).ToList().Count,
                        "… ses cases à leur place.");

                    TestHud.Click(root.Q("roue-cases").Q(className: "roue-place").parent);
                    Assert.IsTrue(test.Plaza.IsActive, "La Place prise…");
                    Assert.IsTrue(root.Q("roue").ClassListContains(ClosedClass), "… la roue se ferme…");
                    Assert.IsTrue(root.Q("roue-vol").ClassListContains(FlightStartClass), "… et son icône s'envole.");
                    yield return TestHud.Settle();
                    Assert.IsFalse(root.Q("roue-vol").ClassListContains(FlightStartClass), "Le vol est fini.");

                    wheel.OpenAt(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
                    yield return TestHud.Settle();
                    TestHud.Click(root.Q("roue-cases").Q(className: "roue-retour").parent);
                    Assert.IsTrue(center.ClassListContains(FlipClass), "La case Outils retourne la roue…");
                    yield return TestHud.Settle();
                    Assert.IsFalse(center.ClassListContains(FlipClass), "… qui se pose sur les outils.");
                }
                finally
                {
                    wheel.Hide();
                }
            }
        }

        [UnityTest]
        public IEnumerator AFanUnfoldingFromItsCaseLetsTheCaseTakeTheClick()
        {
            using (var test = new TestWorld(withPlaza: true))
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var wheel = document.GetComponent<ToolWheel>();
                var card = new ToolCard(document);
                TextTable texts = TextTable.Parse("clé,fr\n", "fr");
                wheel.Show(texts, new FixedToolKeys(), test.Catalog, test.World, test.Tool, test.Clearing, test.Mover,
                    test.Paths, test.Plaza, new Tooltips(document));
                card.Show(texts, test.Tool, test.Clearing, test.Mover, test.Paths, test.Plaza, test.World);
                test.Plaza.Take();
                yield return TestHud.Settle();
                VisualElement root = document.rootVisualElement;
                try
                {
                    TestHud.Click(root.Q("carte-outil"));
                    yield return TestHud.Settle();
                    VisualElement shape = root.Q("roue-cases").Q(className: "place-icone-shape").parent;
                    TestHud.Send(shape,
                        PointerEnterEvent.GetPooled(TestHud.MouseAt(shape.worldBound.center, EventType.MouseMove)));
                    Assert.Greater(root.Q("roue-eventail").childCount, 0, "Survolée, la case Forme déplie ses formes…");
                    TestHud.Click(shape);
                    Assert.AreEqual(PaintToolMode.Shape, test.Plaza.Mode,
                        "… et un clic aussitôt sur elle la prend : ses jetons, partis d'elle, ne volent pas le clic.");
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
