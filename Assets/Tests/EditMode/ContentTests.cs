using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le contenu du jeu, lu dans le projet sans rien écrire (ARCHITECTURE, section 12) : un seul catalogue, qui mène à
    /// chaque fiche de construction une seule fois ; des identifiants uniques ; chaque texte cité a sa clé, non vide,
    /// dans chaque langue de la table (constructions, têtes, corps, couleurs, emplacements, pièces) ; chaque modèle préparé suit
    /// sa fiche. La Chimère a ses tests à part.
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
            Catalog catalog = LoadSingle<Catalog>();
            foreach (StructureFamilyDefinition family in catalog.Families)
            {
                keys.Add(family.NameKey);
                foreach (StructureDefinition structure in family.Structures)
                    keys.Add(structure.NameKey);
            }
            foreach (HeadDefinition head in catalog.Heads)
                keys.Add(head.NameKey);
            foreach (BodyDefinition body in catalog.Bodies)
                keys.Add(body.NameKey);
            foreach (PaletteDefinition palette in catalog.Palettes)
                keys.Add(palette.NameKey);
            foreach (AttachmentSlotDefinition slot in catalog.AttachmentSlots)
                keys.Add(slot.NameKey);
            foreach (AttachmentDefinition attachment in catalog.Attachments)
                keys.Add(attachment.NameKey);
            keys.AddRange(new[]
            {
                TextKeys.CreationTitle, TextKeys.CreationHead, TextKeys.CreationBody, TextKeys.CreationNone,
                TextKeys.CreationPrevious, TextKeys.CreationNext, TextKeys.CreationConfirm
            });
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

        [Test]
        public void EveryModelIsPreparedToItsFootprint()
        {
            // Le modèle préparé (SAVOIR, 2.9), depuis le pack ou en boîte faute de modèle choisi : un seul maillage, un
            // matériau par sous-maillage, centré sur l'empreinte de la fiche et à sa taille (arrondie au centimètre
            // supérieur), la porte sur la façade.
            const float Centimeter = 0.01f;
            var faults = new List<string>();
            foreach (StructureFamilyDefinition family in LoadSingle<Catalog>().Families)
            {
                foreach (StructureDefinition structure in family.Structures)
                {
                    if (structure.Model == null)
                    {
                        faults.Add(structure.Id + " n'a pas de modèle préparé.");
                        continue;
                    }
                    MeshFilter[] filters = structure.Model.GetComponentsInChildren<MeshFilter>(true);
                    var renderer = structure.Model.GetComponent<MeshRenderer>();
                    if (filters.Length != 1 || renderer == null || filters[0].gameObject != structure.Model ||
                        renderer.sharedMaterials.Length != filters[0].sharedMesh.subMeshCount)
                    {
                        faults.Add(structure.Id + " n'a pas de modèle préparé d'un seul maillage, depuis sa source.");
                        continue;
                    }

                    Bounds box = filters[0].sharedMesh.bounds;
                    if (Mathf.Abs(box.center.x) > Centimeter || Mathf.Abs(box.center.z) > Centimeter)
                        faults.Add(structure.Id + " : le modèle n'est pas centré sur son empreinte (" + box.center + ").");
                    if (!Fits(box.size.x, structure.WidthMeters) || !Fits(box.size.z, structure.DepthMeters))
                        faults.Add(structure.Id + " : le modèle mesure " + box.size.x + " × " + box.size.z +
                                   " m, sa fiche " + structure.WidthMeters + " × " + structure.DepthMeters + " m.");
                    if (structure.HasDoor && Mathf.Abs(structure.DoorOffsetMeters) + structure.DoorWidthMeters * 0.5f >
                        structure.WidthMeters * 0.5f + Centimeter)
                        faults.Add(structure.Id + " : sa porte déborde de sa façade.");
                }
            }

            Assert.IsEmpty(faults, string.Join("\n", faults));

            bool Fits(float model, float fiche) => model <= fiche + 0.001f && model >= fiche - Centimeter - 0.001f;
        }

        [Test]
        public void TheCatalogHasOneChimeraHomeFirstUniqueWithADoor()
        {
            // La tente vient en premier (GDD, section 6) : le catalogue a un logis de la Chimère, en tête de sa famille ;
            // il est unique, et il a une porte, devant laquelle elle se couche.
            var homes = new List<StructureDefinition>();
            foreach (StructureFamilyDefinition family in LoadSingle<Catalog>().Families)
            {
                for (int i = 0; i < family.Structures.Count; i++)
                {
                    StructureDefinition structure = family.Structures[i];
                    if (!structure.IsChimeraHome)
                        continue;
                    homes.Add(structure);
                    Assert.AreEqual(0, i, structure.Id + " n'est pas en tête de sa famille.");
                    Assert.IsTrue(structure.IsUnique, structure.Id + " n'est pas unique.");
                    Assert.IsTrue(structure.HasDoor, structure.Id + " n'a pas de porte.");
                }
            }
            Assert.AreEqual(1, homes.Count, "Le catalogue doit avoir un seul logis de la Chimère : la tente.");
        }

        /// <summary>La seule fiche de ce type dans le projet ; il y en a une, et une seule.</summary>
        internal static T LoadSingle<T>() where T : UnityEngine.Object
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
