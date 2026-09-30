using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Enregistre les icônes de l'interface (InterfaceIcons) en images vectorielles d'UI Toolkit, nettes à toute taille,
    /// sans le module SVG (décision 145) : un tracé par face, sauvé par Painter2D. Une icône refaite garde son asset.
    /// </summary>
    internal static class InterfaceIconBuilder
    {
        public const string Folder = "Assets/UI/Icones";

        public static void CreateAll(UssPalette palette)
        {
            EditorAssets.EnsureFolder(Folder);
            foreach (InterfaceIcons.Icon icon in InterfaceIcons.All())
                Save(Draw(icon, palette), Folder + "/" + icon.Name + ".asset");
            AssetDatabase.SaveAssets();
        }

        private static VectorImage Draw(InterfaceIcons.Icon icon, UssPalette palette)
        {
            var painter = new Painter2D();
            painter.lineJoin = LineJoin.Round;
            painter.lineWidth = icon.OutlineWidth;
            foreach (InterfaceIcons.Face face in icon.Outlined)
            {
                Color outline = Colour(icon.OutlineColor, 1f, palette);
                painter.fillColor = outline;
                painter.strokeColor = outline;
                Trace(painter, face.Points);
                painter.Fill();
                painter.Stroke();
            }
            foreach (InterfaceIcons.Face face in icon.Faces)
            {
                painter.fillColor = Colour(face.Color, face.Opacity, palette);
                Trace(painter, face.Points);
                painter.Fill();
            }
            var image = ScriptableObject.CreateInstance<VectorImage>();
            image.name = icon.Name;
            if (!painter.SaveToVectorImage(image))
                throw new System.InvalidOperationException("L'icône « " + icon.Name + " » ne s'enregistre pas.");
            painter.Dispose();
            Box(image, icon);
            return image;
        }

        // Unity range l'image au ras de son dessin : on lui rend la boîte de la maquette (36 × 36 pour la plupart), pour
        // que chaque icône garde sa place et sa taille dans son carré.
        private static void Box(VectorImage image, InterfaceIcons.Icon icon)
        {
            Vector2 drawn = DrawnMinimum(icon);
            var serialized = new SerializedObject(image);
            SerializedProperty vertices = serialized.FindProperty("vertices");
            SerializedProperty size = serialized.FindProperty("size");
            if (vertices == null || size == null)
                throw new System.InvalidOperationException("Une image vectorielle d'Unity n'a plus ses champs vertices et " +
                                                           "size : revoir InterfaceIconBuilder.Box.");
            for (int i = 0; i < vertices.arraySize; i++)
            {
                SerializedProperty position = vertices.GetArrayElementAtIndex(i).FindPropertyRelative("position");
                position.vector3Value += (Vector3)drawn;
            }
            size.vector2Value = new Vector2(icon.Size, icon.Size);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // Le coin haut-gauche du dessin, là où Unity met l'origine : le plus petit point, moins la moitié d'un contour.
        private static Vector2 DrawnMinimum(InterfaceIcons.Icon icon)
        {
            var minimum = new Vector2(float.MaxValue, float.MaxValue);
            foreach (InterfaceIcons.Face face in icon.Faces)
                minimum = Vector2.Min(minimum, Smallest(face.Points, 0f));
            foreach (InterfaceIcons.Face face in icon.Outlined)
                minimum = Vector2.Min(minimum, Smallest(face.Points, icon.OutlineWidth * 0.5f));
            return minimum;
        }

        private static Vector2 Smallest(float[] points, float margin)
        {
            var smallest = new Vector2(float.MaxValue, float.MaxValue);
            for (int i = 0; i < points.Length; i += 2)
                smallest = Vector2.Min(smallest, new Vector2(points[i] - margin, points[i + 1] - margin));
            return smallest;
        }

        private static void Trace(Painter2D painter, float[] points)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(points[0], points[1]));
            for (int i = 2; i < points.Length; i += 2)
                painter.LineTo(new Vector2(points[i], points[i + 1]));
            painter.ClosePath();
        }

        private static Color Colour(string code, float opacity, UssPalette palette)
        {
            Color color;
            if (code.StartsWith("#"))
                ColorUtility.TryParseHtmlString(code, out color);
            else
                color = palette.Get(code);
            color.a *= opacity;
            return color;
        }

        private static void Save(VectorImage image, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<VectorImage>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(image, path);
                return;
            }
            EditorUtility.CopySerialized(image, existing);
            existing.name = image.name;
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(image);
        }
    }
}
