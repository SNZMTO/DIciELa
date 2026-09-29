using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée l'apparence qui manque (GDD, section 6), depuis le pack : les couleurs (ses douze textures, par leurs matériaux
    /// de personnage et d'objet), les emplacements et les pièces (AttachmentBuilder), puis, de chacun de ses seize
    /// personnages d'animaux, une tête et un corps séparés (BodyCutter), dans leur couleur d'origine, le corps avec les
    /// pièces que le personnage porte dans le pack. Les range dans le catalogue. Réexécutable : une fiche qui existe n'est jamais touchée, sauf pour ce qui lui
    /// manque ; une liste du catalogue gagne ce qui lui manque, à la suite, sans rien perdre de ce qu'on y a réglé.
    /// </summary>
    internal static class AppearanceBuilder
    {
        private const string Folder = "Assets/Data/Apparence";
        private const string Characters = "Assets/Synty/PolygonFantasyVillage/Prefabs/Characters";
        private const string Palettes = "Assets/Synty/PolygonFantasyVillage/Materials/Alts";
        private const int PaletteAtlases = 4;
        private const string PaletteVariants = "ABC";

        // Les animaux du pack, dans l'ordre de l'écran : le chat d'abord, la Chimère de départ. Deux silhouettes chacun,
        // A et B (Female et Male dans le pack) : des noms neutres, que le développeur réécrit dans la table des textes.
        private static readonly (string Pack, string Name)[] Animals =
        {
            ("Cat", "Chat"), ("Pig", "Cochon"), ("Frog", "Grenouille"), ("Bunny", "Lapin"), ("Bird", "Oiseau"),
            ("Racoon", "RatonLaveur"), ("Fox", "Renard"), ("Mouse", "Souris")
        };

        private static readonly (string Pack, string Letter)[] Builds = { ("Female", "A"), ("Male", "B") };

        public static void CreateMissing(Catalog catalog)
        {
            List<PaletteDefinition> palettes = CreatePalettes();
            var slots = new List<AttachmentSlotDefinition>();
            var pieces = new List<AttachmentDefinition>();
            GameObject first = EditorAssets.Load<GameObject>(Characters + "/SM_Chr_Cat_Female_01.prefab");
            AttachmentBuilder.CreateMissing(first, palettes, slots, pieces);
            var heads = new List<HeadDefinition>();
            var bodies = new List<BodyDefinition>();
            foreach ((string pack, string name) in Animals)
            {
                foreach ((string build, string letter) in Builds)
                {
                    var model = EditorAssets.Load<GameObject>(Characters + "/SM_Chr_" + pack + "_" + build + "_01.prefab");
                    string file = name + "_" + letter;
                    (Mesh head, Mesh body) = BodyCutter.CutIfMissing(model, file);
                    PaletteDefinition palette = PaletteOf(BodyCutter.VisibleBody(model), palettes);
                    heads.Add(HeadIfMissing(file, head, palette));
                    bodies.Add(BodyIfMissing(file, model, body, palette, pieces));
                }
            }

            AddMissing(catalog, "_heads", heads);
            AddMissing(catalog, "_bodies", bodies);
            AddMissing(catalog, "_palettes", palettes);
            AddMissing(catalog, "_attachmentSlots", slots);
            AddMissing(catalog, "_attachments", pieces);
        }

        private static HeadDefinition HeadIfMissing(string file, Mesh mesh, PaletteDefinition palette)
        {
            string id = file.ToLowerInvariant();
            HeadDefinition head = EditorAssets.CreateIfMissing<HeadDefinition>(Folder + "/Tetes/" + file + ".asset",
                fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = id;
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "tete." + id;
                });
            EditorAssets.AssignIfMissing(head, "_mesh", mesh);
            EditorAssets.AssignIfMissing(head, "_palette", palette);
            return head;
        }

        private static BodyDefinition BodyIfMissing(string file, GameObject model, Mesh mesh, PaletteDefinition palette,
            List<AttachmentDefinition> pieces)
        {
            string id = file.ToLowerInvariant();
            BodyDefinition body = EditorAssets.CreateIfMissing<BodyDefinition>(Folder + "/Corps/" + file + ".asset",
                fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = id;
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "corps." + id;
                    SetList(EditorAssets.Property(fiche, "_worn"), WornBy(model, pieces));
                });
            EditorAssets.AssignIfMissing(body, "_model", model);
            EditorAssets.AssignIfMissing(body, "_mesh", mesh);
            EditorAssets.AssignIfMissing(body, "_palette", palette);
            return body;
        }

        /// <summary>
        /// Les pièces que ce personnage porte dans le pack : celles dont le maillage est aussi celui d'un de ses objets (le
        /// pack y pose des copies des modèles, pas les prefabs de Chr_Attach).
        /// </summary>
        private static List<AttachmentDefinition> WornBy(GameObject model, List<AttachmentDefinition> pieces)
        {
            var worn = new List<AttachmentDefinition>();
            foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>(true))
            {
                foreach (AttachmentDefinition piece in pieces)
                {
                    var own = piece.Model != null ? piece.Model.GetComponentInChildren<MeshFilter>(true) : null;
                    if (own != null && own.sharedMesh == filter.sharedMesh && !worn.Contains(piece))
                        worn.Add(piece);
                }
            }
            return worn;
        }

        private static List<PaletteDefinition> CreatePalettes()
        {
            var palettes = new List<PaletteDefinition>();
            for (int atlas = 1; atlas <= PaletteAtlases; atlas++)
            {
                foreach (char variant in PaletteVariants)
                {
                    string number = (palettes.Count + 1).ToString("00");
                    string texture = "0" + atlas + "_" + variant;
                    var character = EditorAssets.Load<Material>(Palettes + "/Animal_Alts/PolygonFantasyVillage_Animal_" +
                                                                texture + ".mat");
                    var objects = EditorAssets.Load<Material>(Palettes + "/PolygonFantasyVillage_" + texture + ".mat");
                    PaletteDefinition palette = EditorAssets.CreateIfMissing<PaletteDefinition>(
                        Folder + "/Couleurs/Couleur_" + number + ".asset", fiche =>
                        {
                            EditorAssets.Property(fiche, "_id").stringValue = "couleur_" + number;
                            EditorAssets.Property(fiche, "_nameKey").stringValue = "couleur." + number;
                        });
                    EditorAssets.AssignIfMissing(palette, "_characterMaterial", character);
                    EditorAssets.AssignIfMissing(palette, "_objectMaterial", objects);
                    palettes.Add(palette);
                }
            }
            return palettes;
        }

        /// <summary>
        /// La couleur d'un rendu du pack : celle dont l'un des deux matériaux est le sien ; null (et dit) si ce n'en est pas
        /// une.
        /// </summary>
        public static PaletteDefinition PaletteOf(Renderer renderer, List<PaletteDefinition> palettes)
        {
            foreach (PaletteDefinition palette in palettes)
            {
                if (renderer.sharedMaterial == palette.CharacterMaterial || renderer.sharedMaterial == palette.ObjectMaterial)
                    return palette;
            }
            Debug.LogError("DICIELA : " + renderer.name + " n'a pas une couleur du pack.", renderer);
            return null;
        }

        private static void SetList(SerializedProperty list, List<AttachmentDefinition> items)
        {
            list.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        /// <summary>Ajoute à une liste du catalogue ce qui lui manque, à la suite ; ce qui y est reste où on l'a mis.</summary>
        private static void AddMissing<T>(Catalog catalog, string listName, List<T> items) where T : Object
        {
            var serialized = new SerializedObject(catalog);
            SerializedProperty list = EditorAssets.Property(serialized, listName);
            var present = new HashSet<Object>();
            for (int i = 0; i < list.arraySize; i++)
                present.Add(list.GetArrayElementAtIndex(i).objectReferenceValue);
            foreach (T item in items)
            {
                if (item == null || present.Contains(item))
                    continue;
                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = item;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }
    }
}
