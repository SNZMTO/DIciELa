using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Génère les images de l'écran de création (décision 162), en facettes à la façon du pack (FacetPainter) : le
    /// panneau de bois brun au cadre biseauté, ses coins garnis d'une bande et d'un rivet dorés ; la fente sombre où
    /// s'écrit un choix ; le bouton doré des flèches et celui qui valide ; le ruban de brique du titre. Le pack n'a pas
    /// d'images d'interface. Chaque image se découpe en neuf (bords fixes, milieu étiré : Creation.uss) et reste la même
    /// d'un passage à l'autre (graines fixes). Le menu des images de l'interface l'appelle ; tout est refait en place.
    /// </summary>
    internal static class MenuArtBuilder
    {
        public const string Folder = "Assets/UI/Images";

        private static readonly Color Wood = new Color(0.47f, 0.31f, 0.18f);
        private static readonly Color DarkWood = new Color(0.29f, 0.18f, 0.1f);
        private static readonly Color Slot = new Color(0.2f, 0.13f, 0.08f);
        private static readonly Color Gold = new Color(0.78f, 0.56f, 0.29f);
        private static readonly Color DarkGold = new Color(0.46f, 0.3f, 0.15f);
        private static readonly Color Brick = new Color(0.7f, 0.28f, 0.28f);
        private static readonly Color DarkBrick = new Color(0.45f, 0.17f, 0.17f);

        // Le panneau : 768 px de côté, des coins de 64 px que la découpe en neuf garde intacts.
        private const int PanelSize = 768;
        private const int PanelCorner = 64;

        public static void CreateAll()
        {
            Write("Panneau", PanelSize, PanelSize, 1, Panel);
            Write("Fente", 256, 48, 2, painter =>
                painter.Plate(new RectInt(0, 0, 256, 48), 10, 26, 0.3f, Slot, 0, Slot));
            Write("Bouton", 48, 48, 3, painter =>
                painter.Plate(new RectInt(0, 0, 48, 48), 8, 14, 0.7f, Gold, 5, DarkGold));
            Write("Valider", 512, 96, 4, painter =>
                painter.Plate(new RectInt(0, 0, 512, 96), 22, 34, 0.6f, Gold, 9, DarkGold));
            Write("Ruban", 512, 96, 5, Ribbon);
        }

        /// <summary>Le panneau de bois, puis dans chaque coin une bande dorée en travers du biseau et un rivet.</summary>
        private static void Panel(FacetPainter painter)
        {
            painter.Plate(new RectInt(0, 0, PanelSize, PanelSize), 46, 72, 0.45f, Wood, 28, DarkWood);
            for (int corner = 0; corner < 4; corner++)
            {
                bool right = (corner & 1) == 1, top = (corner & 2) == 2;
                // La bande longe le biseau du coin, à une douzaine de pixels de lui.
                painter.Polygon(Mirrored(right, top, new Vector2(8f, 50f), new Vector2(50f, 8f), new Vector2(60f, 18f),
                    new Vector2(18f, 60f)), Gold);
                painter.Polygon(Mirrored(right, top, new Vector2(16f, 52f), new Vector2(52f, 16f), new Vector2(56f, 20f),
                    new Vector2(20f, 56f)), DarkGold);
                Vector2 rivet = Mirrored(right, top, new Vector2(40f, 40f))[0];
                painter.Plate(new RectInt((int)rivet.x - 9, (int)rivet.y - 9, 18, 18), 5, 6, 0.9f, Gold, 3, DarkGold);
            }
        }

        /// <summary>Un ruban de brique : une bande à facettes, et à chaque bout une queue plus sombre, échancrée.</summary>
        private static void Ribbon(FacetPainter painter)
        {
            for (int side = 0; side < 2; side++)
            {
                bool right = side == 1;
                // L'échancrure rend la queue concave : deux moitiés convexes.
                painter.Polygon(Flipped(right, new Vector2(0f, 74f), new Vector2(18f, 46f), new Vector2(64f, 46f),
                    new Vector2(64f, 74f)), DarkBrick);
                painter.Polygon(Flipped(right, new Vector2(18f, 46f), new Vector2(0f, 18f), new Vector2(64f, 18f),
                    new Vector2(64f, 46f)), DarkBrick);
            }
            painter.Plate(new RectInt(40, 8, 432, 80), 8, 24, 0.4f, Brick, 4, DarkGold);
        }

        // Les points d'un coin bas gauche, portés dans le coin voulu du panneau.
        private static List<Vector2> Mirrored(bool right, bool top, params Vector2[] points)
        {
            var mirrored = new List<Vector2>();
            foreach (Vector2 point in points)
                mirrored.Add(new Vector2(right ? PanelSize - point.x : point.x, top ? PanelSize - point.y : point.y));
            return mirrored;
        }

        // Les points d'une queue gauche du ruban, portés à droite s'il le faut.
        private static List<Vector2> Flipped(bool right, params Vector2[] points)
        {
            var flipped = new List<Vector2>();
            foreach (Vector2 point in points)
                flipped.Add(new Vector2(right ? 512f - point.x : point.x, point.y));
            return flipped;
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
