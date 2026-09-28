using System.Collections;
using System.Text.RegularExpressions;
using Diciela.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La scène de test, jouée comme par le joueur : on la charge, elle charge sa carte de test, on la laisse tourner,
    /// on la quitte. Le journal du jeu dit si le monde a reçu sa carte, si la Chimère y est née et s'il a avancé ; toute
    /// erreur dans la console fait échouer le test. La vraie carte n'est jamais chargée.
    /// </summary>
    public sealed class GameSceneTests
    {
        private const float RunSeconds = 1f;
        private const int MaxFrames = 1200;

        [UnityTest]
        public IEnumerator TheWorldReceivesTheTestMapAndSteps()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"^DICIELA : monde créé, carte de \d+ × \d+ m \(relief de \d+ × \d+ points\)"));
            LogAssert.Expect(LogType.Log, new Regex(@"^DICIELA : l'habitant n° 1 \(chimere\) arrive en "));
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePaths.GameTest,
                new LoadSceneParameters(LoadSceneMode.Additive));
#endif
            float start = Time.realtimeSinceStartup;
            for (int frame = 0; frame < MaxFrames && Time.realtimeSinceStartup - start < RunSeconds; frame++)
                yield return null;

            Assert.IsTrue(SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded, "La carte de test doit être chargée.");
            Assert.IsFalse(SceneManager.GetSceneByPath(ScenePaths.Map).isLoaded, "La vraie carte ne doit jamais l'être.");

            LogAssert.Expect(LogType.Log, new Regex(@"^DICIELA : monde arrêté après [1-9]\d* pas, jour 1, "));
            yield return SceneManager.UnloadSceneAsync(ScenePaths.GameTest);
            for (int frame = 0; frame < MaxFrames && SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded; frame++)
                yield return null;

            Assert.IsFalse(SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded,
                "La carte de test doit partir avec sa scène de jeu.");
        }
    }
}
