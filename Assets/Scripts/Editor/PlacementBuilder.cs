using Diciela.Controls;
using Diciela.Data;
using Diciela.Game;
using Diciela.View;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Installe la pose dans une scène racine : les constructions vues (« Constructions »), l'aperçu et les gestes de
    /// la pose (« Pose »), reliés à leurs fiches, à l'asset d'actions et au démarrage. Réexécutable : ce qui est déjà
    /// réglé n'est pas touché.
    /// </summary>
    internal static class PlacementBuilder
    {
        private const string StructuresObjectName = "Constructions";
        private const string PlacementObjectName = "Pose";

        public static void EnsureIn(Scene scene, GameRunner runner)
        {
            var settings = EditorAssets.Load<PlacementSettings>(DataAssetBuilder.PlacementSettingsPath);
            var gestures = EditorAssets.Load<GestureSettings>(DataAssetBuilder.GestureSettingsPath);
            var actions = EditorAssets.Load<InputActionAsset>(InputActionsBuilder.ActionsPath);

            StructureViews views = EditorAssets.FindOrCreate<StructureViews>(scene, StructuresObjectName);
            EditorAssets.AssignIfMissing(views, "_settings", settings);

            PlacementPreview preview = EditorAssets.FindOrCreate<PlacementPreview>(scene, PlacementObjectName);
            EditorAssets.AssignIfMissing(preview, "_settings", settings);
            PlacementControls controls = EditorAssets.FindInScene<PlacementControls>(scene);
            if (controls == null)
                controls = preview.gameObject.AddComponent<PlacementControls>();
            EditorAssets.AssignIfMissing(controls, "_actions", actions);
            EditorAssets.AssignIfMissing(controls, "_settings", settings);
            EditorAssets.AssignIfMissing(controls, "_gestures", gestures);

            EditorAssets.AssignIfMissing(runner, "_placementSettings", settings);
            EditorAssets.AssignIfMissing(runner, "_structureViews", views);
            EditorAssets.AssignIfMissing(runner, "_placementPreview", preview);
            EditorAssets.AssignIfMissing(runner, "_placementControls", controls);
        }
    }
}
