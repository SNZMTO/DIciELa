using System.Collections.Generic;
using Diciela.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Un HUD de test : le document du jeu (Hud.uxml) avec sa feuille de thème, un pixel du panneau par pixel de l'écran,
    /// et la roue des outils. Les gestes de la souris sont des événements envoyés à ses éléments, en un point du panneau
    /// (le repère d'IMGUI : origine en haut), comme UI Toolkit les distribue. Et les vérifications de sa mise en page,
    /// aux marges du GDD : ses blocs, la roue ouverte, la carte de l'outil en main.
    /// </summary>
    internal static class TestHud
    {
        private const float Side = 24f;
        private const float Top = 20f;
        private const float Bottom = 24f;
        // La tringle des fanions pend à 24 px du haut, comme dans la maquette.
        private const float StockTop = 24f;
        private const float Tolerance = 0.5f;

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
        /// Ajoute aux fautes ce qui cloche des blocs du HUD : la journée au coin haut-gauche, à 24 px du bord et 20 du
        /// haut ; la fiche ouverte au coin haut-droit ; les fanions au milieu du haut, à 24 px ; le panneau Construire
        /// ouvert au milieu du bas, à 24 px ; l'icône des outils au coin bas-droit ; l'astuce au coin bas-gauche ; un bloc
        /// hors de l'écran, ou deux blocs qui se chevauchent.
        /// </summary>
        public static void CheckBlocks(VisualElement root, Vector2Int screen, List<string> faults)
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
        /// Ajoute aux fautes ce qui cloche de la carte de l'outil en main : cachée, hors de l'écran, pas à 24 px du bas,
        /// ou sur l'icône des outils, l'astuce ou la fiche. Sa place, sans le soulèvement de son feutre (translate).
        /// </summary>
        public static void CheckCard(VisualElement root, Vector2Int screen, List<string> faults)
        {
            Rect room = root.panel.visualTree.layout;
            VisualElement card = root.Q("carte-outil");
            Rect box = card.layout;
            string at = screen.x + " × " + screen.y + " : ";
            if (card.ClassListContains("bloc-cache"))
                faults.Add(at + "la carte ne paraît pas, un outil en main.");
            if (box.xMin < 0f || box.yMin < 0f || box.xMax > room.width || box.yMax > room.height)
                faults.Add(at + "la carte sort de l'écran (" + box + " dans " + room + ").");
            if (Mathf.Abs(room.height - box.yMax - Bottom) > Tolerance)
                faults.Add(at + "la carte n'est pas à 24 px du bas (" + box + " dans " + room + ").");
            foreach (string other in new[] { "outils", "astuce", "fiche" })
            {
                if (box.Overlaps(root.Q(other).layout))
                    faults.Add(at + "la carte chevauche « " + other + " » (" + box + ", " + root.Q(other).layout + ").");
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
