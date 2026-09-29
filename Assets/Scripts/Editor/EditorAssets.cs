using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les petits gestes communs des monteurs : dossiers, existence d'un asset, fiche créée si elle manque, champs
    /// remplis par nom, composant cherché dans une scène.
    /// </summary>
    internal static class EditorAssets
    {
        /// <summary>
        /// La fiche à ce chemin ; si elle manque, elle est créée et remplie par <paramref name="fill"/>. Une fiche qui
        /// existe n'est jamais touchée, pour ne pas écraser un réglage fait à la main.
        /// </summary>
        public static T CreateIfMissing<T>(string path, Action<SerializedObject> fill) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
                return existing;

            EnsureFolder(path.Substring(0, path.LastIndexOf('/')));
            T fiche = ScriptableObject.CreateInstance<T>();
            fiche.name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (fill != null)
            {
                var serialized = new SerializedObject(fiche);
                fill(serialized);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.CreateAsset(fiche, path);
            Debug.Log("DICIELA : fiche créée : " + path + ".");
            return fiche;
        }

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
            Persist(target);
        }

        /// <summary>Remplit des champs d'une fiche qui existe, par SerializedObject, comme l'Inspector.</summary>
        public static void Set(UnityEngine.Object target, Action<SerializedObject> fill)
        {
            var serialized = new SerializedObject(target);
            fill(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Persist(target);
        }

        /// <summary>
        /// Écrit tout de suite sur le disque une fiche retouchée : un monteur qui enregistre ensuite un prefab peut faire
        /// relire à Unity les fiches du disque, et une retouche restée en mémoire serait perdue.
        /// </summary>
        private static void Persist(UnityEngine.Object target)
        {
            EditorUtility.SetDirty(target);
            if (AssetDatabase.Contains(target))
                AssetDatabase.SaveAssetIfDirty(target);
        }

        /// <summary>Remplit une liste sérialisée de ces références, dans cet ordre, à la place de ce qu'elle tenait.</summary>
        public static void SetList<T>(SerializedProperty list, IReadOnlyList<T> items) where T : UnityEngine.Object
        {
            list.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        /// <summary>Lit un asset qui doit exister ; sinon, une erreur claire.</summary>
        public static T Load<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
                throw new InvalidOperationException("DICIELA : " + assetPath + " est introuvable.");
            return asset;
        }

        /// <summary>Le composant de ce type sur cet objet, ajouté s'il manque.</summary>
        public static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        /// <summary>Le composant de ce type dans la scène ; s'il manque, un objet racine de ce nom qui le porte.</summary>
        public static T FindOrCreate<T>(Scene scene, string objectName) where T : Component
        {
            T existing = FindInScene<T>(scene);
            if (existing != null)
                return existing;

            var created = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(created, scene);
            return created.AddComponent<T>();
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
