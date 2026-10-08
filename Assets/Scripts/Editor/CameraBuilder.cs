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
    /// Installe la caméra de gestion sur la caméra d'une scène racine : la caméra (vue), ses gestes (commandes), ses
    /// ombres et son brouillard qui suivent le zoom, le filtre de rêverie de la prévision et son profil
    /// (ReverieBuilder), leurs fiches et l'asset d'actions, et les filtres du rendu (le post-traitement d'URP), sans
    /// lesquels le rendu du pack ne s'affiche pas. Réexécutable : ce qui est déjà réglé n'est pas touché.
    /// </summary>
    internal static class CameraBuilder
    {
        /// <summary>La caméra de gestion de la scène, installée si besoin ; null (avec une erreur) sans caméra.</summary>
        public static CameraRig EnsureIn(Scene scene)
        {
            // La caméra qui porte déjà la caméra de gestion ; sinon, la caméra de la scène (l'atelier a la sienne, posée après).
            CameraRig installed = EditorAssets.FindInScene<CameraRig>(scene);
            Camera camera = installed != null ? installed.GetComponent<Camera>() : EditorAssets.FindInScene<Camera>(scene);
            if (camera == null)
            {
                Debug.LogError("DICIELA : la scène « " + scene.name + " » n'a pas de caméra.");
                return null;
            }

            var settings = EditorAssets.Load<CameraSettings>(DataAssetBuilder.CameraSettingsPath);
            var gestures = EditorAssets.Load<GestureSettings>(DataAssetBuilder.GestureSettingsPath);
            var actions = EditorAssets.Load<InputActionAsset>(InputActionsBuilder.ActionsPath);

            CameraRig rig = EditorAssets.GetOrAdd<CameraRig>(camera.gameObject);
            EditorAssets.AssignIfMissing(rig, "_settings", settings);

            CameraControls controls = EditorAssets.GetOrAdd<CameraControls>(camera.gameObject);
            EditorAssets.AssignIfMissing(controls, "_actions", actions);
            EditorAssets.AssignIfMissing(controls, "_rig", rig);
            EditorAssets.AssignIfMissing(controls, "_settings", settings);
            EditorAssets.AssignIfMissing(controls, "_gestures", gestures);

            CameraAtmosphere atmosphere = EditorAssets.GetOrAdd<CameraAtmosphere>(camera.gameObject);
            EditorAssets.AssignIfMissing(atmosphere, "_settings", settings);

            ForesightFilter reverie = EditorAssets.GetOrAdd<ForesightFilter>(camera.gameObject);
            EditorAssets.AssignIfMissing(reverie, "_settings", settings);
            EditorAssets.AssignIfMissing(reverie, "_profile", ReverieBuilder.EnsureProfile());

            UniversalAdditionalCameraData renderData = camera.GetUniversalAdditionalCameraData();
            if (!renderData.renderPostProcessing)
            {
                renderData.renderPostProcessing = true;
                EditorUtility.SetDirty(renderData);
            }
            return rig;
        }
    }
}
