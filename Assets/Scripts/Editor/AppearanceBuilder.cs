using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée l'apparence qui manque (GDD, section 6), depuis le pack : les couleurs (ses douze matériaux d'animaux), les
    /// corps (ses seize personnages d'animaux, chacun avec sa couleur d'origine), puis les emplacements et les pièces
    /// (AttachmentBuilder) ; les range dans le catalogue. Réexécutable : une fiche qui existe n'est jamais touchée, sauf
    /// pour ce qui lui manque, et une liste du catalogue déjà remplie non plus.
    /// </summary>
    internal static class AppearanceBuilder
    {
        private const string Folder = "Assets/Data/Apparence";
        private const string Characters = "Assets/Synty/PolygonFantasyVillage/Prefabs/Characters";
        private const string Palettes = "Assets/Synty/PolygonFantasyVillage/Materials/Alts/Animal_Alts";
        private const int PaletteAtlases = 4;
        private const string PaletteVariants = "ABC";

        // Les animaux du pack, dans l'ordre de l'écran : le chat d'abord, le corps de départ. Deux corps chacun, A et B
        // (Female et Male dans le pack) : des noms neutres, que le développeur réécrit dans la table des textes.
        private static readonly (string Pack, string Name)[] Animals =
        {
            ("Cat", "Chat"), ("Pig", "Cochon"), ("Frog", "Grenouille"), ("Bunny", "Lapin"), ("Bird", "Oiseau"),
            ("Racoon", "RatonLaveur"), ("Fox", "Renard"), ("Mouse", "Souris")
        };

        private static readonly (string Pack, string Letter)[] Builds = { ("Female", "A"), ("Male", "B") };

        public static void CreateMissing(Catalog catalog)
        {
            List<PaletteDefinition> palettes = CreatePalettes();
            List<BodyDefinition> bodies = CreateBodies(palettes);
            var slots = new List<AttachmentSlotDefinition>();
            var pieces = new List<AttachmentDefinition>();
            AttachmentBuilder.CreateMissing(bodies[0].Model, slots, pieces);

            FillIfEmpty(catalog, "_bodies", bodies);
            FillIfEmpty(catalog, "_palettes", palettes);
            FillIfEmpty(catalog, "_attachmentSlots", slots);
            FillIfEmpty(catalog, "_attachments", pieces);
        }

        private static List<PaletteDefinition> CreatePalettes()
        {
            var palettes = new List<PaletteDefinition>();
            for (int atlas = 1; atlas <= PaletteAtlases; atlas++)
            {
                foreach (char variant in PaletteVariants)
                {
                    string number = (palettes.Count + 1).ToString("00");
                    var material = EditorAssets.Load<Material>(Palettes + "/PolygonFantasyVillage_Animal_0" + atlas + "_" +
                                                               variant + ".mat");
                    PaletteDefinition palette = EditorAssets.CreateIfMissing<PaletteDefinition>(
                        Folder + "/Couleurs/Couleur_" + number + ".asset", fiche =>
                        {
                            EditorAssets.Property(fiche, "_id").stringValue = "couleur_" + number;
                            EditorAssets.Property(fiche, "_nameKey").stringValue = "couleur." + number;
                        });
                    EditorAssets.AssignIfMissing(palette, "_material", material);
                    palettes.Add(palette);
                }
            }
            return palettes;
        }

        private static List<BodyDefinition> CreateBodies(List<PaletteDefinition> palettes)
        {
            var bodies = new List<BodyDefinition>();
            foreach ((string pack, string name) in Animals)
            {
                foreach ((string build, string letter) in Builds)
                {
                    var model = EditorAssets.Load<GameObject>(Characters + "/SM_Chr_" + pack + "_" + build + "_01.prefab");
                    string id = (name + "_" + letter).ToLowerInvariant();
                    BodyDefinition body = EditorAssets.CreateIfMissing<BodyDefinition>(
                        Folder + "/Corps/" + name + "_" + letter + ".asset", fiche =>
                        {
                            EditorAssets.Property(fiche, "_id").stringValue = id;
                            EditorAssets.Property(fiche, "_nameKey").stringValue = "corps." + id;
                        });
                    EditorAssets.AssignIfMissing(body, "_model", model);
                    EditorAssets.AssignIfMissing(body, "_palette", PaletteOf(model, palettes));
                    bodies.Add(body);
                }
            }
            return bodies;
        }

        /// <summary>
        /// La couleur d'origine d'un personnage : le matériau de son corps allumé, un enfant direct du prefab. Plus bas, une
        /// cape du pack peut porter une autre couleur (l'oiseau, le lapin, la souris et le cochon en ont une d'office).
        /// </summary>
        private static PaletteDefinition PaletteOf(GameObject model, List<PaletteDefinition> palettes)
        {
            foreach (Transform child in model.transform)
            {
                var body = child.GetComponent<SkinnedMeshRenderer>();
                if (body == null || !child.gameObject.activeSelf)
                    continue;
                foreach (PaletteDefinition palette in palettes)
                {
                    if (body.sharedMaterial == palette.Material)
                        return palette;
                }
            }
            Debug.LogError("DICIELA : " + model.name + " n'a pas de corps allumé d'une couleur du pack.", model);
            return null;
        }

        /// <summary>Remplit une liste du catalogue si elle est vide ; une liste réglée à la main n'est pas touchée.</summary>
        private static void FillIfEmpty<T>(Catalog catalog, string listName, List<T> items) where T : Object
        {
            var serialized = new SerializedObject(catalog);
            SerializedProperty list = EditorAssets.Property(serialized, listName);
            if (list.arraySize > 0)
                return;
            list.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }
    }
}
