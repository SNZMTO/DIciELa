using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Génère les images de l'interface « fusion » (décision 145) : le cadre de bois, tiré de l'atlas du pack, en triple
    /// résolution ; son ombre portée ; la feuille qui les découpe en neuf (Cadre.uss), dont les mesures se déduisent des
    /// peintres ; les icônes. Réexécutable : tout est refait en place.
    /// </summary>
    internal static class InterfaceArtBuilder
    {
        private const string ImageFolder = "Assets/UI/Images";
        private const string FramePath = ImageFolder + "/Cadre.png";
        private const string ShadowPath = ImageFolder + "/CadreOmbre.png";
        private const string FrameStylePath = "Assets/UI/Cadre.uss";

        [MenuItem("DICIELA/Interface/Générer les images de l'interface")]
        public static void CreateAll()
        {
            UssPalette palette = UssPalette.Read();
            EditorAssets.EnsureFolder(ImageFolder);
            int pixels = FramePainter.Size * FramePainter.Resolution;
            var frame = new FramePainter(palette, PackWood.Load(FramePainter.Tile));
            Write(FramePath, pixels, frame.Paint(), true);
            Write(ShadowPath, ShadowPainter.Size, new ShadowPainter(palette).Paint(), false);
            File.WriteAllText(FrameStylePath, FrameStyle());
            AssetDatabase.ImportAsset(FrameStylePath);
            InterfaceIconBuilder.CreateAll(palette);
            Debug.Log("DICIELA : images de l'interface générées : " + FramePath + ", " + ShadowPath + ", " +
                      FrameStylePath + ", " + InterfaceIconBuilder.Folder + ".");
        }

        // Le cadre se lit en triple résolution, lissé entre ses tailles (les mipmaps) quand l'interface est petite ;
        // l'ombre, floue, se lit telle quelle.
        private static void Write(string path, int size, Color[] pixels, bool mipmaps)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
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

        private static string FrameStyle()
        {
            int slice = FramePainter.Corner * FramePainter.Resolution;
            string scale = (1f / FramePainter.Resolution).ToString("0.######", CultureInfo.InvariantCulture);
            int margin = ShadowPainter.Margin;
            return "/* Généré par DICIELA/Interface/Générer les images de l'interface : ne pas retoucher. Le cadre de\n" +
                   "   bois (WoodFrame, décision 145) : son ombre, puis sa face en triple résolution, coins fixes, bords\n" +
                   "   et milieu répétés ; son contenu à l'intérieur de la bordure. */\n\n" +
                   ".cadre {\n" +
                   $"    padding: {FramePainter.Border}px;\n" +
                   "}\n\n" +
                   ".cadre-ombre {\n" +
                   "    position: absolute;\n" +
                   $"    left: -{margin}px;\n" +
                   $"    top: -{margin}px;\n" +
                   $"    right: -{margin}px;\n" +
                   $"    bottom: -{margin}px;\n" +
                   "    background-image: url(\"Images/CadreOmbre.png\");\n" +
                   Slices(ShadowPainter.Corner) +
                   "}\n\n" +
                   ".cadre-face {\n" +
                   "    position: absolute;\n" +
                   "    left: 0;\n" +
                   "    top: 0;\n" +
                   "    right: 0;\n" +
                   "    bottom: 0;\n" +
                   "    background-image: url(\"Images/Cadre.png\");\n" +
                   Slices(slice) +
                   $"    -unity-slice-scale: {scale}px;\n" +
                   "    -unity-slice-type: tiled;\n" +
                   "}\n";
        }

        private static string Slices(int slice)
        {
            return $"    -unity-slice-left: {slice};\n    -unity-slice-top: {slice};\n" +
                   $"    -unity-slice-right: {slice};\n    -unity-slice-bottom: {slice};\n";
        }
    }
}
