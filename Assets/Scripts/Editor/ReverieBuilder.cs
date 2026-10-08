using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Le profil de filtres de la rêverie (décision 228 ; ForesightFilter), posé au-dessus de celui de la carte : des
    /// couleurs moins vives et moins contrastées, un peu plus claires, et le voile sombre des bords de l'écran devenu
    /// lavande. Il naît une fois ; ensuite, il se règle dans l'Inspector, et le monteur ne l'écrase pas.
    /// </summary>
    internal static class ReverieBuilder
    {
        internal const string ProfilePath = "Assets/Art/Reverie.asset";

        /// <summary>Le profil de la rêverie, créé s'il manque.</summary>
        public static VolumeProfile EnsureProfile()
        {
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (existing != null)
                return existing;

            EditorAssets.EnsureFolder("Assets/Art");
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);

            var colors = Add<ColorAdjustments>(profile);
            colors.postExposure.Override(0.15f);
            colors.contrast.Override(-5f);
            colors.saturation.Override(-18f);
            colors.colorFilter.Override(new Color(0.98f, 0.95f, 1f));

            var veil = Add<Vignette>(profile);
            veil.color.Override(new Color(0.86f, 0.76f, 0.94f));
            veil.intensity.Override(0.42f);
            veil.smoothness.Override(0.55f);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            Debug.Log("DICIELA : le profil de la rêverie est créé (" + ProfilePath + ").");
            return profile;
        }

        // Un filtre de plus, rangé dans le fichier du profil, ses réglages laissés à ceux du profil de la carte tant
        // qu'on ne les remplace pas.
        private static T Add<T>(VolumeProfile profile) where T : VolumeComponent
        {
            var component = profile.Add<T>();
            component.name = typeof(T).Name;
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }
    }
}
