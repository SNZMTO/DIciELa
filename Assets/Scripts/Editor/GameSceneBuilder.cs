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
    /// Monte les scènes du jeu : les fiches qui manquent, la carte de test, la carte (si elle n'existe pas), puis les
    /// deux scènes racines, Jeu et JeuTest, chacune avec son démarrage relié à ses fiches. Réexécutable : ce qui existe
    /// n'est pas touché, un réglage fait à la main n'est pas écrasé. Finit sur la scène Jeu.
    /// </summary>
    public static class GameSceneBuilder
    {
        private const string StartupObjectName = "Démarrage";

        [MenuItem("DICIELA/Scènes/Monter les scènes")]
        public static void BuildScenes()
        {
            if (!CanChangeScenes())
                return;

            DataAssetBuilder.CreateMissingAssets();
            MapSceneBuilder.CreateTestMapIfMissing();
            MapSceneBuilder.CreateMapIfMissing();
            BuildRootScene(ScenePaths.GameTest, DataAssetBuilder.TestSessionPath);
            BuildRootScene(ScenePaths.Game, DataAssetBuilder.GameSessionPath);
            EnsureInBuild(ScenePaths.Game);
            EnsureInBuild(ScenePaths.Map);
            Debug.Log("DICIELA : scènes montées.");
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

        private static void BuildRootScene(string scenePath, string sessionPath)
        {
            // La scène d'abord : l'ouvrir en mode Single décharge les assets que rien ne tient, et une fiche lue avant
            // deviendrait un objet détruit (SAVOIR, section 3).
            Scene scene = OpenOrCreate(scenePath);
            var settings = EditorAssets.Load<SimulationSettings>(DataAssetBuilder.SimulationSettingsPath);
            var session = EditorAssets.Load<SessionSettings>(sessionPath);

            GameRunner runner = FindOrCreateRunner(scene);
            EditorAssets.AssignIfMissing(runner, "_settings", settings);
            EditorAssets.AssignIfMissing(runner, "_session", session);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static Scene OpenOrCreate(string scenePath)
        {
            if (EditorAssets.Exists(scenePath))
                return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            EditorAssets.EnsureFolder(scenePath.Substring(0, scenePath.LastIndexOf('/')));
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

        private static void EnsureInBuild(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (EditorBuildSettingsScene listed in scenes)
            {
                if (listed.path != scenePath)
                    continue;
                if (!listed.enabled)
                {
                    listed.enabled = true;
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
