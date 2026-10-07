using Diciela.Data;
using Diciela.Game;
using Diciela.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Installe l'interface dans une scène racine : le panneau UI Toolkit (ses réglages et son thème) et sa taille à
    /// l'écran ; le HUD (« Interface » : le panneau Construire, la roue des outils, la journée, la fiche du lieu, le stock
    /// et l'astuce, sur un même document) et l'écran de création (« Création »), chacun avec son document, reliés à la
    /// scène du démarrage. Réexécutable : ce qui est déjà
    /// réglé n'est pas touché.
    /// </summary>
    internal static class InterfaceBuilder
    {
        private const string Folder = "Assets/UI";
        private const string PanelPath = Folder + "/Panneau.asset";
        private const string ThemePath = Folder + "/Theme.tss";
        private const string HudLayoutPath = Folder + "/Hud.uxml";
        private const string CreationLayoutPath = Folder + "/Creation.uxml";
        private const string InterfaceObjectName = "Interface";
        private const string CreationObjectName = "Création";

        public static void EnsureIn(Scene scene, GameStage stage)
        {
            PanelSettings panel = Panel();
            BuildBar bar = EditorAssets.FindOrCreate<BuildBar>(scene, InterfaceObjectName);
            Document(bar, panel, HudLayoutPath);
            EditorAssets.AssignIfMissing(stage, "_buildBar", bar);
            DayBoard day = EditorAssets.GetOrAdd<DayBoard>(bar.gameObject);
            EditorAssets.AssignIfMissing(stage, "_dayBoard", day);
            LocationSheet sheet = EditorAssets.GetOrAdd<LocationSheet>(bar.gameObject);
            EditorAssets.AssignIfMissing(stage, "_locationSheet", sheet);
            StockPennants stock = EditorAssets.GetOrAdd<StockPennants>(bar.gameObject);
            EditorAssets.AssignIfMissing(stage, "_stockPennants", stock);
            TipBoard tip = EditorAssets.GetOrAdd<TipBoard>(bar.gameObject);
            EditorAssets.AssignIfMissing(stage, "_tipBoard", tip);
            ToolWheel wheel = EditorAssets.GetOrAdd<ToolWheel>(bar.gameObject);
            EditorAssets.AssignIfMissing(stage, "_toolWheel", wheel);

            InterfaceScale scale = EditorAssets.GetOrAdd<InterfaceScale>(bar.gameObject);
            EditorAssets.AssignIfMissing(scale, "_panel", panel);
            EditorAssets.AssignIfMissing(scale, "_settings",
                EditorAssets.Load<InterfaceSettings>(DataAssetBuilder.InterfaceSettingsPath));

            CreationScreen creation = EditorAssets.FindOrCreate<CreationScreen>(scene, CreationObjectName);
            Document(creation, panel, CreationLayoutPath);
            EditorAssets.AssignIfMissing(stage, "_creationScreen", creation);
        }

        /// <summary>Le document UI Toolkit de cet écran : le panneau et sa mise en page, s'ils lui manquent.</summary>
        private static void Document(Component screen, PanelSettings panel, string layoutPath)
        {
            UIDocument document = screen.GetComponent<UIDocument>();
            if (document.panelSettings != null && document.visualTreeAsset != null)
                return;
            if (document.panelSettings == null)
                document.panelSettings = panel;
            if (document.visualTreeAsset == null)
                document.visualTreeAsset = EditorAssets.Load<VisualTreeAsset>(layoutPath);
            EditorUtility.SetDirty(document);
        }

        /// <summary>
        /// Le panneau, créé s'il manque. Il garde ses pixels (taille constante) : c'est InterfaceScale qui lui donne son
        /// échelle d'après l'écran (décision 144) ; un panneau réglé autrement y est ramené.
        /// </summary>
        private static PanelSettings Panel()
        {
            PanelSettings panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                panel.themeStyleSheet = EditorAssets.Load<ThemeStyleSheet>(ThemePath);
                panel.scaleMode = PanelScaleMode.ConstantPixelSize;
                panel.scale = 1f;
                AssetDatabase.CreateAsset(panel, PanelPath);
                Debug.Log("DICIELA : panneau d'interface créé : " + PanelPath + ".");
                return panel;
            }
            if (panel.scaleMode != PanelScaleMode.ConstantPixelSize || !Mathf.Approximately(panel.scale, 1f))
            {
                panel.scaleMode = PanelScaleMode.ConstantPixelSize;
                panel.scale = 1f;
                EditorUtility.SetDirty(panel);
                AssetDatabase.SaveAssetIfDirty(panel);
                Debug.Log("DICIELA : panneau d'interface ramené à la taille constante, que règle InterfaceScale.");
            }
            return panel;
        }
    }
}
