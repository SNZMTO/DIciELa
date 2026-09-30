using Diciela.Controls;
using Diciela.Data;
using Diciela.Game;
using Diciela.View;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Installe la pose dans une scène racine : les constructions vues (« Constructions »), les arbres et rochers vus
    /// (« Nature »), l'aperçu et les gestes de la pose (« Pose »), le geste « toucher » (« Choisir »), reliés à leurs fiches, à l'asset d'actions, au
    /// démarrage et à sa scène. Réexécutable : ce qui est déjà réglé n'est pas touché.
    /// </summary>
    internal static class PlacementBuilder
    {
        private const string StructuresObjectName = "Constructions";
        private const string NatureObjectName = "Nature";
        private const string PlacementObjectName = "Pose";
        private const string ChooseObjectName = "Choisir";

        public static void EnsureIn(Scene scene, GameRunner runner, GameStage stage)
        {
            var settings = EditorAssets.Load<PlacementSettings>(DataAssetBuilder.PlacementSettingsPath);
            var gestures = EditorAssets.Load<GestureSettings>(DataAssetBuilder.GestureSettingsPath);
            var actions = EditorAssets.Load<InputActionAsset>(InputActionsBuilder.ActionsPath);

            StructureViews views = EditorAssets.FindOrCreate<StructureViews>(scene, StructuresObjectName);
            EditorAssets.AssignIfMissing(views, "_settings", settings);
            NatureViews nature = EditorAssets.FindOrCreate<NatureViews>(scene, NatureObjectName);
            EditorAssets.AssignIfMissing(nature, "_settings", settings);

            PlacementPreview preview = EditorAssets.FindOrCreate<PlacementPreview>(scene, PlacementObjectName);
            EditorAssets.AssignIfMissing(preview, "_settings", settings);
            PlacementControls controls = EditorAssets.FindInScene<PlacementControls>(scene);
            if (controls == null)
                controls = preview.gameObject.AddComponent<PlacementControls>();
            EditorAssets.AssignIfMissing(controls, "_actions", actions);
            EditorAssets.AssignIfMissing(controls, "_settings", settings);
            EditorAssets.AssignIfMissing(controls, "_gestures", gestures);

            ChooseControls choose = EditorAssets.FindOrCreate<ChooseControls>(scene, ChooseObjectName);
            EditorAssets.AssignIfMissing(choose, "_actions", actions);
            EditorAssets.AssignIfMissing(choose, "_gestures", gestures);

            EditorAssets.AssignIfMissing(runner, "_placementSettings", settings);
            EditorAssets.AssignIfMissing(stage, "_structureViews", views);
            EditorAssets.AssignIfMissing(stage, "_natureViews", nature);
            EditorAssets.AssignIfMissing(stage, "_placementPreview", preview);
            EditorAssets.AssignIfMissing(stage, "_placementControls", controls);
            EditorAssets.AssignIfMissing(stage, "_chooseControls", choose);
        }
    }
}
