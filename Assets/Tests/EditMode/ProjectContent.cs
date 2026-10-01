using System.Collections.Generic;
using Diciela.Data;
using NUnit.Framework;
using UnityEditor;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le contenu du projet, tel que le lisent les tests de contenu, sans rien écrire : la seule fiche d'un type, les
    /// constructions que mène le catalogue, et les clés de textes de la ville au travail.
    /// </summary>
    internal static class ProjectContent
    {
        /// <summary>La seule fiche de ce type dans le projet ; il y en a une, et une seule.</summary>
        public static T LoadSingle<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            Assert.AreEqual(1, guids.Length, "Il faut une seule fiche " + typeof(T).Name + " dans le projet.");
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        /// <summary>
        /// Le campement du catalogue : le logis de la Chimère qui s'installe d'un coup (GDD, section 6) ; null s'il n'en a
        /// pas.
        /// </summary>
        public static StructureDefinition CampOf(Catalog catalog)
        {
            foreach (StructureDefinition structure in catalog.AllStructures())
            {
                if (structure.IsChimeraHome && structure.BuiltWhenPlaced)
                    return structure;
            }
            return null;
        }

        /// <summary>
        /// Les clés des textes de la ville au travail : les marchandises, les statistiques, les postes, les arbres et les
        /// rochers.
        /// </summary>
        public static void AddTownKeys(Catalog catalog, List<string> keys)
        {
            foreach (GoodDefinition good in catalog.Goods)
                keys.Add(good.NameKey);
            foreach (NatureDefinition nature in catalog.Nature)
                keys.Add(nature.NameKey);
            foreach (StatisticDefinition statistic in catalog.Statistics)
            {
                keys.Add(statistic.NameKey);
                foreach (SubStatisticDefinition subStatistic in statistic.SubStatistics)
                    keys.Add(subStatistic.NameKey);
            }
            foreach (StructureDefinition structure in catalog.AllStructures())
            {
                foreach (WorkSlotDefinition slot in structure.WorkSlots)
                {
                    keys.Add(slot.NameKey);
                    foreach (TaskKindDefinition task in slot.Tasks)
                        keys.Add(task.NameKey);
                }
            }
        }
    }
}
