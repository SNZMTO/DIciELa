using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEngine.UIElements;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée le contenu de la ville au travail qui manque (GDD, sections 2 et 5 à 7 ; décisions 137 à 139 et 157 à 160) :
    /// le Bois et la Pierre, les quatre statistiques et leurs sous-statistiques, les tâches « porter », « bâtir » et
    /// « défricher », le poste « à tout faire », l'arbre et le rocher (sans modèle : le développeur y range ceux qu'il
    /// peint). Le campement, la cabane, le déroulé du départ et le catalogue naissent avec eux (ContentBuilder).
    /// Réexécutable : une fiche qui existe n'est jamais touchée, un réglage fait à la main jamais remis.
    /// </summary>
    internal static class TownContentBuilder
    {
        /// <summary>Le contenu de la ville au travail, tel qu'il est sur le disque, fait ou trouvé.</summary>
        internal sealed class Made
        {
            public GoodDefinition Wood;
            public GoodDefinition Stone;
            public WorkSlotDefinition OddJobs;
            public TaskKindDefinition Carry;
            public TaskKindDefinition Build;
            public NatureDefinition[] Nature;
            public List<StatisticDefinition> Statistics;
        }

        private const string GoodsFolder = "Assets/Data/Marchandises";
        private const string StatisticsFolder = "Assets/Data/Statistiques";
        internal const string WorkFolder = "Assets/Data/Travail";
        private const string NatureFolder = "Assets/Data/Nature";

        // Les statistiques et leurs sous-statistiques (GDD, section 5) : l'identifiant de la statistique, puis ceux de
        // ses sous-statistiques ; l'Artisanat en a une par famille d'atelier.
        private static readonly string[][] Statistics =
        {
            new[] { "terre", "culture", "coupe", "extraction", "paysage" },
            new[] { "batisse", "construction", "terrassement", "amenagement" },
            new[] { "artisanat", "travail_bois", "travail_fil", "travail_pierre" },
            new[] { "echange", "transport", "troc", "expedition" }
        };

        public static Made CreateMissing()
        {
            GoodDefinition wood = Good("Bois", "bois");
            GoodDefinition stone = Good("Pierre", "pierre");
            var statistics = new List<StatisticDefinition>();
            var subStatistics = new Dictionary<string, SubStatisticDefinition>();
            foreach (string[] ids in Statistics)
                statistics.Add(Statistic(ids, subStatistics));

            TaskKindDefinition carry = Task("Tache_Porter", "porter", TaskWork.Carry, subStatistics["transport"]);
            TaskKindDefinition build = Task("Tache_Batir", "batir", TaskWork.Build, subStatistics["construction"]);
            TaskKindDefinition clear = Task("Tache_Defricher", "defricher", TaskWork.Clear, subStatistics["paysage"]);
            // Récolter s'ajoutera avec les champs sauvages, entre bâtir et défricher (décision 139).
            WorkSlotDefinition oddJobs = EditorAssets.CreateIfMissing<WorkSlotDefinition>(
                WorkFolder + "/Poste_AToutFaire.asset", fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = "a_tout_faire";
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "poste.a_tout_faire";
                    EditorAssets.SetList(EditorAssets.Property(fiche, "_tasks"), new[] { carry, build, clear });
                });
            // Un arbre rend 3 Bois pour 30 min de travail, un rocher 2 Pierre pour 45 min (décision 160).
            NatureDefinition tree = Nature("Arbre", "arbre", 1f, wood, 3, 0.5f);
            NatureDefinition rock = Nature("Rocher", "rocher", 1.5f, stone, 2, 0.75f);
            return new Made
            {
                Wood = wood, Stone = stone, OddJobs = oddJobs, Carry = carry, Build = build, Nature = new[] { tree, rock },
                Statistics = statistics
            };
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

        private static NatureDefinition Nature(string file, string id, float size, GoodDefinition good, int amount,
            float workHours)
        {
            return EditorAssets.CreateIfMissing<NatureDefinition>(NatureFolder + "/" + file + ".asset", fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = id;
                EditorAssets.Property(fiche, "_nameKey").stringValue = "nature." + id;
                EditorAssets.Property(fiche, "_widthMeters").floatValue = size;
                EditorAssets.Property(fiche, "_depthMeters").floatValue = size;
                SetAmount(EditorAssets.Property(fiche, "_yield"), good, amount);
                EditorAssets.Property(fiche, "_workHours").floatValue = workHours;
            });
        }

        private static VectorImage Icon(string name)
        {
            return EditorAssets.Load<VectorImage>(InterfaceIconBuilder.Folder + "/" + name + ".asset");
        }

        /// <summary>Remplit une liste de quantités : une marchandise, son nombre, une autre, son nombre…</summary>
        internal static void SetAmounts(SerializedProperty list, GoodDefinition first, int firstAmount,
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
