using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Diciela.Tests.EditMode.Architecture
{
    /// <summary>
    /// Cherche dans le code de la simulation ce qu'elle n'a pas le droit d'utiliser (ARCHITECTURE, section 12).
    /// Les commentaires et les chaînes sont retirés avant la recherche.
    /// </summary>
    internal static class SourceScanner
    {
        private static readonly (string Rule, Regex Pattern)[] ForbiddenPatterns =
        {
            ("MonoBehaviour", new Regex(@"\bMonoBehaviour\b")),
            ("GameObject", new Regex(@"\bGameObject\b")),
            ("Time (temps image par image)", new Regex(@"\bTime\s*\.")),
            ("Random (le hasard vient du hachage)", new Regex(@"\bRandom\b")),
            ("Mathf.PerlinNoise", new Regex(@"\bPerlinNoise\b")),
            ("Debug (le journal est dans Game)", new Regex(@"\bDebug\s*\.")),
            ("Resources", new Regex(@"\bResources\s*\.")),
            ("UnityEditor", new Regex(@"\bUnityEditor\b")),
            ("Newtonsoft (réservé à la sauvegarde)", new Regex(@"\bNewtonsoft\b")),
            ("champ statique modifiable", new Regex(
                @"\bstatic\s+(?!readonly\b|class\b|void\b|extern\b|partial\b)(?:[\w<>\[\],.?]+\s+)+\w+\s*(=(?!>)|;)")),
        };

        /// <summary>Rend la liste des règles enfreintes par ce code, une ligne par faute.</summary>
        public static List<string> FindViolations(string source)
        {
            var violations = new List<string>();
            string[] lines = StripCommentsAndStrings(source).Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                foreach ((string rule, Regex pattern) in ForbiddenPatterns)
                {
                    if (pattern.IsMatch(lines[i]))
                        violations.Add("ligne " + (i + 1) + " : " + rule);
                }
            }
            return violations;
        }

        /// <summary>Remplace commentaires et chaînes par des espaces, en gardant les retours à la ligne.</summary>
        public static string StripCommentsAndStrings(string source)
        {
            var result = new StringBuilder(source.Length);
            int i = 0;
            while (i < source.Length)
            {
                char c = source[i];
                char next = i + 1 < source.Length ? source[i + 1] : '\0';
                if (c == '/' && next == '/')
                    i = SkipUntil(source, i, "\n", result, keepEnd: true);
                else if (c == '/' && next == '*')
                    i = SkipUntil(source, i + 2, "*/", result, keepEnd: false);
                else if (c == '"' || c == '\'')
                    i = SkipQuoted(source, i, c, result);
                else
                {
                    result.Append(c);
                    i++;
                }
            }
            return result.ToString();
        }

        private static int SkipUntil(string source, int start, string end, StringBuilder result, bool keepEnd)
        {
            int stop = source.IndexOf(end, start, System.StringComparison.Ordinal);
            if (stop < 0)
                stop = source.Length;
            AppendNewlinesOf(source, start, stop, result);
            if (stop >= source.Length)
                return source.Length;
            return keepEnd ? stop : stop + end.Length;
        }

        private static int SkipQuoted(string source, int start, char quote, StringBuilder result)
        {
            int i = start + 1;
            while (i < source.Length && source[i] != quote && source[i] != '\n')
                i += source[i] == '\\' ? 2 : 1;
            result.Append(' ');
            return i < source.Length && source[i] == quote ? i + 1 : i;
        }

        private static void AppendNewlinesOf(string source, int from, int to, StringBuilder result)
        {
            for (int i = from; i < to && i < source.Length; i++)
            {
                if (source[i] == '\n')
                    result.Append('\n');
            }
        }
    }
}
