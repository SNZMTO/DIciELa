using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Génère les images de l'interface « fusion » (décision 145) : le cadre de bois, tiré de l'atlas du pack, en
    /// triple résolution ; son ombre portée ; la feuille qui les découpe en neuf (Cadre.uss), dont les mesures se
    /// déduisent des peintres ; le voile de rêverie de la prévision (ReveriePainter) ; les icônes ; les images du
    /// panneau de bois brun de l'écran de création (MenuArtBuilder, décision 162). Réexécutable : tout est refait en
    /// place.
    /// </summary>
    internal static class InterfaceArtBuilder
    {
        private const string ImageFolder = "Assets/UI/Images";
        private const string FramePath = ImageFolder + "/Cadre.png";
        private const string ShadowPath = ImageFolder + "/CadreOmbre.png";
        private const string ReveriePath = ImageFolder + "/Reverie.png";
        private const string FrameStylePath = "Assets/UI/Cadre.uss";

        [MenuItem("DICIELA/Interface/Générer les images de l'interface")]
        public static void CreateAll()
        {
            UssPalette palette = UssPalette.Read();
            EditorAssets.EnsureFolder(ImageFolder);
            int pixels = FramePainter.Size * FramePainter.Resolution;
            var frame = new FramePainter(palette, PackWood.Load(FramePainter.Tile));
            // Le cadre se lit en triple résolution, lissé entre ses tailles ; l'ombre, floue, se lit telle quelle.
            InterfaceImage.Save(FramePath, pixels, pixels, frame.Paint(), true);
            InterfaceImage.Save(ShadowPath, ShadowPainter.Size, ShadowPainter.Size, new ShadowPainter(palette).Paint(), false);
            var reverie = new ReveriePainter(palette);
            InterfaceImage.Save(ReveriePath, ReveriePainter.Size, ReveriePainter.Size, reverie.Paint(), false);
            File.WriteAllText(FrameStylePath, FrameStyle());
            AssetDatabase.ImportAsset(FrameStylePath);
            InterfaceIconBuilder.CreateAll(palette);
            MenuArtBuilder.CreateAll();
            AssetDatabase.Refresh();
            Debug.Log("DICIELA : images de l'interface générées : " + FramePath + ", " + ShadowPath + ", " +
                      ReveriePath + ", " + FrameStylePath + ", " + InterfaceIconBuilder.Folder + ", " +
                      MenuArtBuilder.Folder + ".");
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
