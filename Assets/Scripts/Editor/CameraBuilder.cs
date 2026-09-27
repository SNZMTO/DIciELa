using Diciela.Controls;
using Diciela.Data;
using Diciela.View;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Installe la caméra de gestion sur la caméra d'une scène racine : la caméra (vue), ses gestes (commandes), leurs
    /// fiches et l'asset d'actions, et les filtres du rendu (le post-traitement d'URP), sans lesquels le rendu du pack
    /// ne s'affiche pas. Réexécutable : ce qui est déjà réglé n'est pas touché.
    /// </summary>
    internal static class CameraBuilder
    {
        /// <summary>La caméra de gestion de la scène, installée si besoin ; null (avec une erreur) sans caméra.</summary>
        public static CameraRig EnsureIn(Scene scene)
        {
            Camera camera = FindCamera(scene);
            if (camera == null)
            {
                Debug.LogError("DICIELA : la scène « " + scene.name + " » n'a pas de caméra.");
                return null;
            }

            var settings = EditorAssets.Load<CameraSettings>(DataAssetBuilder.CameraSettingsPath);
            var actions = EditorAssets.Load<InputActionAsset>(InputActionsBuilder.ActionsPath);

            CameraRig rig = GetOrAdd<CameraRig>(camera.gameObject);
            EditorAssets.AssignIfMissing(rig, "_settings", settings);

            CameraControls controls = GetOrAdd<CameraControls>(camera.gameObject);
            EditorAssets.AssignIfMissing(controls, "_actions", actions);
            EditorAssets.AssignIfMissing(controls, "_rig", rig);
            EditorAssets.AssignIfMissing(controls, "_settings", settings);

            UniversalAdditionalCameraData renderData = camera.GetUniversalAdditionalCameraData();
            if (!renderData.renderPostProcessing)
            {
                renderData.renderPostProcessing = true;
                EditorUtility.SetDirty(renderData);
            }
            return rig;
        }

        private static Camera FindCamera(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Camera camera = root.GetComponentInChildren<Camera>(true);
                if (camera != null)
                    return camera;
            }
            return null;
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
