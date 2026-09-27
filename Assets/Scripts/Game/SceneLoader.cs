using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.Game
{
    /// <summary>
    /// Charge une scène par-dessus les autres, par son chemin. Dans l'éditeur, n'importe quelle scène du projet se
    /// charge (la carte de test n'est pas dans la liste de build) ; dans le jeu compilé, seules celles de la liste.
    /// </summary>
    internal static class SceneLoader
    {
        public static AsyncOperation LoadAdditive(string scenePath)
        {
#if UNITY_EDITOR
            return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
#else
            return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
#endif
        }
    }
}
