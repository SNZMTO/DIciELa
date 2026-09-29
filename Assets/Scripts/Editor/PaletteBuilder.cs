using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les couleurs de l'apparence (GDD, section 6) : les douze textures du pack, chacune tenue par ses deux matériaux, celui
    /// des personnages (corps, têtes, queues) et celui des objets (les autres pièces). Crée les fiches qui manquent et
    /// reconnaît la couleur d'un rendu du pack.
    /// </summary>
    internal static class PaletteBuilder
    {
        private const string Folder = "Assets/Data/Apparence/Couleurs";
        private const string Materials = "Assets/Synty/PolygonFantasyVillage/Materials/Alts";
        private const int Atlases = 4;
        private const string Variants = "ABC";

        /// <summary>Les douze couleurs, dans l'ordre des textures du pack ; créées si elles manquent.</summary>
        public static List<PaletteDefinition> CreateMissing()
        {
            var palettes = new List<PaletteDefinition>();
            for (int atlas = 1; atlas <= Atlases; atlas++)
            {
                foreach (char variant in Variants)
                {
                    string number = (palettes.Count + 1).ToString("00");
                    string texture = "0" + atlas + "_" + variant;
                    var character = EditorAssets.Load<Material>(Materials + "/Animal_Alts/PolygonFantasyVillage_Animal_" +
                                                                texture + ".mat");
                    var objects = EditorAssets.Load<Material>(Materials + "/PolygonFantasyVillage_" + texture + ".mat");
                    PaletteDefinition palette = EditorAssets.CreateIfMissing<PaletteDefinition>(
                        Folder + "/Couleur_" + number + ".asset", fiche =>
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
    }
}
