using System.Collections;
using System.Text.RegularExpressions;
using Diciela.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La scène de test, jouée comme par le joueur : on la charge, elle charge sa carte de test et ouvre l'écran de
    /// création, on valide, on la laisse tourner, on la quitte. Le journal du jeu dit si le monde a reçu sa carte, si la
    /// Chimère y est née et s'il a avancé ; toute erreur dans la console fait échouer le test. La vraie carte n'est
    /// jamais chargée. Une ville qui ne peut pas s'ouvrir (un HUD qui a perdu un élément) arrête la partie, l'erreur dite,
    /// au lieu de la laisser figée.
    /// </summary>
    public sealed class GameSceneTests
    {
        private const int MaxFrames = 1200;

        [UnityTearDown]
        public IEnumerator LeaveTheTestScene()
        {
            yield return TestOpening.Leave(MaxFrames);
        }

        [UnityTest]
        public IEnumerator TheWorldReceivesTheTestMapAndSteps()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"^DICIELA : monde créé, carte de \d+ × \d+ m \(relief de \d+ × \d+ points\)"));
            LogAssert.Expect(LogType.Log, new Regex(@"^DICIELA : l'habitant n° 1 \(chimere\) arrive en "));
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePaths.GameTest,
                new LoadSceneParameters(LoadSceneMode.Additive));
#endif
            yield return TestOpening.Confirm(MaxFrames);
            // Elle tourne jusqu'à ce que sa carte soit chargée et que le monde ait joué un pas : celui où la Chimère naît.
            // La borne compte les images, jamais les secondes : une machine lente attend plus longtemps.
            GameRunner runner = null;
            for (int frame = 0; frame < MaxFrames; frame++)
            {
                if (runner == null)
                    runner = Object.FindAnyObjectByType<GameRunner>();
                if (SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded && runner != null && runner.World != null &&
                    runner.World.Villagers.Chimera != null)
                    break;
                yield return null;
            }

            Assert.IsTrue(SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded, "La carte de test doit être chargée.");
            Assert.IsFalse(SceneManager.GetSceneByPath(ScenePaths.Map).isLoaded, "La vraie carte ne doit jamais l'être.");

            LogAssert.Expect(LogType.Log, new Regex(@"^DICIELA : monde arrêté après [1-9]\d* pas, jour 1, "));
            yield return TestOpening.Leave(MaxFrames);

            Assert.IsFalse(SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded,
                "La carte de test doit partir avec sa scène de jeu.");
        }

        [UnityTest]
        public IEnumerator ATownThatCannotOpenStopsTheGameInsteadOfFreezingIt()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePaths.GameTest,
                new LoadSceneParameters(LoadSceneMode.Additive));
#endif
            // Le HUD perd la fiche d'un lieu, comme après une retouche ratée de son UXML : la ville ne peut pas s'ouvrir.
            GameRunner runner = null;
            for (int frame = 0; frame < MaxFrames && (runner == null || runner.World == null); frame++)
            {
                runner = Object.FindAnyObjectByType<GameRunner>();
                yield return null;
            }
            Assert.IsNotNull(runner?.World, "Le monde de la scène de test n'est pas né.");
            Object.FindAnyObjectByType<Diciela.UI.PlaceSheet>().GetComponent<UIDocument>()
                .rootVisualElement.Q("fiche").RemoveFromHierarchy();
            LogAssert.Expect(LogType.Exception, new Regex("La fiche d'un lieu ne trouve pas ses éléments"));

            yield return TestOpening.Confirm(MaxFrames);
            yield return null;

            Assert.IsFalse(runner.enabled, "Le démarrage s'arrête au lieu de rester figé, ville à moitié ouverte.");
            Assert.IsNull(runner.World, "Le monde est libéré.");
            yield return TestOpening.Leave(MaxFrames);
        }
    }
}
