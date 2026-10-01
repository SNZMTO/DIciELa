using System.Collections;
using Diciela.Game;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'ouverture d'une partie dans la scène de test : elle s'ouvre sur l'écran de création, et le temps ne part qu'à la
    /// validation (GDD, section 6). Le test valide par le même appel que le bouton et qu'Entrée. Et sa fermeture : la
    /// scène quittée, sa carte part avec elle.
    /// </summary>
    internal static class TestOpening
    {
        /// <summary>
        /// Quitte la scène de test si elle est chargée, puis attend que sa carte parte avec elle (son démarrage la
        /// décharge). Les tests l'appellent à leur fin, et à leur démontage : un test qui échoue en chemin ne laisse pas
        /// sa scène aux suivants, qui y trouveraient sa carte déjà ouverte.
        /// </summary>
        public static IEnumerator Leave(int maxFrames)
        {
            if (SceneManager.GetSceneByPath(ScenePaths.GameTest).isLoaded)
                yield return SceneManager.UnloadSceneAsync(ScenePaths.GameTest);
            // La borne compte les images, jamais les secondes : une machine lente attend plus longtemps.
            for (int frame = 0; frame < maxFrames && SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded; frame++)
                yield return null;
        }

        /// <summary>Attend que le monde soit né et l'écran ouvert, puis valide la Chimère de départ.</summary>
        public static IEnumerator Confirm(int maxFrames)
        {
            // La borne compte les images, jamais les secondes : une machine lente attend plus longtemps.
            for (int frame = 0; frame < maxFrames; frame++)
            {
                var runner = Object.FindAnyObjectByType<GameRunner>();
                if (runner != null && runner.World != null)
                {
                    Object.FindAnyObjectByType<CreationScreen>().Confirm();
                    yield break;
                }
                yield return null;
            }
            Assert.Fail("L'écran de création de la scène de test ne s'est pas ouvert.");
        }
    }
}
