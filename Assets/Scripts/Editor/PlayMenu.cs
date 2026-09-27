using Diciela.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Jouer depuis n'importe où dans l'éditeur : ouvre la scène Jeu, puis lance le Play. On ne fixe jamais la scène
    /// de départ du Play en permanence (SAVOIR, section 3).
    /// </summary>
    public static class PlayMenu
    {
        [MenuItem("DICIELA/Jouer")]
        public static void Play()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (SceneManager.GetActiveScene().path != ScenePaths.Game)
            {
                if (!EditorAssets.Exists(ScenePaths.Game))
                {
                    Debug.LogError("DICIELA : la scène Jeu n'existe pas encore : lancer « DICIELA/Scènes/Monter les " +
                                   "scènes ».");
                    return;
                }
                if (!GameSceneBuilder.CanChangeScenes())
                    return;
                EditorSceneManager.OpenScene(ScenePaths.Game, OpenSceneMode.Single);
            }

            EditorApplication.EnterPlaymode();
        }
    }
}
