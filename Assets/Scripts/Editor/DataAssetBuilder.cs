using Diciela.Data;
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
        private const string SimulationSettingsPath = SettingsFolder + "/SimulationSettings.asset";

        [MenuItem("DICIELA/Données/Créer les fiches manquantes")]
        public static void CreateMissingAssets()
        {
            EnsureFolder("Assets", "Data");
            EnsureFolder("Assets/Data", "Settings");

            if (AssetDatabase.LoadAssetAtPath<SimulationSettings>(SimulationSettingsPath) != null)
            {
                Debug.Log("DICIELA : les réglages de la simulation existent déjà ; rien à créer.");
                return;
            }

            SimulationSettings settings = ScriptableObject.CreateInstance<SimulationSettings>();
            AssetDatabase.CreateAsset(settings, SimulationSettingsPath);
            AssetDatabase.SaveAssets();
            Debug.Log("DICIELA : réglages de la simulation créés dans " + SimulationSettingsPath + ".");
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
