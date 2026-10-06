using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Le banc photo des planches : photographie des personnages hors de toute scène (PreviewRenderUtility, qui rend avec
    /// l'URP et les matériaux du pack), de près, sous une lumière fixe, puis assemble les photos en une planche, une case
    /// par photo, en JPG. Rien ne touche la scène ouverte. Les planches servent à juger une pose à l'œil, de près.
    /// </summary>
    internal sealed class Studio : IDisposable
    {
        private const float FieldOfView = 24f;
        private const int JpegQuality = 88;
        private static readonly Color Background = new Color(0.62f, 0.66f, 0.7f);

        private readonly PreviewRenderUtility _preview = new PreviewRenderUtility();
        private readonly int _cell;

        /// <summary>Un banc dont chaque photo fait ce nombre de pixels de côté, sur le fond gris des planches.</summary>
        public Studio(int cell) : this(cell, Background)
        {
        }

        /// <summary>Un banc dont chaque photo fait ce nombre de pixels de côté, sur ce fond.</summary>
        public Studio(int cell, Color background)
        {
            _cell = cell;
            Camera camera = _preview.camera;
            camera.fieldOfView = FieldOfView;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 60f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            _preview.lights[0].intensity = 1.3f;
            _preview.lights[0].transform.rotation = Quaternion.Euler(35f, -25f, 0f);
            _preview.lights[1].intensity = 0.7f;
            _preview.lights[1].transform.rotation = Quaternion.Euler(20f, 160f, 0f);
            _preview.ambientColor = new Color(0.5f, 0.5f, 0.55f);
        }

        /// <summary>Met ce personnage devant l'objectif ; le banc le détruira avec lui.</summary>
        public void Add(GameObject subject)
        {
            _preview.AddSingleGO(subject);
        }

        /// <summary>Une photo de ce point, prise de là (en mètres, dans le repère du personnage), assez large pour ce rayon.</summary>
        public Texture2D Shoot(Vector3 target, Vector3 direction, float radius)
        {
            float distance = radius / Mathf.Tan(FieldOfView * 0.5f * Mathf.Deg2Rad);
            Transform camera = _preview.camera.transform;
            camera.position = target + direction.normalized * distance;
            camera.LookAt(target);
            _preview.BeginStaticPreview(new Rect(0, 0, _cell, _cell));
            _preview.Render(true);
            return _preview.EndStaticPreview();
        }

        /// <summary>Assemble ces photos en une planche de tant de colonnes, écrite en JPG à ce chemin ; les détruit.</summary>
        public void Save(List<Texture2D> photos, int columns, string path)
        {
            int rows = (photos.Count + columns - 1) / columns;
            var sheet = new Texture2D(columns * _cell, rows * _cell, TextureFormat.RGB24, false);
            var fill = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++)
                fill[i] = Color.white;
            sheet.SetPixels32(fill);
            for (int i = 0; i < photos.Count; i++)
            {
                int x = i % columns * _cell;
                int y = (rows - 1 - i / columns) * _cell;
                // Un liseré blanc d'un pixel sépare les cases.
                Color[] pixels = photos[i].GetPixels(1, 1, _cell - 2, _cell - 2);
                sheet.SetPixels(x + 1, y + 1, _cell - 2, _cell - 2, pixels);
                UnityEngine.Object.DestroyImmediate(photos[i]);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            File.WriteAllBytes(path, sheet.EncodeToJPG(JpegQuality));
            UnityEngine.Object.DestroyImmediate(sheet);
        }

        public void Dispose()
        {
            _preview.Cleanup();
        }
    }
}
