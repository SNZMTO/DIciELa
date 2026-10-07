using System.Collections.Generic;
using Diciela.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Un HUD de test : le document du jeu (Hud.uxml) avec sa feuille de thème, un pixel du panneau par pixel de l'écran,
    /// et la roue des outils. Les gestes de la souris sont des événements envoyés à ses éléments, en un point du panneau
    /// (le repère d'IMGUI : origine en haut), comme UI Toolkit les distribue.
    /// </summary>
    internal static class TestHud
    {
        /// <summary>Le document du HUD, la roue des outils posée dessus ; détruit avec le banc.</summary>
        public static UIDocument WithWheel(TestWorld test)
        {
#if UNITY_EDITOR
            var panel = test.Keep(ScriptableObject.CreateInstance<PanelSettings>());
            panel.themeStyleSheet =
                UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/UI/Theme.tss");
            panel.scaleMode = PanelScaleMode.ConstantPixelSize;
            GameObject host = test.Keep(new GameObject("HUD de test"));
            host.SetActive(false);
            var document = host.AddComponent<UIDocument>();
            document.panelSettings = panel;
            document.visualTreeAsset =
                UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Hud.uxml");
            host.AddComponent<ToolWheel>();
            host.SetActive(true);
            return document;
#else
            throw new System.NotSupportedException("Le HUD de test se monte dans l'éditeur.");
#endif
        }

        /// <summary>Un événement de la souris en ce point du panneau, du bouton gauche.</summary>
        public static Event MouseAt(Vector2 panelPoint, EventType type)
        {
            return new Event { type = type, mousePosition = panelPoint, button = 0, clickCount = 1 };
        }

        /// <summary>Un clic au milieu de l'élément : appuyer puis relâcher au même endroit.</summary>
        public static void Click(VisualElement element)
        {
            Vector2 middle = element.worldBound.center;
            Send(element, PointerDownEvent.GetPooled(MouseAt(middle, EventType.MouseDown)));
            Send(element, PointerUpEvent.GetPooled(MouseAt(middle, EventType.MouseUp)));
        }

        public static void Send(VisualElement target, EventBase pointerEvent)
        {
            using (pointerEvent)
            {
                pointerEvent.target = target;
                target.SendEvent(pointerEvent);
            }
        }

        /// <summary>Ce point du panneau sur l'écran, en pixels depuis le bas à gauche, comme le lit le pointeur.</summary>
        public static Vector2 OnScreen(IPanel panel, Vector2 panelPoint)
        {
            Vector2 origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
            Vector2 unit = RuntimePanelUtils.ScreenToPanel(panel, Vector2.one) - origin;
            var fromTop = new Vector2((panelPoint.x - origin.x) / unit.x, (panelPoint.y - origin.y) / unit.y);
            return new Vector2(fromTop.x, Screen.height - fromTop.y);
        }

        /// <summary>
        /// Ajoute aux fautes ce qui sort de l'écran de la roue ouverte : son plateau, ses cases et les jetons de son
        /// éventail.
        /// </summary>
        public static void CheckWheel(VisualElement root, Vector2Int screen, List<string> faults)
        {
            Rect room = root.panel.visualTree.layout;
            var parts = new List<VisualElement> { root.Q("roue-plateau") };
            parts.AddRange(root.Q("roue").Query(className: "roue-outil").ToList());
            parts.AddRange(root.Q("roue").Query(className: "roue-jeton").ToList());
            foreach (VisualElement part in parts)
            {
                Rect box = part.worldBound;
                if (box.xMin < 0f || box.yMin < 0f || box.xMax > room.width || box.yMax > room.height)
                    faults.Add(screen.x + " × " + screen.y + " : la roue ouverte au coin sort de l'écran (" +
                               string.Join(" ", part.GetClasses()) + " " + box + " dans " + room + ").");
            }
        }

        /// <summary>
        /// Le point de l'écran (origine en bas) où le panneau voit ce point : la conversion d'HudArea, inversée par deux
        /// points connus.
        /// </summary>
        public static Vector2 OnScreen(VisualElement root, Vector2 panelPoint)
        {
            Vector2 origin = HudArea.ToPanel(root.panel, new Vector2(0f, Screen.height));
            Vector2 unit = HudArea.ToPanel(root.panel, new Vector2(1f, Screen.height - 1f)) - origin;
            var fromTop = new Vector2((panelPoint.x - origin.x) / unit.x, (panelPoint.y - origin.y) / unit.y);
            return new Vector2(fromTop.x, Screen.height - fromTop.y);
        }
    }
}
