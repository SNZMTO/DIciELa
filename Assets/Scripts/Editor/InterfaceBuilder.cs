using Diciela.Game;
using Diciela.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Installe l'interface dans une scène racine : le panneau UI Toolkit (ses réglages et son thème), la barre de
    /// construction (« Interface ») et l'écran de création (« Création »), chacun avec son document, reliés à la scène du
    /// démarrage. Réexécutable : ce qui est déjà réglé n'est pas touché.
    /// </summary>
    internal static class InterfaceBuilder
    {
        private const string Folder = "Assets/UI";
        private const string PanelPath = Folder + "/Panneau.asset";
        private const string ThemePath = Folder + "/Theme.tss";
        private const string BarLayoutPath = Folder + "/BarreConstruction.uxml";
        private const string CreationLayoutPath = Folder + "/Creation.uxml";
        private const string InterfaceObjectName = "Interface";
        private const string CreationObjectName = "Création";

        // L'écran de référence : l'interface garde sa taille relative, à mi-chemin entre la largeur et la hauteur.
        private static readonly Vector2Int ReferenceResolution = new Vector2Int(1920, 1080);
        private const float WidthOrHeight = 0.5f;

        public static void EnsureIn(Scene scene, GameStage stage)
        {
            PanelSettings panel = PanelIfMissing();
            BuildBar bar = EditorAssets.FindOrCreate<BuildBar>(scene, InterfaceObjectName);
            Document(bar, panel, BarLayoutPath);
            EditorAssets.AssignIfMissing(stage, "_buildBar", bar);

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
