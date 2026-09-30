using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les couleurs de la feuille de style de l'interface (le bloc :root de Fusion.uss), lues par les générateurs
    /// d'images : une couleur ne s'écrit qu'une fois, dans la feuille.
    /// </summary>
    internal sealed class UssPalette
    {
        public const string StyleSheetPath = "Assets/UI/Fusion.uss";

        private static readonly Regex Hex = new Regex(@"--([a-z-]+)\s*:\s*#([0-9A-Fa-f]{6})\s*;");
        private static readonly Regex Rgba = new Regex(
            @"--([a-z-]+)\s*:\s*rgba\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*([0-9.]+)\s*\)\s*;");

        private readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>();

        private UssPalette()
        {
        }

        public static UssPalette Read()
        {
            string text = File.ReadAllText(StyleSheetPath);
            int start = text.IndexOf(":root", System.StringComparison.Ordinal);
            int end = start < 0 ? -1 : text.IndexOf('}', start);
            if (start < 0 || end < 0)
                throw new InvalidDataException("La feuille " + StyleSheetPath + " n'a pas de bloc :root.");

            string root = text.Substring(start, end - start);
            var palette = new UssPalette();
            foreach (Match match in Hex.Matches(root))
            {
                ColorUtility.TryParseHtmlString("#" + match.Groups[2].Value, out Color color);
                palette._colors[match.Groups[1].Value] = color;
            }
            foreach (Match match in Rgba.Matches(root))
            {
                palette._colors[match.Groups[1].Value] = new Color(Channel(match, 2), Channel(match, 3),
                    Channel(match, 4), float.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture));
            }
            return palette;
        }

        /// <summary>La couleur --name ; une couleur absente est une erreur de la feuille, pas un noir silencieux.</summary>
        public Color Get(string name)
        {
            if (!_colors.TryGetValue(name, out Color color))
                throw new KeyNotFoundException("La couleur --" + name + " manque au bloc :root de " + StyleSheetPath + ".");
            return color;
        }

        private static float Channel(Match match, int group)
        {
            return int.Parse(match.Groups[group].Value, CultureInfo.InvariantCulture) / 255f;
        }
    }
}
