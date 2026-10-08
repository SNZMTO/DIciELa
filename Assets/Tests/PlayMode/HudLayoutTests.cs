using System.Collections;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le HUD à toutes les tailles d'écran (GDD, section 16 ; décision 144), rendu hors écran de 1280 × 720 à la 4K,
    /// écrans larges compris : la journée tient au coin haut-gauche, à 24 px du bord et 20 du haut ; la fiche du
    /// campement, ouverte, au coin haut-droit ; le stock en fanions au milieu du haut, à 24 px ; l'icône des outils au
    /// coin bas-droit, à 24 px ; l'astuce au coin bas-gauche ; tout reste dans l'écran, sans que deux blocs se
    /// chevauchent. La roue tient dans l'écran, ouverte au coin sur l'icône des outils ; la Place en main, sa carte
    /// pleine (une étoile, une raison) tient à 24 px du bas, à côté de l'icône, sans toucher l'astuce ni la fiche, et
    /// sa roue, son éventail des formes déplié vers le coin. C'est le vrai HUD (sa mise en page, la feuille du jeu, ses
    /// vues, ses présentateurs, sa taille), avec les textes du projet, et le petit monde du campement pour la fiche et
    /// les fanions. Les marges attendues sont celles du GDD.
    /// </summary>
    public sealed class HudLayoutTests
    {
        private const int SettleFrames = 4;

        private static readonly Vector2Int[] Screens =
        {
            new Vector2Int(1280, 720), new Vector2Int(1280, 800), new Vector2Int(1600, 900), new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440), new Vector2Int(2560, 1080), new Vector2Int(3440, 1440), new Vector2Int(3840, 2160)
        };

        [UnityTest]
        public IEnumerator EveryBlockStaysOnScreenAtItsMargins()
        {
#if UNITY_EDITOR
            var theme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/UI/Theme.tss");
            var layout = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Hud.uxml");
            var settings = TestAssets.LoadSingle<InterfaceSettings>();
            var faults = new List<string>();
            using (var world = new TestWorld(withPlaza: true))
            using (var town = new TestCampTown())
            {
                var panel = world.Keep(ScriptableObject.CreateInstance<PanelSettings>());
                panel.themeStyleSheet = theme;
                panel.scaleMode = PanelScaleMode.ConstantPixelSize;
                GameObject host = world.Keep(new GameObject("HUD de test"));
                host.SetActive(false);
                var document = host.AddComponent<UIDocument>();
                document.panelSettings = panel;
                document.visualTreeAsset = layout;
                var day = host.AddComponent<DayBoard>();
                var sheet = host.AddComponent<LocationSheet>();
                var stock = host.AddComponent<StockPennants>();
                var tip = host.AddComponent<TipBoard>();
                var wheel = host.AddComponent<ToolWheel>();
                var scale = host.AddComponent<InterfaceScale>();
                TestWorld.Fill(scale, "_panel", panel);
                TestWorld.Fill(scale, "_settings", settings);
                RenderTexture target = null;
                ToolCard card = null;
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
                            card = new ToolCard(document);
                            card.Show(texts, world.Tool, world.Clearing, world.Mover, world.Paths, world.Plaza,
                                world.World);
                            wheel.Show(texts, new FixedToolKeys(), world.Catalog, world.World, world.Tool,
                                world.Clearing, world.Mover, world.Paths, world.Plaza, tooltips);
                            day.Show(world.World.Clock, texts, tooltips);
                            town.InstallTheCamp();
                            stock.Show(town.World, town.Catalog, texts, tooltips);
                            sheet.Show(town.World, texts);
                            sheet.Touch(town.PlacedCamp);
                            tip.Show(town.World, texts, null);
                        }
                        for (int frame = 0; frame < SettleFrames; frame++)
                            yield return null;
                        TestHud.CheckBlocks(document.rootVisualElement, screen, faults);
                        Vector2 icon = document.rootVisualElement.Q("outils").worldBound.center;
                        wheel.OpenAt(TestHud.OnScreen(document.rootVisualElement, icon));
                        yield return TestHud.Settle();
                        TestHud.CheckWheel(document.rootVisualElement, screen, faults);
                        wheel.Escape();
                        // La Place en main, sa carte pleine : une étoile, et la raison d'un ordre refusé.
                        world.Plaza.Take();
                        world.Plaza.Options.Choose(PaintSymmetry.Star);
                        world.World.Submit(new RemoveCartSpotCommand(99));
                        world.World.Step();
                        yield return TestHud.Settle();
                        TestHud.CheckCard(document.rootVisualElement, screen, faults);
                        wheel.OpenAt(TestHud.OnScreen(document.rootVisualElement, icon));
                        wheel.PointAt(TestHud.OnScreen(document.rootVisualElement, icon + new Vector2(212f, 212f)));
                        yield return TestHud.Settle();
                        if (document.rootVisualElement.Query(className: "roue-jeton").ToList().Count == 0)
                            faults.Add(screen.x + " × " + screen.y + " : l'éventail des formes ne s'est pas déplié.");
                        TestHud.CheckWheel(document.rootVisualElement, screen, faults);
                        wheel.Escape();
                        world.Plaza.PutDown();
                    }
                }
                finally
                {
                    panel.targetTexture = null;
                    if (target != null)
                    {
                        target.Release();
                        Object.Destroy(target);
                    }
                    card?.Hide();
                    wheel.Hide();
                    day.Hide();
                    sheet.Hide();
                    stock.Hide();
                    tip.Hide();
                }
            }
            Assert.IsEmpty(faults, string.Join("\n", faults));
#else
            yield break;
#endif
        }
    }
}
