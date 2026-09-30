using System.Collections.Generic;
using Diciela.Data;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le contenu du départ et de la ville au travail, lu dans le projet sans rien écrire : le logis de la Chimère, le
    /// tas de départ (hors de la barre, un entrepôt qui tient son contenu de départ et ouvre un poste), les statistiques
    /// (chaque sous-statistique dans une seule), les tâches (chacune lit une sous-statistique du catalogue), les coûts
    /// (des marchandises du catalogue).
    /// </summary>
    public sealed class TownContentTests
    {
        [Test]
        public void TheCatalogHasOneChimeraHomeFirstUniqueWithADoor()
        {
            // La tente vient en premier (GDD, section 6) : le catalogue a un logis de la Chimère, en tête de sa famille ;
            // il est unique, et il a une porte, devant laquelle elle se couche.
            var homes = new List<StructureDefinition>();
            foreach (StructureFamilyDefinition family in ContentTests.LoadSingle<Catalog>().Families)
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

        [Test]
        public void TheStartingPileHoldsItsGoodsAndOpensAPost()
        {
            Catalog catalog = ContentTests.LoadSingle<Catalog>();
            Assert.IsNotNull(catalog.Opening, "Le catalogue n'a pas de déroulé du départ.");
            StructureDefinition pile = catalog.Opening.StartingPile;
            Assert.IsNotNull(pile, "Le déroulé du départ n'a pas de tas.");
            Assert.IsTrue(pile.BuiltWhenPlaced && pile.IsUnique, "Le tas se pose d'un coup, une seule fois.");
            Assert.IsNotEmpty(pile.WorkSlots, "Le tas ouvre le poste « à tout faire ».");

            int total = 0;
            foreach (GoodAmount goods in catalog.Opening.StartingGoods)
            {
                Assert.IsTrue(Contains(catalog.Goods, goods.Good), "Le tas tient une marchandise hors du catalogue.");
                total += goods.Amount;
            }
            Assert.LessOrEqual(total, pile.StorageCapacity, "Le tas ne tient pas son contenu de départ.");
        }

        [Test]
        public void EachSubStatisticIsInOneStatisticAndEachTaskReadsOne()
        {
            Catalog catalog = ContentTests.LoadSingle<Catalog>();
            var seen = new HashSet<SubStatisticDefinition>();
            var faults = new List<string>();
            foreach (StatisticDefinition statistic in catalog.Statistics)
            {
                foreach (SubStatisticDefinition subStatistic in statistic.SubStatistics)
                {
                    if (subStatistic == null || !seen.Add(subStatistic))
                        faults.Add(statistic.Id + " a une sous-statistique vide, ou déjà dans une statistique.");
                }
            }
            foreach (StructureDefinition structure in ContentTests.StructuresOf(catalog))
            {
                foreach (WorkSlotDefinition slot in structure.WorkSlots)
                {
                    foreach (TaskKindDefinition task in slot.Tasks)
                    {
                        if (task == null || !seen.Contains(task.SubStatistic))
                            faults.Add(slot.Id + " : une tâche vide, ou qui lit une sous-statistique hors du catalogue.");
                    }
                }
                foreach (GoodAmount cost in structure.Cost)
                {
                    if (!Contains(catalog.Goods, cost.Good))
                        faults.Add(structure.Id + " coûte une marchandise hors du catalogue.");
                }
            }

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        /// <summary>Les clés des textes de la ville au travail, que le test des textes vérifie avec les autres.</summary>
        internal static void AddTownKeys(Catalog catalog, List<string> keys)
        {
            StructureDefinition pile = ContentTests.StartingPileOf(catalog);
            if (pile != null)
                keys.Add(pile.NameKey);
            foreach (GoodDefinition good in catalog.Goods)
                keys.Add(good.NameKey);
            foreach (StatisticDefinition statistic in catalog.Statistics)
            {
                keys.Add(statistic.NameKey);
                foreach (SubStatisticDefinition subStatistic in statistic.SubStatistics)
                    keys.Add(subStatistic.NameKey);
            }
            foreach (StructureDefinition structure in ContentTests.StructuresOf(catalog))
            {
                foreach (WorkSlotDefinition slot in structure.WorkSlots)
                {
                    keys.Add(slot.NameKey);
                    foreach (TaskKindDefinition task in slot.Tasks)
                        keys.Add(task.NameKey);
                }
            }
        }

        private static bool Contains(IReadOnlyList<GoodDefinition> goods, GoodDefinition good)
        {
            foreach (GoodDefinition each in goods)
            {
                if (each == good)
                    return true;
            }
            return false;
        }
    }
}
