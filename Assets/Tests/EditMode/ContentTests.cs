using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le contenu du jeu, lu dans le projet sans rien écrire (ARCHITECTURE, section 12) : un seul catalogue, qui mène à
    /// chaque fiche de construction une seule fois ; des identifiants uniques ; chaque texte cité a sa clé, non vide,
    /// dans chaque langue de la table (constructions, têtes, corps, couleurs, emplacements, pièces, la ville au travail) ;
    /// chaque modèle préparé suit sa fiche. La Chimère et la ville au travail ont leurs tests à part.
    /// </summary>
    public sealed class ContentTests
    {
        [Test]
        public void TheCatalogReachesEveryStructureOnce()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
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
            InterfaceSettings settings = ProjectContent.LoadSingle<InterfaceSettings>();
            Assert.IsNotNull(settings.Texts, "Les réglages de l'interface n'ont pas leur table des textes.");
            string csv = settings.Texts.text;

            var keys = new List<string>();
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
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
            ProjectContent.AddTownKeys(catalog, keys);
            keys.AddRange(new[]
            {
                TextKeys.CreationTitle, TextKeys.CreationHead, TextKeys.CreationBody, TextKeys.CreationNone,
                TextKeys.CreationConfirm, TextKeys.DayNumber, TextKeys.SheetPutChimera, TextKeys.SheetRemoveChimera,
                TextKeys.SheetFreeSlot, TextKeys.SheetSleep, TextKeys.SheetWake,
                TextKeys.StockTooltip, TextKeys.TipCamp, TextKeys.TipPost, TextKeys.TipCabin, TextKeys.BuildTool,
                TextKeys.BuildToolDetail, TextKeys.ClearingTool, TextKeys.ClearingToolDetail, TextKeys.ToolsTooltip,
                TextKeys.ToolsTooltipDetail, TextKeys.CardBeds, TextKeys.PlazaArea, TextKeys.PlazaMarks,
                TextKeys.PathsTool, TextKeys.PathsToolDetail, TextKeys.PaintBrushSize, TextKeys.PlazaRemoveMark,
                TextKeys.PlazaZoneInk, TextKeys.PlazaZoneInkDetail, TextKeys.FloorInkDetail, TextKeys.CardMarked,
                TextKeys.CardStar, TextKeys.ForesightTooltip, TextKeys.ForesightTooltipDetail
            });
            foreach (DayPhase phase in Enum.GetValues(typeof(DayPhase)))
            {
                keys.Add(TextKeys.DayPhase(phase.ToString()));
                keys.Add(TextKeys.DayNext(phase.ToString()));
                keys.Add(TextKeys.DayNote(phase.ToString()));
            }
            foreach (PlacementResult result in Enum.GetValues(typeof(PlacementResult)))
            {
                if (result != PlacementResult.Ok)
                    keys.Add(TextKeys.PlacementRefusal(result.ToString()));
            }
            foreach (PlazaResult result in Enum.GetValues(typeof(PlazaResult)))
            {
                if (result != PlazaResult.Ok)
                    keys.Add(TextKeys.PlazaRefusal(result.ToString()));
            }
            foreach (FloorResult result in Enum.GetValues(typeof(FloorResult)))
            {
                if (result != FloorResult.Ok)
                    keys.Add(TextKeys.FloorRefusal(result.ToString()));
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
            // supérieur), la porte sur la façade, la couche dans l'empreinte. Le campement, sans murs, a sa couche :
            // sans elle, la Chimère y disparaîtrait en dormant (décision 172).
            const float Centimeter = 0.01f;
            var faults = new List<string>();
            foreach (StructureDefinition structure in ProjectContent.LoadSingle<Catalog>().AllStructures())
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
                if (structure.HasCouch && (Mathf.Abs(structure.CouchFeet.x) > structure.WidthMeters * 0.5f ||
                                         Mathf.Abs(structure.CouchFeet.z) > structure.DepthMeters * 0.5f))
                    faults.Add(structure.Id + " : sa couche est hors de son empreinte.");
                if (structure.IsChimeraHome && structure.BuiltWhenPlaced && !structure.HasCouch)
                    faults.Add(structure.Id + " : le campement n'a pas de couche (un objet « Couche » dans sa composition).");
            }

            Assert.IsEmpty(faults, string.Join("\n", faults));

            bool Fits(float model, float fiche) => model <= fiche + 0.001f && model >= fiche - Centimeter - 0.001f;
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
