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
    /// écrans larges compris : la journée tient au coin haut-gauche, à 24 px du bord et 20 du haut ; la fiche du tas,
    /// ouverte, au coin haut-droit ; le stock en fanions au milieu du haut, à 24 px ; la barre au milieu du bas, à 24 px,
    /// la tente plantée, avec le bouton de l'outil « Retirer » à gauche des onglets ; tout reste dans l'écran, sans que
    /// deux blocs se chevauchent. C'est le vrai HUD (sa mise en page, la feuille du jeu,
    /// ses vues, ses présentateurs, sa taille), avec le catalogue et les textes du projet pour la barre et la journée, et
    /// le petit monde du tas pour la fiche et les fanions. Les marges attendues sont celles du GDD.
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
            using (var town = new TestPileTown())
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
                            bar.Show(catalog, texts, world.Tool, world.Clearing, world.World, tooltips);
                            PlantTheHome(world, catalog);
                            day.Show(world.World.Clock, texts, tooltips);
                            town.PlantTheTent();
                            stock.Show(town.World, town.Catalog, texts, tooltips);
                            sheet.Show(town.World, texts);
                            sheet.Touch(town.PlacedPile);
                            tip.Show(town.World, texts, null);
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

        /// <summary>
        /// Plante le logis du catalogue, par l'outil de pose, comme le joueur : la barre propose alors le reste de sa
        /// famille et le bouton de l'outil « Retirer ».
        /// </summary>
        private static void PlantTheHome(TestWorld world, Catalog catalog)
        {
            StructureDefinition home = null;
            foreach (StructureFamilyDefinition family in catalog.Families)
            {
                foreach (StructureDefinition structure in family.Structures)
                {
                    if (home == null && structure != null && structure.IsChimeraHome)
                        home = structure;
                }
            }
            Assert.IsNotNull(home, "Le catalogue du jeu a un logis pour la Chimère.");
            world.Tool.Begin(home);
            world.Tool.AimAt(world.OnScreen(TestWorld.Center));
            Assert.IsTrue(world.Tool.Confirm(), "Le centre du petit monde est libre.");
            world.World.Step();
        }

        private static void Check(VisualElement root, Vector2Int screen, List<string> faults)
        {
            Rect room = root.panel.visualTree.layout;
            Rect day = root.Q("journee").worldBound;
            Rect bar = root.Q("bar").worldBound;
            VisualElement clearing = root.Q("outil-retirer");
            Rect tabs = root.Q("families").worldBound;
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
            if (clearing.resolvedStyle.display == DisplayStyle.None || !bar.Contains(clearing.worldBound.center) ||
                clearing.worldBound.xMax > tabs.xMin)
                faults.Add(at + "l'outil « Retirer » n'est pas dans la barre, à gauche des onglets (" +
                           clearing.worldBound + ", " + tabs + ").");
            if (Mathf.Abs(room.width - sheet.xMax - Side) > Tolerance || Mathf.Abs(sheet.yMin - Top) > Tolerance)
                faults.Add(at + "la fiche n'est pas à 24 px du bord et 20 du haut (" + sheet + " dans " + room + ").");
            if (Mathf.Abs(stock.center.x - room.width * 0.5f) > Tolerance || Mathf.Abs(stock.yMin - StockTop) > Tolerance)
                faults.Add(at + "les fanions ne sont pas au milieu, à 24 px du haut (" + stock + ").");
            if (Mathf.Abs(tip.xMin - Side) > Tolerance || Mathf.Abs(room.height - tip.yMax - Bottom) > Tolerance)
                faults.Add(at + "l'astuce n'est pas à 24 px du bord et du bas (" + tip + " dans " + room + ").");
            Rect[] blocks = { day, stock, sheet, bar, tip };
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
    }
}
