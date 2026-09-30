using System.IO;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Écrit une image de l'interface générée (le cadre, son ombre, une vignette) et la règle pour UI Toolkit : sans
    /// compression ni redimensionnement, bornée à ses bords ; lissée entre ses tailles (les mipmaps) si elle est en
    /// triple résolution, lue telle quelle sinon.
    /// </summary>
    internal static class InterfaceImage
    {
        public static void Save(string path, int width, int height, Color[] pixels, bool mipmaps)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = mipmaps;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = mipmaps ? FilterMode.Trilinear : FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
}
