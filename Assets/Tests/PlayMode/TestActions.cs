using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les vraies actions du jeu pour les tests des gestes : une copie de l'asset monté par le menu (le chemin est celui
    /// d'InputActionsBuilder), pour que les tests lisent les mêmes touches que le joueur sans toucher à l'asset.
    /// </summary>
    internal static class TestActions
    {
        private const string ActionsPath = "Assets/Input/Diciela.inputactions";

        /// <summary>Une copie de l'asset d'actions du jeu, que le test détruit.</summary>
        public static InputActionAsset CopyOfTheGame()
        {
#if UNITY_EDITOR
            var actions = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
            Assert.IsNotNull(actions, "L'asset d'actions manque : lancer « DICIELA/Scènes/Monter les scènes ».");
            return Object.Instantiate(actions);
#else
            return null;
#endif
        }
    }
}
