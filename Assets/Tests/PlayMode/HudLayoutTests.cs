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
    /// écrans larges compris : la journée tient au coin haut-gauche, à 24 px du bord et 20 du haut ; la fiche du
    /// campement, ouverte, au coin haut-droit ; le stock en fanions au milieu du haut, à 24 px ; le panneau Construire,
    /// ouvert, au milieu du bas, à 24 px ; l'icône des outils au coin bas-droit, à 24 px ; tout reste dans l'écran, sans que deux
    /// blocs se chevauchent. La roue des outils, ouverte sur leur icône, est ramenée dans l'écran. C'est le vrai HUD (sa mise en page, la feuille du jeu,
    /// ses vues, ses présentateurs, sa taille), avec le catalogue et les textes du projet pour la barre et la journée, et
    /// le petit monde du campement pour la fiche et les fanions. Les marges attendues sont celles du GDD.
    /// </summary>
    public sealed class HudLayoutTests
    {
        private const float Side = 24f;
        private const float Top = 20f;
        private const float Bottom = 24f;
        // La tringle des fanions pend à 24 px du haut, comme dans la maquette.
        private const float StockTop = 24f;
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
                var bar = host.AddComponent<BuildBar>();
                var day = host.AddComponent<DayBoard>();
                var sheet = host.AddComponent<PlaceSheet>();
                var stock = host.AddComponent<StockPennants>();
                var tip = host.AddComponent<TipBoard>();
                var wheel = host.AddComponent<ToolWheel>();
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
                            var tooltips = new Tooltips(document);
                            bar.Show(catalog, texts, world.Tool, world.Place, world.World, new FixedToolKeys(),
                                new ReasonBubble(document));
                            bar.Open();
                            wheel.Show(texts, new FixedToolKeys(), world.Clearing, bar, world.Mover, tooltips);
                            day.Show(world.World.Clock, texts, tooltips);
                            town.InstallTheCamp();
                            stock.Show(town.World, town.Catalog, texts, tooltips);
                            sheet.Show(town.World, texts);
                            sheet.Touch(town.PlacedCamp);
                            tip.Show(town.World, texts, null);
                        }
                        for (int frame = 0; frame < SettleFrames; frame++)
                            yield return null;
                        Check(document.rootVisualElement, screen, faults);
                        wheel.OpenAt(OnScreen(document.rootVisualElement,
                            document.rootVisualElement.Q("outils").worldBound.center));
                        for (int frame = 0; frame < SettleFrames; frame++)
                            yield return null;
                        CheckWheel(document.rootVisualElement, screen, faults);
                        wheel.Escape();
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
                    wheel.Hide();
                    bar.Hide();
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

        private static void Check(VisualElement root, Vector2Int screen, List<string> faults)
        {
            Rect room = root.panel.visualTree.layout;
            Rect day = root.Q("journee").worldBound;
            Rect bar = root.Q("bar").worldBound;
            // L'icône des outils par sa place, sans l'enfoncement de son feutre (translate), qui s'anime quand la roue
            // se ferme.
            Rect tools = root.Q("outils").layout;
            Rect sheet = root.Q("fiche").worldBound;
            Rect stock = root.Q(className: "stock-corps").worldBound;
            Rect tip = root.Q("astuce").worldBound;
            string at = screen.x + " × " + screen.y + " : ";
            if (Mathf.Abs(day.xMin - Side) > Tolerance || Mathf.Abs(day.yMin - Top) > Tolerance)
                faults.Add(at + "la journée n'est pas à 24 px du bord et 20 du haut (" + day + ").");
            if (Mathf.Abs(room.height - bar.yMax - Bottom) > Tolerance)
                faults.Add(at + "la barre n'est pas à 24 px du bas (" + bar + " dans " + room + ").");
            if (Mathf.Abs(bar.center.x - room.width * 0.5f) > Tolerance)
                faults.Add(at + "la barre n'est pas au milieu (" + bar + " dans " + room + ").");
            if (Mathf.Abs(room.width - tools.xMax - Side) > Tolerance ||
                Mathf.Abs(room.height - tools.yMax - Bottom) > Tolerance)
                faults.Add(at + "l'icône des outils n'est pas à 24 px du bord et du bas (" + tools + " dans " + room +
                           ").");
            if (Mathf.Abs(room.width - sheet.xMax - Side) > Tolerance || Mathf.Abs(sheet.yMin - Top) > Tolerance)
                faults.Add(at + "la fiche n'est pas à 24 px du bord et 20 du haut (" + sheet + " dans " + room + ").");
            if (Mathf.Abs(stock.center.x - room.width * 0.5f) > Tolerance || Mathf.Abs(stock.yMin - StockTop) > Tolerance)
                faults.Add(at + "les fanions ne sont pas au milieu, à 24 px du haut (" + stock + ").");
            if (Mathf.Abs(tip.xMin - Side) > Tolerance || Mathf.Abs(room.height - tip.yMax - Bottom) > Tolerance)
                faults.Add(at + "l'astuce n'est pas à 24 px du bord et du bas (" + tip + " dans " + room + ").");
            Rect[] blocks = { day, stock, sheet, bar, tip, tools };
            foreach (Rect block in blocks)
            {
                if (block.xMin < 0f || block.yMin < 0f || block.xMax > room.width || block.yMax > room.height)
                    faults.Add(at + "un bloc sort de l'écran (" + block + " dans " + room + ").");
            }
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = i + 1; j < blocks.Length; j++)
                {
                    if (blocks[i].Overlaps(blocks[j]))
                        faults.Add(at + "deux blocs se chevauchent (" + blocks[i] + ", " + blocks[j] + ").");
                }
            }
        }

        private static void CheckWheel(VisualElement root, Vector2Int screen, List<string> faults)
        {
            Rect room = root.panel.visualTree.layout;
            foreach (string name in new[] { "roue-plateau", "roue-construire", "roue-retirer", "roue-deplacer" })
            {
                Rect tool = root.Q(name).worldBound;
                if (tool.xMin < 0f || tool.yMin < 0f || tool.xMax > room.width || tool.yMax > room.height)
                    faults.Add(screen.x + " × " + screen.y + " : la roue ouverte sur l'icône sort de l'écran (" + name +
                               " " + tool + " dans " + room + ").");
            }
        }

        /// <summary>
        /// Le point de l'écran (origine en bas) où le panneau voit ce point : la conversion d'HudArea, inversée par deux
        /// points connus.
        /// </summary>
        private static Vector2 OnScreen(VisualElement root, Vector2 panelPoint)
        {
            Vector2 origin = HudArea.ToPanel(root.panel, new Vector2(0f, Screen.height));
            Vector2 unit = HudArea.ToPanel(root.panel, new Vector2(1f, Screen.height - 1f)) - origin;
            var fromTop = new Vector2((panelPoint.x - origin.x) / unit.x, (panelPoint.y - origin.y) / unit.y);
            return new Vector2(fromTop.x, Screen.height - fromTop.y);
        }
    }
}
