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
    /// Le HUD à toutes les tailles d'écran (GDD, section 16 ; décision 144), rendu hors écran de 1280 × 720 à la 4K,
    /// écrans larges compris : la journée tient au coin haut-gauche, à 24 px du bord et 20 du haut ; la barre au milieu
    /// du bas, à 24 px ; tout reste dans l'écran, sans que deux blocs se chevauchent. C'est le vrai HUD (sa mise en page,
    /// la feuille du jeu, ses vues, ses présentateurs, sa taille), avec le catalogue et les textes du projet, sur un
    /// petit monde en mémoire. Les marges attendues sont celles du GDD.
    /// </summary>
    public sealed class HudLayoutTests
    {
        private const float Side = 24f;
        private const float Top = 20f;
        private const float Bottom = 24f;
        private const float Tolerance = 0.5f;
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
            var settings = BuildBarTests.LoadSingle<InterfaceSettings>();
            var catalog = BuildBarTests.LoadSingle<Catalog>();
            var faults = new List<string>();
            using (var world = new TestWorld())
            {
                var panel = world.Keep(ScriptableObject.CreateInstance<PanelSettings>());
                panel.themeStyleSheet = theme;
                panel.scaleMode = PanelScaleMode.ConstantPixelSize;
                GameObject host = world.Keep(new GameObject("HUD de test"));
                host.SetActive(false);
                var document = host.AddComponent<UIDocument>();
                document.panelSettings = panel;
                document.visualTreeAsset = layout;
                var bar = host.AddComponent<BuildBar>();
                var day = host.AddComponent<DayBoard>();
                var scale = host.AddComponent<InterfaceScale>();
                TestWorld.Fill(scale, "_panel", panel);
                TestWorld.Fill(scale, "_settings", settings);
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
                            bar.Show(catalog, texts, world.Tool, world.World);
                            day.Show(world.World.Clock, texts);
                        }
                        for (int frame = 0; frame < SettleFrames; frame++)
                            yield return null;
                        Check(document.rootVisualElement, screen, faults);
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
                    bar.Hide();
                    day.Hide();
                }
            }
            Assert.IsEmpty(faults, string.Join("\n", faults));
#else
            yield break;
#endif
        }

        private static void Check(VisualElement root, Vector2Int screen, List<string> faults)
        {
            Rect room = root.panel.visualTree.layout;
            Rect day = root.Q("journee").worldBound;
            Rect bar = root.Q("bar").worldBound;
            string at = screen.x + " × " + screen.y + " : ";
            if (Mathf.Abs(day.xMin - Side) > Tolerance || Mathf.Abs(day.yMin - Top) > Tolerance)
                faults.Add(at + "la journée n'est pas à 24 px du bord et 20 du haut (" + day + ").");
            if (Mathf.Abs(room.height - bar.yMax - Bottom) > Tolerance)
                faults.Add(at + "la barre n'est pas à 24 px du bas (" + bar + " dans " + room + ").");
            if (Mathf.Abs(bar.center.x - room.width * 0.5f) > Tolerance)
                faults.Add(at + "la barre n'est pas au milieu (" + bar + " dans " + room + ").");
            foreach (Rect block in new[] { day, bar })
            {
                if (block.xMin < 0f || block.yMin < 0f || block.xMax > room.width || block.yMax > room.height)
                    faults.Add(at + "un bloc sort de l'écran (" + block + " dans " + room + ").");
            }
            if (day.Overlaps(bar))
                faults.Add(at + "la journée et la barre se chevauchent.");
        }
    }
}
