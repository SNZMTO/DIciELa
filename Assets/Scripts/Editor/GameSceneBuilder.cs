using System.Collections.Generic;
using Diciela.Data;
using Diciela.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Monte la scène Jeu : une caméra, une lumière et l'objet « Démarrage », qui porte le GameRunner relié à la fiche
    /// de réglages. Réexécutable : ce qui existe déjà n'est pas touché, un réglage fait à la main n'est pas écrasé.
    /// </summary>
    public static class GameSceneBuilder
    {
        internal const string ScenesFolder = "Assets/Scenes";
        internal const string GameScenePath = ScenesFolder + "/" + SceneNames.Game + ".unity";
        private const string StartupObjectName = "Démarrage";

        [MenuItem("DICIELA/Scènes/Monter la scène Jeu")]
        public static void BuildGameScene()
        {
            if (!CanChangeScenes())
                return;

            // La scène d'abord : l'ouvrir en mode Single décharge les assets que rien ne tient, et une fiche lue avant
            // deviendrait un objet détruit (SAVOIR, section 3).
            Scene scene = OpenOrCreateGameScene();

            DataAssetBuilder.CreateMissingAssets();
            var settings = AssetDatabase.LoadAssetAtPath<SimulationSettings>(DataAssetBuilder.SimulationSettingsPath);
            if (settings == null)
            {
                Debug.LogError("DICIELA : la fiche " + DataAssetBuilder.SimulationSettingsPath + " est introuvable.");
                return;
            }

            GameRunner runner = FindOrCreateRunner(scene);
            AssignSettingsIfMissing(runner, settings);
            EditorSceneManager.SaveScene(scene, GameScenePath);
            AddToBuildScenes(GameScenePath);
            Debug.Log("DICIELA : scène " + GameScenePath + " montée.");
        }

        /// <summary>Vrai si l'on peut ouvrir une autre scène sans rien perdre ; sinon, dit pourquoi dans la console.</summary>
        internal static bool CanChangeScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("DICIELA : sortir du Play avant de changer de scène.");
                return false;
            }

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene open = SceneManager.GetSceneAt(i);
                if (open.isDirty)
                {
                    Debug.LogError("DICIELA : la scène « " + open.name + " » a des changements non enregistrés : " +
                                   "les enregistrer ou les abandonner d'abord.");
                    return false;
                }
            }
            return true;
        }

        private static Scene OpenOrCreateGameScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameScenePath) != null)
                return EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

            if (!AssetDatabase.IsValidFolder(ScenesFolder))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            return EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }

        private static GameRunner FindOrCreateRunner(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                GameRunner existing = root.GetComponentInChildren<GameRunner>(true);
                if (existing != null)
                    return existing;
            }

            var startup = new GameObject(StartupObjectName);
            SceneManager.MoveGameObjectToScene(startup, scene);
            return startup.AddComponent<GameRunner>();
        }

        private static void AssignSettingsIfMissing(GameRunner runner, SimulationSettings settings)
        {
            var serialized = new SerializedObject(runner);
            SerializedProperty property = serialized.FindProperty("_settings");
            if (property.objectReferenceValue != null)
                return;

            property.objectReferenceValue = settings;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(runner);
        }

        private static void AddToBuildScenes(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (EditorBuildSettingsScene listed in scenes)
            {
                if (listed.path != path)
                    continue;
                if (!listed.enabled)
                {
                    listed.enabled = true;
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
