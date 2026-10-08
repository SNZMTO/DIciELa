using NUnit.Framework;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les fiches du projet que les tests lisent telles quelles : celles qui n'existent qu'en un exemplaire.
    /// </summary>
    internal static class TestAssets
    {
#if UNITY_EDITOR
        /// <summary>
        /// La seule fiche de ce type dans le projet ; le test échoue s'il n'y en a pas une, et une seule.
        /// </summary>
        internal static T LoadSingle<T>() where T : Object
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name);
            Assert.AreEqual(1, guids.Length, "Il faut une seule fiche " + typeof(T).Name + " dans le projet.");
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
        }
#endif
    }
}
