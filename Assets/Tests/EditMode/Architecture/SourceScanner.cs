using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Diciela.Tests.EditMode.Architecture
{
    /// <summary>
    /// Cherche dans le code de la simulation ce qu'elle n'a pas le droit d'utiliser (ARCHITECTURE, sections 9 et 12).
    /// Les commentaires et les chaînes sont retirés avant la recherche, sauf le code des trous d'une chaîne interpolée.
    /// L'état statique modifiable, c'est un champ, une propriété qu'on peut écrire, ou une collection ; un tableau
    /// readonly reste permis, pour une table qu'on ne fait que lire. « == null » sur une fiche échappe à la recherche :
    /// le texte ne dit pas le type de ce qu'on compare.
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
            ("AnimationCurve ou Gradient (une fiche ne donne que des nombres)",
                new Regex(@"\b(AnimationCurve|Gradient)\b")),
            (".name (l'identifiant d'une fiche est Id)", new Regex(@"\.name\b")),
            ("champ statique modifiable", new Regex(
                @"\bstatic\s+(?!readonly\b|class\b|void\b|extern\b|partial\b)(?:[\w<>\[\],.?]+\s+)+\w+\s*(=(?!>)|;)")),
            ("propriété statique modifiable", new Regex(
                @"\bstatic\s+(?!readonly\b|class\b|void\b|extern\b|partial\b)(?:[\w<>\[\],.?]+\s+)+\w+\s*\{[^}]*\bset\b")),
            ("collection statique modifiable", new Regex(
                @"\bstatic\s+readonly\s+(?:[\w.]+\.)?(List|Dictionary|HashSet|Queue|Stack|SortedDictionary|SortedList|SortedSet|LinkedList|StringBuilder)\b")),
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

        /// <summary>
        /// Remplace commentaires et chaînes par des espaces, en gardant les retours à la ligne, et le code des trous d'une
        /// chaîne interpolée.
        /// </summary>
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
                else if (c == '$' && next == '"')
                    i = SkipInterpolated(source, i + 1, result);
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

        // Une chaîne interpolée, son guillemet en start : son texte est retiré (« {{ » et « }} » en font partie), le code
        // de ses trous reste.
        private static int SkipInterpolated(string source, int start, StringBuilder result)
        {
            int i = start + 1;
            while (i < source.Length && source[i] != '"' && source[i] != '\n')
            {
                char c = source[i];
                char next = i + 1 < source.Length ? source[i + 1] : '\0';
                if (c == '\\' || (c == '{' && next == '{') || (c == '}' && next == '}'))
                    i += 2;
                else if (c == '{')
                    i = KeepHole(source, i + 1, result);
                else
                    i++;
            }
            result.Append(' ');
            return i < source.Length && source[i] == '"' ? i + 1 : i;
        }

        // Le code d'un trou, jusqu'à son accolade fermante ; une chaîne qu'il contient est retirée.
        private static int KeepHole(string source, int start, StringBuilder result)
        {
            result.Append(' ');
            int depth = 0;
            int i = start;
            while (i < source.Length && source[i] != '\n')
            {
                char c = source[i];
                if (c == '"' || c == '\'')
                {
                    i = SkipQuoted(source, i, c, result);
                    continue;
                }
                if (c == '}' && depth == 0)
                    return i + 1;
                if (c == '{')
                    depth++;
                else if (c == '}')
                    depth--;
                result.Append(c);
                i++;
            }
            return i;
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
