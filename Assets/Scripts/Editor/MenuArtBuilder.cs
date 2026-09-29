using System.IO;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Génère les images de l'écran de création, en facettes à la façon du pack (FacetPainter) : le panneau de bois brun
    /// au cadre biseauté, la fente sombre où s'écrit un choix, le bouton des flèches et celui qui valide, dorés. Le pack
    /// n'a pas d'images d'interface. Chaque image se découpe en neuf (bords fixes, milieu étiré : Creation.uss) et reste la
    /// même d'un passage à l'autre (graines fixes). Réexécutable : elles sont refaites en place.
    /// </summary>
    internal static class MenuArtBuilder
    {
        public const string Folder = "Assets/UI/Images";

        private static readonly Color Wood = new Color(0.47f, 0.31f, 0.18f);
        private static readonly Color DarkWood = new Color(0.29f, 0.18f, 0.1f);
        private static readonly Color Slot = new Color(0.2f, 0.13f, 0.08f);
        private static readonly Color Gold = new Color(0.78f, 0.56f, 0.29f);
        private static readonly Color DarkGold = new Color(0.46f, 0.3f, 0.15f);

        [MenuItem("DICIELA/Interface/Générer les images du menu")]
        public static void CreateAll()
        {
            Write("Panneau", 768, 768, 1, painter =>
                painter.Plate(new RectInt(0, 0, 768, 768), 46, 72, 0.45f, Wood, 28, DarkWood));
            Write("Fente", 256, 48, 2, painter =>
                painter.Plate(new RectInt(0, 0, 256, 48), 10, 26, 0.3f, Slot, 0, Slot));
            Write("Bouton", 48, 48, 3, painter =>
                painter.Plate(new RectInt(0, 0, 48, 48), 8, 14, 0.7f, Gold, 5, DarkGold));
            Write("Valider", 512, 96, 4, painter =>
                painter.Plate(new RectInt(0, 0, 512, 96), 22, 34, 0.6f, Gold, 9, DarkGold));
            AssetDatabase.Refresh();
            Debug.Log("DICIELA : images du menu générées dans " + Folder + ".");
        }

        private static void Write(string name, int width, int height, int seed, System.Action<FacetPainter> paint)
        {
            var painter = new FacetPainter(width, height, seed);
            paint(painter);
            Texture2D texture = painter.ToTexture();
            EditorAssets.EnsureFolder(Folder);
            string path = Folder + "/" + name + ".png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
}
