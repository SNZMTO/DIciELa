using System.Collections;
using Diciela.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le démarrage de la scène de test, quitté dans l'image même où il lance le chargement de sa carte (dès que la
    /// scène est chargée, avant son premier Update) : la carte doit partir quand même, à la fin de son chargement.
    /// La vraie carte n'est jamais chargée.
    /// </summary>
    public sealed class GameRunnerTests
    {
        private const float WaitSeconds = 2f;
        private const int MaxFrames = 2400;

        [UnityTest]
        public IEnumerator LeavingWhileTheMapLoadsUnloadsItOnceLoaded()
        {
            void LeaveAtOnce(Scene scene, LoadSceneMode mode)
            {
                if (scene.path != ScenePaths.GameTest)
                    return;
                SceneManager.sceneLoaded -= LeaveAtOnce;
                SceneManager.UnloadSceneAsync(scene);
            }

            bool mapWasLoaded = false;
            void NoteMap(Scene scene, LoadSceneMode mode)
            {
                if (scene.path == ScenePaths.MapTest)
                    mapWasLoaded = true;
            }

            SceneManager.sceneLoaded += LeaveAtOnce;
            SceneManager.sceneLoaded += NoteMap;
            try
            {
#if UNITY_EDITOR
                yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePaths.GameTest,
                    new LoadSceneParameters(LoadSceneMode.Additive));
#endif
                float start = Time.realtimeSinceStartup;
                for (int frame = 0; frame < MaxFrames && Time.realtimeSinceStartup - start < WaitSeconds; frame++)
                    yield return null;

                Assert.IsTrue(mapWasLoaded, "La carte de test a fini de se charger avant de partir.");
                Assert.IsFalse(SceneManager.GetSceneByPath(ScenePaths.GameTest).isLoaded, "La scène de test est quittée.");
                Assert.IsFalse(SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded,
                    "La carte doit partir avec son démarrage, même quitté pendant qu'elle se charge.");
            }
            finally
            {
                SceneManager.sceneLoaded -= LeaveAtOnce;
                SceneManager.sceneLoaded -= NoteMap;
                if (SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded)
                    SceneManager.UnloadSceneAsync(ScenePaths.MapTest);
            }
        }
    }
}
