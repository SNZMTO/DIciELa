using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La fabrique de fiches des tests, suite : les fiches de la ville au travail (marchandises, statistiques, tâches,
    /// postes, déroulé du départ). Même règle : en mémoire, remplies par SerializedObject.
    /// </summary>
    internal static partial class TestData
    {
        private static Catalog _emptyCatalog;

        /// <summary>
        /// Un catalogue vide : un monde sans déroulé du départ ni statistiques, pour les tests qui n'en ont pas besoin. Il ne
        /// contient rien et n'est jamais modifié : un seul pour tous les tests, jamais enregistré.
        /// </summary>
        public static Catalog EmptyCatalog
        {
            get
            {
                if (_emptyCatalog == null)
                {
                    _emptyCatalog = ScriptableObject.CreateInstance<Catalog>();
                    _emptyCatalog.hideFlags = HideFlags.HideAndDontSave;
                }
                return _emptyCatalog;
            }
        }

        public static GoodDefinition Good(string id)
        {
            GoodDefinition good = ScriptableObject.CreateInstance<GoodDefinition>();
            var serialized = new SerializedObject(good);
            Property(serialized, "_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return good;
        }

        public static SubStatisticDefinition SubStatistic(string id)
        {
            SubStatisticDefinition subStatistic = ScriptableObject.CreateInstance<SubStatisticDefinition>();
            var serialized = new SerializedObject(subStatistic);
            Property(serialized, "_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return subStatistic;
        }

        public static StatisticDefinition Statistic(string id, params SubStatisticDefinition[] subStatistics)
        {
            StatisticDefinition statistic = ScriptableObject.CreateInstance<StatisticDefinition>();
            var serialized = new SerializedObject(statistic);
            Property(serialized, "_id").stringValue = id;
            Fill(Property(serialized, "_subStatistics"), subStatistics);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return statistic;
        }

        public static TaskKindDefinition TaskKind(string id, TaskWork work, SubStatisticDefinition subStatistic)
        {
            TaskKindDefinition task = ScriptableObject.CreateInstance<TaskKindDefinition>();
            var serialized = new SerializedObject(task);
            Property(serialized, "_id").stringValue = id;
            Property(serialized, "_work").enumValueIndex = (int)work;
            Property(serialized, "_subStatistic").objectReferenceValue = subStatistic;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return task;
        }

        /// <summary>
        /// Une sorte d'arbre ou de rocher dont le pied prend size mètres de côté, qui rend ceci une fois retirée et demande
        /// tant d'heures de travail à un débutant.
        /// </summary>
        public static NatureDefinition Nature(string id, float size, GoodAmount yield, float workHours)
        {
            NatureDefinition nature = ScriptableObject.CreateInstance<NatureDefinition>();
            var serialized = new SerializedObject(nature);
            Property(serialized, "_id").stringValue = id;
            Property(serialized, "_widthMeters").floatValue = size;
            Property(serialized, "_depthMeters").floatValue = size;
            SerializedProperty amount = Property(serialized, "_yield");
            amount.FindPropertyRelative("_good").objectReferenceValue = yield.Good;
            amount.FindPropertyRelative("_amount").intValue = yield.Amount;
            Property(serialized, "_workHours").floatValue = workHours;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return nature;
        }

        /// <summary>Un poste dont les tâches se choisissent dans cet ordre.</summary>
        public static WorkSlotDefinition WorkSlot(string id, params TaskKindDefinition[] tasks)
        {
            WorkSlotDefinition slot = ScriptableObject.CreateInstance<WorkSlotDefinition>();
            var serialized = new SerializedObject(slot);
            Property(serialized, "_id").stringValue = id;
            Fill(Property(serialized, "_tasks"), tasks);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return slot;
        }

        /// <summary>Donne à cette construction son coût et les heures de travail que son chantier demande à un débutant.</summary>
        public static void Cost(StructureDefinition definition, float workHours, params GoodAmount[] cost)
        {
            var serialized = new SerializedObject(definition);
            Property(serialized, "_workHours").floatValue = workHours;
            FillAmounts(Property(serialized, "_cost"), cost);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Le déroulé du départ : le stock de départ, que reçoit le premier logis de la Chimère.</summary>
        public static OpeningDefinition Opening(params GoodAmount[] goods)
        {
            OpeningDefinition opening = ScriptableObject.CreateInstance<OpeningDefinition>();
            var serialized = new SerializedObject(opening);
            FillAmounts(Property(serialized, "_startingGoods"), goods);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return opening;
        }

        /// <summary>Un catalogue qui n'a que ce déroulé du départ, ces marchandises et ces statistiques.</summary>
        public static Catalog TownCatalog(OpeningDefinition opening, GoodDefinition[] goods,
            StatisticDefinition[] statistics)
        {
            Catalog catalog = ScriptableObject.CreateInstance<Catalog>();
            var serialized = new SerializedObject(catalog);
            Property(serialized, "_opening").objectReferenceValue = opening;
            Fill(Property(serialized, "_goods"), goods);
            Fill(Property(serialized, "_statistics"), statistics);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }

        private static void FillAmounts(SerializedProperty list, GoodAmount[] amounts)
        {
            list.arraySize = amounts.Length;
            for (int i = 0; i < amounts.Length; i++)
            {
                SerializedProperty item = list.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("_good").objectReferenceValue = amounts[i].Good;
                item.FindPropertyRelative("_amount").intValue = amounts[i].Amount;
            }
        }
    }
}
