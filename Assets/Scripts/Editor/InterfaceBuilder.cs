using Diciela.Game;
using Diciela.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Installe l'interface dans une scène racine : le panneau UI Toolkit (ses réglages et son thème) et la barre de
    /// construction (« Interface »), reliée au démarrage. Réexécutable : ce qui est déjà réglé n'est pas touché.
    /// </summary>
    internal static class InterfaceBuilder
    {
        private const string Folder = "Assets/UI";
        private const string PanelPath = Folder + "/Panneau.asset";
        private const string ThemePath = Folder + "/Theme.tss";
        private const string BarLayoutPath = Folder + "/BarreConstruction.uxml";
        private const string InterfaceObjectName = "Interface";

        // L'écran de référence : l'interface garde sa taille relative, à mi-chemin entre la largeur et la hauteur.
        private static readonly Vector2Int ReferenceResolution = new Vector2Int(1920, 1080);
        private const float WidthOrHeight = 0.5f;

        public static void EnsureIn(Scene scene, GameRunner runner)
        {
            PanelSettings panel = PanelIfMissing();
            var layout = EditorAssets.Load<VisualTreeAsset>(BarLayoutPath);

            BuildBar bar = EditorAssets.FindOrCreate<BuildBar>(scene, InterfaceObjectName);
            UIDocument document = bar.GetComponent<UIDocument>();
            if (document.panelSettings == null || document.visualTreeAsset == null)
            {
                if (document.panelSettings == null)
                    document.panelSettings = panel;
                if (document.visualTreeAsset == null)
                    document.visualTreeAsset = layout;
                EditorUtility.SetDirty(document);
            }
            EditorAssets.AssignIfMissing(runner, "_buildBar", bar);
        }

        private static PanelSettings PanelIfMissing()
        {
            PanelSettings existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
            if (existing != null)
                return existing;

            var panel = ScriptableObject.CreateInstance<PanelSettings>();
            panel.themeStyleSheet = EditorAssets.Load<ThemeStyleSheet>(ThemePath);
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = ReferenceResolution;
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = WidthOrHeight;
            AssetDatabase.CreateAsset(panel, PanelPath);
            Debug.Log("DICIELA : panneau d'interface créé : " + PanelPath + ".");
            return panel;
        }
    }
}
