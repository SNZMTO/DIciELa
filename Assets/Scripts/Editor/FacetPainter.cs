using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Peint une image en facettes, à la façon du low poly du pack : une forme aux coins coupés, pavée de triangles plats,
    /// chacun d'une seule teinte que donne son inclinaison sous une lumière venue d'en haut à gauche. Un cadre biseauté
    /// l'entoure : son bord haut et gauche prend la lumière, le bas et la droite l'ombre. Peinte deux fois plus grande
    /// puis réduite, pour des bords nets sans marches. Tirée d'une graine : la même image à chaque passage.
    /// </summary>
    internal sealed class FacetPainter
    {
        private const int Supersample = 2;
        private const int MinFrameCell = 8;
        private const int Groove = 2;
        // La pente que donne un bord de cadre à ses facettes, vers l'extérieur.
        private const float BevelSlope = 1.4f;
        private static readonly Vector3 Light = new Vector3(-0.45f, 0.6f, 0.65f).normalized;

        private readonly int _width;
        private readonly int _height;
        private readonly Color[] _pixels;
        private readonly System.Random _random;

        public FacetPainter(int width, int height, int seed)
        {
            _width = width * Supersample;
            _height = height * Supersample;
            _pixels = new Color[_width * _height];
            _random = new System.Random(seed);
        }

        /// <summary>
        /// Une plaque : ce rectangle (en pixels de l'image finale, origine en bas à gauche) aux coins coupés de chamfer,
        /// pavé de facettes d'environ cell pixels de cette couleur, penchées au plus de tilt ; frame > 0 lui donne un cadre
        /// biseauté de cette largeur, de la couleur frameColor.
        /// </summary>
        public void Plate(RectInt rect, int chamfer, int cell, float tilt, Color color, int frame, Color frameColor)
        {
            var outer = Octagon(rect, chamfer);
            if (frame > 0)
            {
                Facets(outer, rect, Mathf.Max(MinFrameCell, frame), tilt * 0.5f, frameColor, true);
                rect = new RectInt(rect.x + frame, rect.y + frame, rect.width - 2 * frame, rect.height - 2 * frame);
                chamfer = Mathf.Max(0, chamfer - frame / 2);
                // Une rainure sombre sépare le cadre de la plaque.
                var groove = new RectInt(rect.x - Groove, rect.y - Groove, rect.width + 2 * Groove, rect.height + 2 * Groove);
                var dark = new Color(frameColor.r * 0.5f, frameColor.g * 0.5f, frameColor.b * 0.5f, 1f);
                Facets(Octagon(groove, chamfer + Groove / 2), groove, cell, 0f, dark, false);
            }
            Facets(Octagon(rect, chamfer), rect, cell, tilt, color, false);
        }

        /// <summary>Un polygone plein d'une seule couleur (en pixels de l'image finale).</summary>
        public void Polygon(IReadOnlyList<Vector2> points, Color color)
        {
            var scaled = new List<Vector2>();
            foreach (Vector2 point in points)
                scaled.Add(point * Supersample);
            for (int i = 1; i + 1 < scaled.Count; i++)
                Triangle(scaled[0], scaled[i], scaled[i + 1], color, null);
        }

        /// <summary>L'image finale, réduite, prête à écrire.</summary>
        public Texture2D ToTexture()
        {
            int width = _width / Supersample, height = _height / Supersample;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var result = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color sum = Color.clear;
                    for (int dy = 0; dy < Supersample; dy++)
                    {
                        for (int dx = 0; dx < Supersample; dx++)
                            sum += _pixels[(y * Supersample + dy) * _width + x * Supersample + dx];
                    }
                    Color mean = sum / (Supersample * Supersample);
                    // Couleur non prémultipliée : un bord à moitié couvert garde sa teinte.
                    if (mean.a > 0f)
                        mean = new Color(mean.r / mean.a, mean.g / mean.a, mean.b / mean.a, mean.a);
                    result[y * width + x] = mean;
                }
            }
            texture.SetPixels(result);
            texture.Apply();
            return texture;
        }

        private List<Vector2> Octagon(RectInt rect, int chamfer)
        {
            float x0 = rect.xMin * Supersample, y0 = rect.yMin * Supersample;
            float x1 = rect.xMax * Supersample, y1 = rect.yMax * Supersample, c = chamfer * Supersample;
            return new List<Vector2>
            {
                new Vector2(x0 + c, y0), new Vector2(x1 - c, y0), new Vector2(x1, y0 + c), new Vector2(x1, y1 - c),
                new Vector2(x1 - c, y1), new Vector2(x0 + c, y1), new Vector2(x0, y1 - c), new Vector2(x0, y0 + c)
            };
        }

        // Un pavage de la boîte du rectangle en triangles, sommets secoués sauf au bord ; seuls les pixels dans le
        // polygone sont peints. Un cadre penche ses facettes vers l'extérieur, du côté du bord le plus proche.
        private void Facets(List<Vector2> shape, RectInt rect, int cell, float tilt, Color color, bool bevel)
        {
            float x0 = rect.xMin * Supersample, y0 = rect.yMin * Supersample;
            float w = rect.width * Supersample, h = rect.height * Supersample;
            int columns = Mathf.Max(1, Mathf.RoundToInt(w / (cell * Supersample)));
            int rows = Mathf.Max(1, Mathf.RoundToInt(h / (cell * Supersample)));
            var grid = new Vector2[columns + 1, rows + 1];
            for (int i = 0; i <= columns; i++)
            {
                for (int j = 0; j <= rows; j++)
                {
                    bool edge = i == 0 || j == 0 || i == columns || j == rows;
                    float jx = edge ? 0f : (float)(_random.NextDouble() - 0.5) * 0.7f;
                    float jy = edge ? 0f : (float)(_random.NextDouble() - 0.5) * 0.7f;
                    grid[i, j] = new Vector2(x0 + (i + jx) * w / columns, y0 + (j + jy) * h / rows);
                }
            }
            var center = new Vector2(x0 + w / 2f, y0 + h / 2f);
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Vector2 a = grid[i, j], b = grid[i + 1, j], c = grid[i + 1, j + 1], d = grid[i, j + 1];
                    bool flip = ((i + j) & 1) == 0;
                    Shade(flip ? a : b, flip ? b : c, flip ? c : d, center, w, h, tilt, color, bevel, shape);
                    Shade(flip ? a : b, flip ? c : d, flip ? d : a, center, w, h, tilt, color, bevel, shape);
                }
            }
        }

        private void Shade(Vector2 a, Vector2 b, Vector2 c, Vector2 center, float w, float h, float tilt, Color color,
            bool bevel, List<Vector2> shape)
        {
            var normal = new Vector3((float)(_random.NextDouble() - 0.5) * tilt, (float)(_random.NextDouble() - 0.5) * tilt,
                1f);
            if (bevel)
            {
                Vector2 from = ((a + b + c) / 3f - center) / new Vector2(w, h);
                // Le bord le plus proche décide du côté où penche la facette du cadre.
                Vector2 side = Mathf.Abs(from.x) > Mathf.Abs(from.y)
                    ? new Vector2(Mathf.Sign(from.x), 0f)
                    : new Vector2(0f, Mathf.Sign(from.y));
                normal += new Vector3(side.x, side.y, 0f) * BevelSlope;
            }
            float light = Mathf.Clamp(Vector3.Dot(normal.normalized, Light), 0.35f, 1.1f);
            var shaded = new Color(color.r * (0.45f + 0.6f * light), color.g * (0.45f + 0.6f * light),
                color.b * (0.45f + 0.6f * light), 1f);
            Triangle(a, b, c, shaded, shape);
        }

        private void Triangle(Vector2 a, Vector2 b, Vector2 c, Color color, List<Vector2> shape)
        {
            int xMin = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
            int xMax = Mathf.Min(_width - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
            int yMin = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
            int yMax = Mathf.Min(_height - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));
            float area = Edge(a, b, c);
            if (Mathf.Abs(area) < 1e-6f)
                return;
            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    float w0 = Edge(b, c, p) / area, w1 = Edge(c, a, p) / area, w2 = Edge(a, b, p) / area;
                    if (w0 < 0f || w1 < 0f || w2 < 0f || (shape != null && !Inside(shape, p)))
                        continue;
                    _pixels[y * _width + x] = color;
                }
            }
        }

        private static float Edge(Vector2 a, Vector2 b, Vector2 p)
        {
            return (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
        }

        // Un polygone convexe, sommets dans le sens direct : le point est à gauche de chaque côté.
        private static bool Inside(List<Vector2> polygon, Vector2 p)
        {
            for (int i = 0; i < polygon.Count; i++)
            {
                if (Edge(polygon[i], polygon[(i + 1) % polygon.Count], p) < 0f)
                    return false;
            }
            return true;
        }
    }
}
