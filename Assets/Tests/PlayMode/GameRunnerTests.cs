using System.Collections;
using Diciela.Data;
using Diciela.Game;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le démarrage, monté en mémoire sur la carte de test (ses fiches remplies par l'Inspector, comme le ferait le
    /// monteur de scènes). On le quitte dans l'image même où il lance le chargement de sa carte : la carte doit partir
    /// quand même, à la fin de son chargement. La vraie carte n'est jamais chargée.
    /// </summary>
    public sealed class GameRunnerTests
    {
        private const float WaitSeconds = 2f;
        private const int MaxFrames = 2400;

        [UnityTest]
        public IEnumerator LeavingWhileTheMapLoadsUnloadsItOnceLoaded()
        {
            var settings = ScriptableObject.CreateInstance<SimulationSettings>();
            var session = ScriptableObject.CreateInstance<SessionSettings>();
            var cameraObject = new GameObject("Caméra de test", typeof(Camera));
            var startup = new GameObject("Démarrage de test");
            startup.SetActive(false);
            GameRunner runner = startup.AddComponent<GameRunner>();
#if UNITY_EDITOR
            var sessionFields = new UnityEditor.SerializedObject(session);
            sessionFields.FindProperty("_mapScenePath").stringValue = ScenePaths.MapTest;
            sessionFields.ApplyModifiedPropertiesWithoutUndo();
            var runnerFields = new UnityEditor.SerializedObject(runner);
            runnerFields.FindProperty("_settings").objectReferenceValue = settings;
            runnerFields.FindProperty("_session").objectReferenceValue = session;
            runnerFields.FindProperty("_cameraRig").objectReferenceValue = cameraObject.AddComponent<CameraRig>();
            runnerFields.ApplyModifiedPropertiesWithoutUndo();
#endif
            try
            {
                startup.SetActive(true);
                runner.enabled = false;

                float start = Time.realtimeSinceStartup;
                for (int frame = 0; frame < MaxFrames && Time.realtimeSinceStartup - start < WaitSeconds; frame++)
                    yield return null;

                Assert.IsFalse(SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded,
                    "La carte doit partir avec son démarrage, même quitté pendant qu'elle se charge.");
            }
            finally
            {
                if (SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded)
                    SceneManager.UnloadSceneAsync(ScenePaths.MapTest);
                Object.Destroy(startup);
                Object.Destroy(cameraObject);
                Object.Destroy(settings);
                Object.Destroy(session);
            }
        }
    }
}
