using System;
using System.Collections.Generic;
using System.Text;

namespace Diciela.Data
{
    /// <summary>
    /// La table des textes (ARCHITECTURE, section 2) : chaque texte visible a une clé, et une colonne par langue. Elle se
    /// lit dans Assets/Localization/strings.csv : la première ligne nomme les colonnes (la clé, puis les langues) ; un
    /// champ qui contient une virgule, un guillemet ou un retour à la ligne s'écrit entre guillemets, et un guillemet
    /// s'y double.
    /// </summary>
    public sealed class TextTable
    {
        private readonly Dictionary<string, string> _texts;

        private TextTable(string language, Dictionary<string, string> texts)
        {
            Language = language;
            _texts = texts;
        }

        /// <summary>La langue lue : le nom de sa colonne.</summary>
        public string Language { get; }

        /// <summary>Lit la table dans cette langue. Une langue absente ou une clé en double est une erreur.</summary>
        public static TextTable Parse(string csv, string language)
        {
            if (csv == null)
                throw new ArgumentNullException(nameof(csv));

            List<List<string>> rows = ReadRows(csv);
            int column = rows.Count > 0 ? rows[0].IndexOf(language) : -1;
            if (column < 1)
                throw new ArgumentException("La table des textes n'a pas de colonne « " + language + " ».",
                    nameof(language));

            var texts = new Dictionary<string, string>();
            for (int i = 1; i < rows.Count; i++)
            {
                List<string> row = rows[i];
                string key = row[0];
                if (key.Length == 0)
                    continue;
                if (texts.ContainsKey(key))
                    throw new ArgumentException("La clé « " + key + " » est deux fois dans la table des textes.",
                        nameof(csv));
                texts.Add(key, column < row.Count ? row[column] : "");
            }
            return new TextTable(language, texts);
        }

        /// <summary>Vrai si cette clé a un texte, non vide, dans cette langue.</summary>
        public bool Has(string key)
        {
            return key != null && _texts.TryGetValue(key, out string text) && text.Length > 0;
        }

        /// <summary>Le texte de cette clé ; une clé sans texte se voit à l'écran, entre crochets.</summary>
        public string Get(string key)
        {
            return Has(key) ? _texts[key] : "[" + key + "]";
        }

        /// <summary>Les lignes du fichier, champ par champ ; une ligne vide rend un seul champ vide.</summary>
        private static List<List<string>> ReadRows(string csv)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;
            int start = csv.Length > 0 && csv[0] == '﻿' ? 1 : 0;

            for (int i = start; i < csv.Length; i++)
            {
                char c = csv[i];
                if (quoted)
                {
                    bool doubled = c == '"' && i + 1 < csv.Length && csv[i + 1] == '"';
                    if (doubled)
                        i++;
                    if (c == '"' && !doubled)
                        quoted = false;
                    else
                        field.Append(c);
                }
                else if (c == '"')
                    quoted = true;
                else if (c == ',')
                    EndField(row, field);
                else if (c == '\n' || c == '\r')
                {
                    if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                        i++;
                    EndField(row, field);
                    rows.Add(row);
                    row = new List<string>();
                }
                else
                    field.Append(c);
            }

            if (field.Length > 0 || row.Count > 0)
            {
                EndField(row, field);
                rows.Add(row);
            }
            return rows;
        }

        private static void EndField(List<string> row, StringBuilder field)
        {
            row.Add(field.ToString());
            field.Clear();
        }
    }
}
