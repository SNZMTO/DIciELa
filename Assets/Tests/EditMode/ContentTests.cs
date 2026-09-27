using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEditor;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le contenu du jeu, lu dans le projet sans rien écrire (ARCHITECTURE, section 12) : un seul catalogue, qui mène à
    /// chaque fiche de construction une seule fois ; des identifiants uniques ; chaque texte cité a sa clé, non vide,
    /// dans chaque langue de la table.
    /// </summary>
    public sealed class ContentTests
    {
        [Test]
        public void TheCatalogReachesEveryStructureOnce()
        {
            Catalog catalog = LoadSingle<Catalog>();
            var familyIds = new HashSet<string>();
            var structureIds = new HashSet<string>();
            var reached = new HashSet<StructureDefinition>();
            var faults = new List<string>();

            foreach (StructureFamilyDefinition family in catalog.Families)
            {
                if (family == null || string.IsNullOrEmpty(family.Id) || !familyIds.Add(family.Id))
                {
                    faults.Add("Une famille du catalogue est vide, sans identifiant, ou en double.");
                    continue;
                }
                foreach (StructureDefinition structure in family.Structures)
                {
                    if (structure == null || !reached.Add(structure))
                        faults.Add("La famille « " + family.Id + " » a une construction vide, ou déjà dans une famille.");
                    else if (string.IsNullOrEmpty(structure.Id) || !structureIds.Add(structure.Id))
                        faults.Add(structure.name + " : identifiant vide, ou déjà pris.");
                    else if (structure.Model == null)
                        faults.Add(structure.name + " n'a pas de modèle.");
                }
            }

            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(StructureDefinition)))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!reached.Contains(AssetDatabase.LoadAssetAtPath<StructureDefinition>(path)))
                    faults.Add(path + " n'est dans aucune famille du catalogue.");
            }

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        [Test]
        public void EveryCitedTextExistsInEveryLanguage()
        {
            InterfaceSettings settings = LoadSingle<InterfaceSettings>();
            Assert.IsNotNull(settings.Texts, "Les réglages de l'interface n'ont pas leur table des textes.");
            string csv = settings.Texts.text;

            var keys = new List<string>();
            foreach (StructureFamilyDefinition family in LoadSingle<Catalog>().Families)
            {
                keys.Add(family.NameKey);
                foreach (StructureDefinition structure in family.Structures)
                    keys.Add(structure.NameKey);
            }
            foreach (PlacementResult result in Enum.GetValues(typeof(PlacementResult)))
            {
                if (result != PlacementResult.Ok)
                    keys.Add(TextKeys.PlacementRefusal(result.ToString()));
            }

            var faults = new List<string>();
            foreach (string language in LanguagesOf(csv))
            {
                TextTable table = TextTable.Parse(csv, language);
                foreach (string key in keys)
                {
                    if (!table.Has(key))
                        faults.Add(language + " : pas de texte pour « " + key + " ».");
                }
            }

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        private static T LoadSingle<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            Assert.AreEqual(1, guids.Length, "Il faut une seule fiche " + typeof(T).Name + " dans le projet.");
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        /// <summary>Les langues de la table : les colonnes de sa première ligne, après celle des clés.</summary>
        private static IEnumerable<string> LanguagesOf(string csv)
        {
            string[] columns = csv.Split('\n')[0].Trim().TrimStart('﻿').Split(',');
            for (int i = 1; i < columns.Length; i++)
                yield return columns[i];
        }
    }
}
