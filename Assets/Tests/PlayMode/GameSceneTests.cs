using System.Collections;
using System.Text.RegularExpressions;
using Diciela.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La scène Jeu, jouée comme par le joueur : on la charge, on la laisse tourner, on la quitte. Le journal du jeu
    /// dit si le monde a été créé et s'il a avancé ; toute erreur dans la console fait échouer le test.
    /// </summary>
    public sealed class GameSceneTests
    {
        private const float RunSeconds = 0.5f;
        private const int MaxFrames = 600;

        [UnityTest]
        public IEnumerator TheWorldStepsWhileTheGameSceneRuns()
        {
            LogAssert.Expect(LogType.Log, new Regex("^DICIELA : monde créé, jour 1, "));
            yield return SceneManager.LoadSceneAsync(SceneNames.Game, LoadSceneMode.Additive);

            float start = Time.realtimeSinceStartup;
            for (int frame = 0; frame < MaxFrames && Time.realtimeSinceStartup - start < RunSeconds; frame++)
                yield return null;

            LogAssert.Expect(LogType.Log, new Regex(@"^DICIELA : monde arrêté après [1-9]\d* pas, jour 1, "));
            yield return SceneManager.UnloadSceneAsync(SceneNames.Game);
        }
    }
}
