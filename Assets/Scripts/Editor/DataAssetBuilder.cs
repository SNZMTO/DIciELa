using Diciela.Data;
using Diciela.Game;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée les fiches qui manquent : les réglages, puis le contenu. Réexécutable : une fiche qui existe déjà n'est
    /// jamais touchée, pour ne pas écraser un réglage fait à la main.
    /// </summary>
    public static class DataAssetBuilder
    {
        private const string SettingsFolder = "Assets/Data/Settings";
        internal const string SimulationSettingsPath = SettingsFolder + "/SimulationSettings.asset";
        internal const string GameSessionPath = SettingsFolder + "/Session_Jeu.asset";
        internal const string TestSessionPath = SettingsFolder + "/Session_Test.asset";
        internal const string MapCreationPath = SettingsFolder + "/CarteNeuve.asset";
        internal const string TestMapCreationPath = SettingsFolder + "/CarteNeuve_Test.asset";
        internal const string CameraSettingsPath = SettingsFolder + "/CameraSettings.asset";
        internal const string GestureSettingsPath = SettingsFolder + "/GestureSettings.asset";
        internal const string PlacementSettingsPath = SettingsFolder + "/PlacementSettings.asset";
        internal const string InterfaceSettingsPath = SettingsFolder + "/InterfaceSettings.asset";
        private const string TextsPath = "Assets/Localization/strings.csv";
        private const string TranslucentMaterialPath = "Assets/Art/Vue/Translucide.mat";
        private const string SiteMaterialPath = "Assets/Art/Vue/Chantier.mat";
        private const string SiteGhostMaterialPath = "Assets/Art/Vue/ChantierFantome.mat";
        private const string FloorGhostMaterialPath = "Assets/Art/Vue/SolFantome.mat";

        [MenuItem("DICIELA/Données/Créer les fiches manquantes")]
        public static void CreateMissingAssets()
        {
            EditorAssets.CreateIfMissing<SimulationSettings>(SimulationSettingsPath, null);
            EditorAssets.CreateIfMissing<SessionSettings>(GameSessionPath,
                fiche => EditorAssets.Property(fiche, "_mapScenePath").stringValue = ScenePaths.Map);
            EditorAssets.CreateIfMissing<SessionSettings>(TestSessionPath,
                fiche => EditorAssets.Property(fiche, "_mapScenePath").stringValue = ScenePaths.MapTest);
            EditorAssets.CreateIfMissing<MapCreationSettings>(MapCreationPath, null);
            EditorAssets.CreateIfMissing<MapCreationSettings>(TestMapCreationPath, FillTestMapCreation);
            EditorAssets.CreateIfMissing<CameraSettings>(CameraSettingsPath, null);
            EditorAssets.CreateIfMissing<GestureSettings>(GestureSettingsPath, null);
            var placement = EditorAssets.CreateIfMissing<PlacementSettings>(PlacementSettingsPath, fiche =>
                EditorAssets.Property(fiche, "_translucentMaterial").objectReferenceValue =
                    MaterialBuilder.TranslucentIfMissing(TranslucentMaterialPath));
            EditorAssets.AssignIfMissing(placement, "_siteMaterial",
                MaterialBuilder.OnShaderIfMissing(SiteMaterialPath, "Diciela/Chantier"));
            EditorAssets.AssignIfMissing(placement, "_siteGhostMaterial",
                MaterialBuilder.OnShaderIfMissing(SiteGhostMaterialPath, "Diciela/ChantierFantome"));
            EditorAssets.AssignIfMissing(placement, "_floorGhostMaterial",
                MaterialBuilder.OnShaderIfMissing(FloorGhostMaterialPath, "Diciela/SolFantome"));
            EditorAssets.CreateIfMissing<InterfaceSettings>(InterfaceSettingsPath, fiche =>
                EditorAssets.Property(fiche, "_texts").objectReferenceValue = EditorAssets.Load<TextAsset>(TextsPath));
            ContentBuilder.CreateMissing();

            AssetDatabase.SaveAssets();
            // Réenregistrer les fiches de réglages : un champ ajouté au code y entre avec sa valeur, un champ retiré en
            // sort. Sans cela, la fiche garde une vieille vérité et le jeu lit celle du code (SAVOIR, section 3).
            AssetDatabase.ForceReserializeAssets(SettingsPaths);
        }

        private static readonly string[] SettingsPaths =
        {
            SimulationSettingsPath, GameSessionPath, TestSessionPath, MapCreationPath, TestMapCreationPath,
            CameraSettingsPath, GestureSettingsPath, PlacementSettingsPath, InterfaceSettingsPath
        };

        /// <summary>La carte de test : petite, plate, vite chargée.</summary>
        private static void FillTestMapCreation(SerializedObject fiche)
        {
            EditorAssets.Property(fiche, "_sizeMeters").floatValue = 64f;
            EditorAssets.Property(fiche, "_maxHeightMeters").floatValue = 50f;
            EditorAssets.Property(fiche, "_groundHeightMeters").floatValue = 10f;
            EditorAssets.Property(fiche, "_heightmapResolution").intValue = 65;
            EditorAssets.Property(fiche, "_alphamapResolution").intValue = 64;
        }
    }
}
