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

            if (SceneManager.GetActiveScene().path != GameSceneBuilder.GameScenePath)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameSceneBuilder.GameScenePath) == null)
                {
                    Debug.LogError("DICIELA : la scène Jeu n'existe pas encore : lancer « DICIELA/Scènes/Monter la " +
                                   "scène Jeu ».");
                    return;
                }
                if (!GameSceneBuilder.CanChangeScenes())
                    return;
                EditorSceneManager.OpenScene(GameSceneBuilder.GameScenePath, OpenSceneMode.Single);
            }

            EditorApplication.EnterPlaymode();
        }
    }
}
