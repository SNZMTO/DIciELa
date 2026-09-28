using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée les matériaux qui manquent, sur le shader Lit d'URP. Un matériau qui existe n'est jamais touché : ses
    /// réglages se font dans l'Inspector.
    /// </summary>
    internal static class MaterialBuilder
    {
        private const string LitShader = "Universal Render Pipeline/Lit";

        /// <summary>Un matériau opaque de cette couleur.</summary>
        public static Material OpaqueIfMissing(string path, Color color)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            var material = new Material(Shader.Find(LitShader));
            material.SetColor("_BaseColor", color);
            return Save(material, path);
        }

        /// <summary>
        /// Un matériau translucide, blanc, sans ombre : la vue lui donne sa couleur et sa transparence. Les réglages
        /// sont ceux que l'Inspector d'URP pose pour « Transparent, Alpha » (BaseShaderGUI).
        /// </summary>
        public static Material TranslucentIfMissing(string path)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            var material = new Material(Shader.Find(LitShader));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_BlendModePreserveSpecular", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetShaderPassEnabled("DepthOnly", false);
            material.SetShaderPassEnabled("ShadowCaster", false);
            material.renderQueue = (int)RenderQueue.Transparent;
            return Save(material, path);
        }

        private static Material Save(Material material, string path)
        {
            EditorAssets.EnsureFolder(path.Substring(0, path.LastIndexOf('/')));
            AssetDatabase.CreateAsset(material, path);
            Debug.Log("DICIELA : matériau créé : " + path + ".");
            return material;
        }
    }
}
