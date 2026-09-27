using System;
using Diciela.Data;
using Diciela.Game;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée les fiches qui manquent. Réexécutable : une fiche qui existe déjà n'est jamais touchée, pour ne pas
    /// écraser un réglage fait à la main.
    /// </summary>
    public static class DataAssetBuilder
    {
        private const string SettingsFolder = "Assets/Data/Settings";
        internal const string SimulationSettingsPath = SettingsFolder + "/SimulationSettings.asset";
        internal const string GameSessionPath = SettingsFolder + "/Session_Jeu.asset";
        internal const string TestSessionPath = SettingsFolder + "/Session_Test.asset";
        internal const string MapCreationPath = SettingsFolder + "/CarteNeuve.asset";
        internal const string TestMapCreationPath = SettingsFolder + "/CarteNeuve_Test.asset";

        [MenuItem("DICIELA/Données/Créer les fiches manquantes")]
        public static void CreateMissingAssets()
        {
            EditorAssets.EnsureFolder(SettingsFolder);

            CreateIfMissing<SimulationSettings>(SimulationSettingsPath, null);
            CreateIfMissing<SessionSettings>(GameSessionPath,
                fiche => EditorAssets.Property(fiche, "_mapScenePath").stringValue = ScenePaths.Map);
            CreateIfMissing<SessionSettings>(TestSessionPath,
                fiche => EditorAssets.Property(fiche, "_mapScenePath").stringValue = ScenePaths.MapTest);
            CreateIfMissing<MapCreationSettings>(MapCreationPath, null);
            CreateIfMissing<MapCreationSettings>(TestMapCreationPath, FillTestMapCreation);

            AssetDatabase.SaveAssets();
        }

        /// <summary>La carte de test : petite, plate, vite chargée.</summary>
        private static void FillTestMapCreation(SerializedObject fiche)
        {
            EditorAssets.Property(fiche, "_sizeMeters").floatValue = 64f;
            EditorAssets.Property(fiche, "_maxHeightMeters").floatValue = 50f;
            EditorAssets.Property(fiche, "_groundHeightMeters").floatValue = 10f;
            EditorAssets.Property(fiche, "_heightmapResolution").intValue = 65;
            EditorAssets.Property(fiche, "_alphamapResolution").intValue = 64;
        }

        private static void CreateIfMissing<T>(string path, Action<SerializedObject> fill) where T : ScriptableObject
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null)
                return;

            T fiche = ScriptableObject.CreateInstance<T>();
            if (fill != null)
            {
                var serialized = new SerializedObject(fiche);
                fill(serialized);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.CreateAsset(fiche, path);
            Debug.Log("DICIELA : fiche créée : " + path + ".");
        }
    }
}
