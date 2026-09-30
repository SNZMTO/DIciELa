using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEngine.UIElements;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée le contenu de la ville au travail qui manque (GDD, sections 5 à 7 ; décisions 137 à 139) : le Bois et la
    /// Pierre, les quatre statistiques et leurs sous-statistiques, les tâches « porter » et « bâtir », le poste « à tout
    /// faire », le tas de départ et le déroulé du départ, le coût de la cabane, et leur place dans le catalogue.
    /// Réexécutable : une fiche qui existe n'est jamais touchée, sauf pour ce qui lui manque (une liste vide, un
    /// nombre à 0).
    /// </summary>
    internal static class TownContentBuilder
    {
        private const string GoodsFolder = "Assets/Data/Marchandises";
        private const string StatisticsFolder = "Assets/Data/Statistiques";
        private const string WorkFolder = "Assets/Data/Travail";
        internal const string PilePath = "Assets/Data/Structures/TasDeDepart.asset";
        private const string OpeningPath = "Assets/Data/Depart.asset";

        // Les statistiques et leurs sous-statistiques (GDD, section 5) : l'identifiant de la statistique, puis ceux de
        // ses sous-statistiques ; l'Artisanat en a une par famille d'atelier.
        private static readonly string[][] Statistics =
        {
            new[] { "terre", "culture", "coupe", "extraction", "paysage" },
            new[] { "batisse", "construction", "terrassement", "amenagement" },
            new[] { "artisanat", "travail_bois", "travail_fil", "travail_pierre" },
            new[] { "echange", "transport", "troc", "expedition" }
        };

        public static void CreateMissing(Catalog catalog, StructureDefinition cabin)
        {
            GoodDefinition wood = Good("Bois", "bois");
            GoodDefinition stone = Good("Pierre", "pierre");
            var statistics = new List<StatisticDefinition>();
            var subStatistics = new Dictionary<string, SubStatisticDefinition>();
            foreach (string[] ids in Statistics)
                statistics.Add(Statistic(ids, subStatistics));

            TaskKindDefinition carry = Task("Tache_Porter", "porter", TaskWork.Carry, subStatistics["transport"]);
            TaskKindDefinition build = Task("Tache_Batir", "batir", TaskWork.Build, subStatistics["construction"]);
            // Récolter et défricher s'ajouteront avec les arbres et les champs sauvages (décision 139).
            WorkSlotDefinition oddJobs = EditorAssets.CreateIfMissing<WorkSlotDefinition>(
                WorkFolder + "/Poste_AToutFaire.asset", fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = "a_tout_faire";
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "poste.a_tout_faire";
                    EditorAssets.SetList(EditorAssets.Property(fiche, "_tasks"), new[] { carry, build });
                });

            StructureDefinition pile = EditorAssets.CreateIfMissing<StructureDefinition>(PilePath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "tas_de_depart";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "construction.tas_de_depart";
                EditorAssets.Property(fiche, "_widthMeters").floatValue = 2f;
                EditorAssets.Property(fiche, "_depthMeters").floatValue = 2f;
                EditorAssets.Property(fiche, "_hasDoor").boolValue = false;
                EditorAssets.Property(fiche, "_unique").boolValue = true;
                EditorAssets.Property(fiche, "_builtWhenPlaced").boolValue = true;
                EditorAssets.Property(fiche, "_storageCapacity").intValue = 50;
                EditorAssets.SetList(EditorAssets.Property(fiche, "_workSlots"), new[] { oddJobs });
            });
            // Le pack a des tas de bûches et de pierres ; le choix est au développeur : une boîte en attendant.
            if (pile.Model == null)
                ModelPreparer.Prepare(pile);

            OpeningDefinition opening = EditorAssets.CreateIfMissing<OpeningDefinition>(OpeningPath, fiche =>
            {
                EditorAssets.Property(fiche, "_startingPile").objectReferenceValue = pile;
                SetAmounts(EditorAssets.Property(fiche, "_startingGoods"), wood, 30, stone, 20);
            });

            // Une cabane coûte 10 Bois et 5 Pierre, et demande 2 h de travail à un débutant (décision 137).
            if (cabin.Cost.Count == 0)
                EditorAssets.Set(cabin, fiche => SetAmounts(EditorAssets.Property(fiche, "_cost"), wood, 10, stone, 5));
            if (cabin.WorkHours <= 0f)
                EditorAssets.Set(cabin, fiche => EditorAssets.Property(fiche, "_workHours").floatValue = 2f);

            EditorAssets.AssignIfMissing(catalog, "_opening", opening);
            if (catalog.Goods.Count == 0)
                EditorAssets.Set(catalog, fiche =>
                    EditorAssets.SetList(EditorAssets.Property(fiche, "_goods"), new[] { wood, stone }));
            if (catalog.Statistics.Count == 0)
                EditorAssets.Set(catalog, fiche =>
                    EditorAssets.SetList(EditorAssets.Property(fiche, "_statistics"), statistics));
        }

        private static GoodDefinition Good(string file, string id)
        {
            return EditorAssets.CreateIfMissing<GoodDefinition>(GoodsFolder + "/" + file + ".asset", fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = id;
                EditorAssets.Property(fiche, "_nameKey").stringValue = "marchandise." + id;
                EditorAssets.Property(fiche, "_icon").objectReferenceValue = Icon(id);
            });
        }

        private static StatisticDefinition Statistic(string[] ids, Dictionary<string, SubStatisticDefinition> made)
        {
            var subStatistics = new List<SubStatisticDefinition>();
            for (int i = 1; i < ids.Length; i++)
            {
                string id = ids[i];
                SubStatisticDefinition subStatistic = EditorAssets.CreateIfMissing<SubStatisticDefinition>(
                    StatisticsFolder + "/Sous_" + id + ".asset", fiche =>
                    {
                        EditorAssets.Property(fiche, "_id").stringValue = id;
                        EditorAssets.Property(fiche, "_nameKey").stringValue = "sous-statistique." + id;
                    });
                made[id] = subStatistic;
                subStatistics.Add(subStatistic);
            }
            return EditorAssets.CreateIfMissing<StatisticDefinition>(StatisticsFolder + "/" + ids[0] + ".asset",
                fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = ids[0];
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "statistique." + ids[0];
                    EditorAssets.SetList(EditorAssets.Property(fiche, "_subStatistics"), subStatistics);
                });
        }

        private static TaskKindDefinition Task(string file, string id, TaskWork work, SubStatisticDefinition reads)
        {
            return EditorAssets.CreateIfMissing<TaskKindDefinition>(WorkFolder + "/" + file + ".asset", fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = id;
                EditorAssets.Property(fiche, "_nameKey").stringValue = "tache." + id;
                EditorAssets.Property(fiche, "_icon").objectReferenceValue = Icon(id);
                EditorAssets.Property(fiche, "_work").enumValueIndex = (int)work;
                EditorAssets.Property(fiche, "_subStatistic").objectReferenceValue = reads;
            });
        }

        private static VectorImage Icon(string name)
        {
            return EditorAssets.Load<VectorImage>(InterfaceIconBuilder.Folder + "/" + name + ".asset");
        }

        /// <summary>Remplit une liste de quantités : une marchandise, son nombre, une autre, son nombre…</summary>
        private static void SetAmounts(SerializedProperty list, GoodDefinition first, int firstAmount,
            GoodDefinition second, int secondAmount)
        {
            list.arraySize = 2;
            SetAmount(list.GetArrayElementAtIndex(0), first, firstAmount);
            SetAmount(list.GetArrayElementAtIndex(1), second, secondAmount);
        }

        private static void SetAmount(SerializedProperty item, GoodDefinition good, int amount)
        {
            item.FindPropertyRelative("_good").objectReferenceValue = good;
            item.FindPropertyRelative("_amount").intValue = amount;
        }
    }
}
