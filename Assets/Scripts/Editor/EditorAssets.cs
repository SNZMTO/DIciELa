using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les petits gestes communs des monteurs : dossiers, existence d'un asset, champs remplis par nom, composant
    /// cherché dans une scène.
    /// </summary>
    internal static class EditorAssets
    {
        /// <summary>Crée le dossier et ses parents s'ils manquent (chemin commençant par « Assets »).</summary>
        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            int slash = folder.LastIndexOf('/');
            string parent = folder.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder.Substring(slash + 1));
        }

        public static bool Exists(string assetPath)
        {
            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath, AssetPathToGUIDOptions.OnlyExistingAssets));
        }

        /// <summary>Le champ sérialisé de ce nom, ou une erreur claire s'il n'existe pas.</summary>
        public static SerializedProperty Property(SerializedObject serialized, string name)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null)
                throw new InvalidOperationException(serialized.targetObject.GetType().Name + " n'a pas de champ " + name +
                                                    ".");
            return property;
        }

        /// <summary>Relie un champ à un asset s'il est vide ; ne remplace jamais un réglage fait à la main.</summary>
        public static void AssignIfMissing(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = Property(serialized, fieldName);
            if (property.objectReferenceValue != null)
                return;

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        /// <summary>Lit un asset qui doit exister ; sinon, une erreur claire.</summary>
        public static T Load<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
                throw new InvalidOperationException("DICIELA : " + assetPath + " est introuvable.");
            return asset;
        }

        /// <summary>Le premier composant de ce type dans la scène (objets racines et leurs enfants), ou null.</summary>
        public static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T found = root.GetComponentInChildren<T>(true);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
