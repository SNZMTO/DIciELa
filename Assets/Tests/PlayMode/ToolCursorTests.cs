using System.Collections;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La vue du curseur des outils (décision 221), dans un HUD de test : la Place en main, le pointeur sur la ville, le
    /// curseur se montre, la pointe du pinceau sur le pointeur, et la flèche du système se cache ; sur l'interface (l'icône
    /// des outils), il se cache et rend la flèche ; caché, il la rend toujours. Chaque silhouette a son image.
    /// </summary>
    public sealed class ToolCursorTests
    {
        private const string HiddenClass = "bloc-cache";
        private const int SettleFrames = 3;
        // La pointe du pinceau, en pixels du curseur (ToolCursorShapes).
        private static readonly Vector2 BrushTip = new Vector2(9f, 31f);
        private static readonly string[] Silhouettes =
        {
            "curseur-marteau", "curseur-hache", "curseur-main", "curseur-pinceau", "curseur-gomme", "curseur-plume",
            "curseur-viseur", "curseur-place-charrette"
        };

        [UnityTest]
        public IEnumerator TheCursorShowsOverTheTownAndGivesTheArrowBackOverTheInterface()
        {
            using (var test = new TestWorld(withPlaza: true))
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var wheel = document.GetComponent<ToolWheel>();
                wheel.Show(TextTable.Parse("clé,fr\n", "fr"), new FixedToolKeys(), test.Catalog, test.World, test.Tool,
                    test.Clearing, test.Mover, test.Paths, test.Plaza, new Tooltips(document));
                var cursor = new ToolCursor(document);
                cursor.Show(test.Tool, test.Clearing, test.Mover, test.Paths, test.Plaza, new HudArea(document));
                VisualElement root = document.rootVisualElement;
                VisualElement drawn = root.Q("curseur");
                try
                {
                    test.Plaza.Take();
                    for (int frame = 0; frame < SettleFrames; frame++)
                        yield return null;
                    var town = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                    cursor.FollowTo(town);
                    Assert.IsFalse(drawn.ClassListContains(HiddenClass), "La Place en main, sur la ville : son curseur…");
                    Assert.IsFalse(UnityEngine.Cursor.visible, "… et la flèche se cache.");
                    Vector2 pointer = HudArea.ToPanel(drawn.panel, town);
                    Translate at = drawn.style.translate.value;
                    Assert.AreEqual(pointer.x - BrushTip.x, at.x.value, 0.01f, "La pointe du pinceau sur le pointeur.");
                    Assert.AreEqual(pointer.y - BrushTip.y, at.y.value, 0.01f);

                    cursor.FollowTo(TestHud.OnScreen(root, root.Q("outils").worldBound.center));
                    Assert.IsTrue(drawn.ClassListContains(HiddenClass), "Sur l'icône des outils, la flèche…");
                    Assert.IsTrue(UnityEngine.Cursor.visible, "… rendue.");

                    cursor.FollowTo(town);
                    cursor.Hide();
                    Assert.IsTrue(drawn.ClassListContains(HiddenClass), "Caché, le curseur rend la flèche.");
                    Assert.IsTrue(UnityEngine.Cursor.visible);
                }
                finally
                {
                    cursor.Hide();
                    wheel.Hide();
                    UnityEngine.Cursor.visible = true;
                }
            }
        }

        [UnityTest]
        public IEnumerator EverySilhouetteHasItsPicture()
        {
            using (var test = new TestWorld())
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                VisualElement screen = document.rootVisualElement.Q("screen");
                var faces = new List<VisualElement>();
                foreach (string silhouette in Silhouettes)
                {
                    var face = new VisualElement();
                    face.AddToClassList(silhouette);
                    screen.Add(face);
                    faces.Add(face);
                }
                for (int frame = 0; frame < SettleFrames; frame++)
                    yield return null;
                for (int i = 0; i < faces.Count; i++)
                    Assert.IsNotNull(faces[i].resolvedStyle.backgroundImage.vectorImage,
                        "Le curseur « " + Silhouettes[i] + " » a son image (son url dans Hud.uss mène à elle).");
            }
        }
    }
}
