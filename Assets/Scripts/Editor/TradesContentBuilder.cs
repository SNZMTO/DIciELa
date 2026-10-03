using Diciela.Data;
using UnityEngine.UIElements;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée ce qui manque des métiers (GDD, sections 7, 8 et 11 ; décisions 190 et 197) : les postes d'ouvrier (bâtir)
    /// et de charretier (porter, puis conduire) ; la loge des bâtisseurs, 5 × 4 m, 15 Bois et 10 Pierre, 3 h, deux
    /// postes d'ouvrier ; l'entrepôt, 6 × 5 m, 20 Bois et 10 Pierre, 3 h, 200 places, deux postes de charretier, sa
    /// charrette ; chacun en boîte à sa taille, en attendant son modèle du pack ; et leur famille, les Métiers (pêche,
    /// l'icône « bâtir »). Ce sont les lieux de travail dont les premiers habitants apportent l'idée. Réexécutable :
    /// une fiche qui existe n'est jamais touchée.
    /// </summary>
    internal static class TradesContentBuilder
    {
        private const string StructuresFolder = "Assets/Data/Structures";
        private const string FamilyPath = StructuresFolder + "/Famille_Metiers.asset";

        /// <summary>Ce que le monteur rend : la loge, l'entrepôt et leur famille.</summary>
        internal sealed class Made
        {
            public StructureDefinition Lodge;
            public StructureDefinition Warehouse;
            public StructureFamilyDefinition Family;
        }

        /// <summary>Crée ce qui manque, d'après le contenu de la ville au travail (ses marchandises, ses tâches).</summary>
        public static Made CreateMissing(TownContentBuilder.Made town)
        {
            WorkSlotDefinition worker = Post("Poste_Ouvrier", "ouvrier", town.Build);
            WorkSlotDefinition carter = Post("Poste_Charretier", "charretier", town.Carry, town.Drive);
            StructureDefinition lodge = Building("Loge", "loge", 5f, 4f, 15, 10, worker, 0, town);
            StructureDefinition warehouse = Building("Entrepot", "entrepot", 6f, 5f, 20, 10, carter, 200, town);
            StructureFamilyDefinition family = EditorAssets.CreateIfMissing<StructureFamilyDefinition>(FamilyPath,
                fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = "metiers";
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "famille.metiers";
                    EditorAssets.Property(fiche, "_pastel").intValue = (int)Pastel.Peche;
                    EditorAssets.Property(fiche, "_icon").objectReferenceValue =
                        EditorAssets.Load<VectorImage>(InterfaceIconBuilder.Folder + "/batir.asset");
                    EditorAssets.SetList(EditorAssets.Property(fiche, "_structures"), new[] { lodge, warehouse });
                });
            return new Made { Lodge = lodge, Warehouse = warehouse, Family = family };
        }

        private static WorkSlotDefinition Post(string file, string id, params TaskKindDefinition[] tasks)
        {
            return EditorAssets.CreateIfMissing<WorkSlotDefinition>(
                TownContentBuilder.WorkFolder + "/" + file + ".asset", fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = id;
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "poste." + id;
                    EditorAssets.SetList(EditorAssets.Property(fiche, "_tasks"), tasks);
                });
        }

        // Un bâtiment de service, à bâtir par un chantier : sa taille, son coût, 3 h de travail à un débutant, ses deux
        // postes ; et, s'il en a, les marchandises qu'il tient pour la ville, et alors sa charrette : chaque entrepôt a la
        // sienne (GDD, section 11). Sans modèle du pack, sa boîte.
        private static StructureDefinition Building(string file, string id, float width, float depth, int wood,
            int stone, WorkSlotDefinition post, int capacity, TownContentBuilder.Made town)
        {
            StructureDefinition building = EditorAssets.CreateIfMissing<StructureDefinition>(
                StructuresFolder + "/" + file + ".asset", fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = id;
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "construction." + id;
                    EditorAssets.Property(fiche, "_widthMeters").floatValue = width;
                    EditorAssets.Property(fiche, "_depthMeters").floatValue = depth;
                    TownContentBuilder.SetAmounts(EditorAssets.Property(fiche, "_cost"), town.Wood, wood, town.Stone,
                        stone);
                    EditorAssets.Property(fiche, "_workHours").floatValue = 3f;
                    EditorAssets.SetList(EditorAssets.Property(fiche, "_workSlots"), new[] { post, post });
                    EditorAssets.Property(fiche, "_storageCapacity").intValue = capacity;
                    EditorAssets.Property(fiche, "_carts").intValue = capacity > 0 ? 1 : 0;
                });
            if (building.Model == null)
                ModelPreparer.Prepare(building);
            return building;
        }
    }
}
