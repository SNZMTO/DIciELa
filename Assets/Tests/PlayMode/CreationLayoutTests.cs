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
    /// Le panneau de l'écran de création à toutes les tailles d'écran (décision 162), rendu hors écran de 1280 × 720 à la
    /// 4K : ouvert, il se voit, à droite, tout entier dans l'écran, avec ses images (le bois, le ruban, le bouton qui
    /// valide). C'est la vraie mise en page, la feuille du jeu et sa taille d'interface.
    /// </summary>
    public sealed class CreationLayoutTests
    {
        private const int SettleFrames = 4;

        private static readonly Vector2Int[] Screens =
        {
            new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080), new Vector2Int(2560, 1440),
            new Vector2Int(3440, 1440), new Vector2Int(3840, 2160)
        };

        [UnityTest]
        public IEnumerator TheOpenPanelStaysOnScreenOnTheRightWithItsImages()
        {
#if UNITY_EDITOR
            var theme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/UI/Theme.tss");
            var layout = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Creation.uxml");
            var settings = BuildBarTests.LoadSingle<InterfaceSettings>();
            var faults = new List<string>();
            using (var world = new TestWorld())
            {
                var panel = world.Keep(ScriptableObject.CreateInstance<PanelSettings>());
                panel.themeStyleSheet = theme;
                panel.scaleMode = PanelScaleMode.ConstantPixelSize;
                GameObject host = world.Keep(new GameObject("Création de test"));
                host.SetActive(false);
                var document = host.AddComponent<UIDocument>();
                document.panelSettings = panel;
                document.visualTreeAsset = layout;
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
                            document.rootVisualElement.Q("panel").RemoveFromClassList("bloc-cache");
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
                }
            }
            Assert.IsEmpty(faults, string.Join("\n", faults));
#else
            yield break;
#endif
        }

        private static void Check(VisualElement root, Vector2Int screen, List<string> faults)
        {
            string at = screen.x + " × " + screen.y + " : ";
            VisualElement panel = root.Q("panel");
            Rect box = panel.worldBound;
            Rect whole = root.worldBound;
            if (box.width < 100f || box.height < 100f)
                faults.Add(at + "le panneau ne se voit pas (" + box + ").");
            if (box.xMin < whole.xMin || box.xMax > whole.xMax + 0.5f || box.yMin < whole.yMin ||
                box.yMax > whole.yMax + 0.5f)
                faults.Add(at + "le panneau déborde de l'écran (" + box + " dans " + whole + ").");
            if (box.center.x < whole.center.x)
                faults.Add(at + "le panneau n'est pas à droite.");
            if (panel.resolvedStyle.backgroundImage.texture == null)
                faults.Add(at + "le panneau n'a pas son bois.");
            if (root.Q(className: "ruban").resolvedStyle.backgroundImage.texture == null)
                faults.Add(at + "le titre n'a pas son ruban.");
            if (root.Q("confirm").resolvedStyle.backgroundImage.texture == null)
                faults.Add(at + "le bouton qui valide n'a pas son image.");
        }
    }
}
