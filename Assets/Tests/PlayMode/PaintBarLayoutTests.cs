using System.Collections;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation.Land;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La barre de la Place à toutes les tailles d'écran (décision 206), rendue hors écran de 1280 × 720 à la 4K, l'outil
    /// de la Place en main, à sa plus large (le polygone et ses styles, l'étoile et ses nombres) : elle tient au milieu
    /// du bas, à 24 px, dans l'écran, sans chevaucher l'astuce (en bas à gauche) ni l'icône des outils (en bas à droite).
    /// Au-dessus d'elle, la bulle des raisons dit la plus longue (« La Place dépasserait sa superficie… ») : plus large
    /// que la bulle, elle passe à la ligne au lieu d'en déborder. C'est le vrai HUD (sa mise en page, la feuille du jeu,
    /// sa taille), avec les textes du projet, le petit monde des tests pour la Place et celui du campement pour l'astuce.
    /// Au mode Place de charrette, ses rangées d'options vides ne laissent pas de trou.
    /// </summary>
    public sealed class PaintBarLayoutTests
    {
        private const float Bottom = 24f;
        private const float Tolerance = 0.5f;
        private const int SettleFrames = 4;

        private static readonly Vector2Int[] Screens =
        {
            new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440), new Vector2Int(3440, 1440), new Vector2Int(3840, 2160)
        };

        [UnityTest]
        public IEnumerator ThePaintBarStaysBetweenTheTipAndTheTools()
        {
#if UNITY_EDITOR
            var settings = BuildBarTests.LoadSingle<InterfaceSettings>();
            var faults = new List<string>();
            using (var world = new TestWorld(withPlaza: true))
            using (var town = new TestCampTown())
            {
                var panel = world.Keep(ScriptableObject.CreateInstance<PanelSettings>());
                panel.themeStyleSheet =
                    UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/UI/Theme.tss");
                panel.scaleMode = PanelScaleMode.ConstantPixelSize;
                GameObject host = world.Keep(new GameObject("HUD de test"));
                host.SetActive(false);
                var document = host.AddComponent<UIDocument>();
                document.panelSettings = panel;
                document.visualTreeAsset =
                    UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Hud.uxml");
                var tip = host.AddComponent<TipBoard>();
                var wheel = host.AddComponent<ToolWheel>();
                var scale = host.AddComponent<InterfaceScale>();
                TestWorld.Fill(scale, "_panel", panel);
                TestWorld.Fill(scale, "_settings", settings);
                PaintBar bar = null;
                RenderTexture target = null;
                try
                {
                    foreach (Vector2Int screen in Screens)
                    {
                        if (target != null)
                        {
                            panel.targetTexture = null;
                            target.Release();
                            Object.Destroy(target);
                        }
                        target = new RenderTexture(screen.x, screen.y, 24);
                        panel.targetTexture = target;
                        if (!host.activeSelf)
                        {
                            host.SetActive(true);
                            yield return null;
                            TextTable texts = TextTable.Parse(settings.Texts.text, settings.Language);
                            var tooltips = new Tooltips(document);
                            wheel.Show(texts, new FixedToolKeys(), world.Clearing, new NotedBuildMenu(), world.Mover,
                                world.Paths, world.Plaza, tooltips);
                            tip.Show(town.World, texts, null);
                            bar = new PaintBar(document, tooltips, new ReasonBubble(document));
                            bar.Show(texts, world.Plaza, world.Paths, world.World, new FixedToolKeys());
                            world.Plaza.Take();
                            world.Plaza.Choose(PaintToolMode.Shape);
                            world.Plaza.Options.Choose(PaintShape.Polygon);
                            world.Plaza.Options.Choose(PaintSymmetry.Star);
                            bar.ShowReason(texts.Get(TextKeys.PlazaRefusal(PlazaResult.TooLarge.ToString())));
                        }
                        for (int frame = 0; frame < SettleFrames; frame++)
                            yield return null;
                        Check(document.rootVisualElement, screen, faults);
                    }

                    // Au mode Place de charrette, sans symétrie : les formes, les styles, la taille et l'étoile se cachent.
                    world.Plaza.Choose(PaintToolMode.CartSpot);
                    world.Plaza.Options.Choose(PaintSymmetry.None);
                    for (int frame = 0; frame < SettleFrames; frame++)
                        yield return null;
                    CheckEmptyRows(document.rootVisualElement, faults);
                }
                finally
                {
                    panel.targetTexture = null;
                    if (target != null)
                    {
                        target.Release();
                        Object.Destroy(target);
                    }
                    bar?.Hide();
                    wheel.Hide();
                    tip.Hide();
                }
            }
            Assert.IsEmpty(faults, string.Join("\n", faults));
#else
            yield break;
#endif
        }

        // Une rangée d'options dont tout est caché ne prend aucune place, ni hauteur ni marge : la barre n'a pas de trou ;
        // celle qui se voit (les symétries) garde son écart sous ce qui la précède.
        private static void CheckEmptyRows(VisualElement root, List<string> faults)
        {
            int empty = 0;
            int spaced = 0;
            root.Q("place-barre").Query(className: "place-options").ForEach(row =>
            {
                foreach (VisualElement child in row.Children())
                {
                    if (child.resolvedStyle.display == DisplayStyle.None)
                        continue;
                    if (child.resolvedStyle.marginTop > Tolerance)
                        spaced++;
                    return;
                }
                empty++;
                if (row.layout.height > Tolerance || row.resolvedStyle.marginTop > Tolerance)
                    faults.Add("Une rangée d'options vide prend de la place (" + row.layout + ", marge " +
                               row.resolvedStyle.marginTop + ").");
            });
            if (empty == 0)
                faults.Add("Au mode Place de charrette, aucune rangée d'options n'est vide : le test ne vérifie rien.");
            if (spaced == 0)
                faults.Add("Au mode Place de charrette, la rangée des symétries n'a pas son écart sous les modes.");
        }

        private static void Check(VisualElement root, Vector2Int screen, List<string> faults)
        {
            Rect room = root.panel.visualTree.layout;
            Rect bar = root.Q("place-barre").worldBound;
            Rect tip = root.Q("astuce").worldBound;
            Rect tools = root.Q("outils").layout;
            string at = screen.x + " × " + screen.y + " : ";
            if (bar.width <= 0f || tip.width <= 0f || tools.width <= 0f)
                faults.Add(at + "la barre, l'astuce et l'icône des outils doivent se voir (" + bar + ", " + tip + ", " +
                           tools + ").");
            if (Mathf.Abs(room.height - bar.yMax - Bottom) > Tolerance)
                faults.Add(at + "la barre de la Place n'est pas à 24 px du bas (" + bar + " dans " + room + ").");
            if (Mathf.Abs(bar.center.x - room.width * 0.5f) > Tolerance)
                faults.Add(at + "la barre de la Place n'est pas au milieu (" + bar + " dans " + room + ").");
            if (bar.xMin < 0f || bar.yMin < 0f || bar.xMax > room.width || bar.yMax > room.height)
                faults.Add(at + "la barre de la Place sort de l'écran (" + bar + " dans " + room + ").");
            if (bar.Overlaps(tip))
                faults.Add(at + "la barre de la Place chevauche l'astuce (" + bar + ", " + tip + ").");
            if (bar.Overlaps(tools))
                faults.Add(at + "la barre de la Place chevauche l'icône des outils (" + bar + ", " + tools + ").");

            Rect bubble = root.Q("reason").worldBound;
            var reason = root.Q<Label>("reason-text");
            // La raison tenue sur une seule ligne : plus large que la place qu'elle a dans la bulle, elle doit passer à la ligne.
            Vector2 line = reason.MeasureTextSize(reason.text, 0f, VisualElement.MeasureMode.Undefined, 0f,
                VisualElement.MeasureMode.Undefined);
            if (bubble.width <= 0f || bubble.yMax > bar.yMin + Tolerance)
                faults.Add(at + "la bulle des raisons doit se voir au-dessus de la barre (" + bubble + ", " + bar + ").");
            else if (line.x > reason.contentRect.width + Tolerance && reason.contentRect.height < line.y * 1.5f)
                faults.Add(at + "la raison déborde de sa bulle : " + line.x + " px sur une ligne pour " +
                           reason.contentRect.width + " px de place, sans passer à la ligne.");
        }
    }
}
