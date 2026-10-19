using System.Collections.Generic;
using Diciela.Data;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le contenu du départ et de la ville au travail, lu dans le projet sans rien écrire : le logis de la Chimère, le
    /// campement (un entrepôt qui reçoit le stock de départ et ouvre un poste), ce que la Chimère sait bâtir en arrivant
    /// (son campement, et rien hors du catalogue), les premiers habitants (un par statistique, chacun son humain), les
    /// statistiques (chaque sous-statistique dans une seule), les tâches (chacune lit une sous-statistique du
    /// catalogue), les coûts (des marchandises du catalogue), les familles (chacune sa case dans la roue de Construire).
    /// </summary>
    public sealed class TownContentTests
    {
        [Test]
        public void TheCatalogHasOneChimeraHomeFirstUniqueWithADoor()
        {
            // Le campement vient en premier (GDD, section 6) : le catalogue a un logis de la Chimère, en tête de sa famille ;
            // il est unique, et il a une porte, devant laquelle elle se couche.
            var homes = new List<StructureDefinition>();
            foreach (StructureFamilyDefinition family in ProjectContent.LoadSingle<Catalog>().Families)
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
            Assert.AreEqual(1, homes.Count, "Le catalogue doit avoir un seul logis de la Chimère : le campement.");
        }

        [Test]
        public void EveryFamilyHasItsPlaceInTheBuildWheel()
        {
            // La roue de Construire a huit cases, dont la case Outils (décision 226) : sept familles au plus.
            Assert.LessOrEqual(ProjectContent.LoadSingle<Catalog>().Families.Count, 7,
                "Une famille de trop n'aurait pas de case dans la roue de Construire.");
        }

        [Test]
        public void TheCampHoldsTheStartingGoodsAndOpensAPost()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            Assert.IsNotNull(catalog.Opening, "Le catalogue n'a pas de déroulé du départ.");
            StructureDefinition camp = ProjectContent.CampOf(catalog);
            Assert.IsNotNull(camp, "Le catalogue n'a pas de campement.");
            Assert.IsTrue(camp.IsUnique, "Le campement s'installe une seule fois.");
            Assert.IsNotEmpty(camp.WorkSlots, "Le campement ouvre le poste « à tout faire ».");

            int total = 0;
            foreach (GoodAmount goods in catalog.Opening.StartingGoods)
            {
                Assert.IsTrue(Contains(catalog.Goods, goods.Good), "Le stock de départ a une marchandise hors du catalogue.");
                Assert.GreaterOrEqual(goods.Amount, 1, "Le stock de départ a une ligne vide.");
                total += goods.Amount * goods.Good.Space;
            }
            Assert.LessOrEqual(total, camp.StorageCapacity, "Le campement n'a pas les places du stock de départ.");
        }

        [Test]
        public void TheChimeraKnowsHerCampAndOnlyStructuresOfTheCatalog()
        {
            // Elle sait installer son campement en arrivant (GDD, section 6), sinon la partie ne commence pas ; et rien
            // qu'elle sache n'est hors du catalogue, où le panneau Construire le prend.
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            IReadOnlyList<StructureDefinition> known = catalog.Opening.KnownStructures;
            CollectionAssert.Contains(known, ProjectContent.CampOf(catalog), "La Chimère ne sait pas son campement.");
            CollectionAssert.AllItemsAreUnique(known, "Le déroulé du départ sait deux fois la même construction.");
            List<StructureDefinition> structures = catalog.AllStructures();
            foreach (StructureDefinition structure in known)
            {
                Assert.IsNotNull(structure, "Le déroulé du départ a une ligne vide.");
                Assert.IsTrue(structures.Contains(structure), structure.Id + " est su, mais hors du catalogue.");
            }
        }

        [Test]
        public void SomethingTheChimeraKnowsOffersBeds()
        {
            // Les premiers habitants n'attendent qu'un lit (GDD, section 6) : ce qu'elle sait bâtir en arrivant en offre.
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            int beds = 0;
            foreach (StructureDefinition structure in catalog.Opening.KnownStructures)
                beds += structure.Beds;
            Assert.Greater(beds, 0, "Rien de ce que la Chimère sait bâtir en arrivant n'offre de lit.");
        }

        [Test]
        public void OneFirstVillagerPerStatisticEachWithAHuman()
        {
            // Un premier habitant par statistique (GDD, section 6), chacun avec un humain du pack (GDD, section 16) ; ils
            // restent en ville comme habitants ordinaires, qui rentrent dormir la nuit (GDD, section 3).
            // Les humains sont les modèles du pack, tels quels : sans le pack (une copie publique du dépôt), le test ne
            // demande pas leur modèle, et vérifie tout le reste.
            bool pack = UnityEditor.AssetDatabase.IsValidFolder("Assets/Synty");
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            Assert.IsNotNull(catalog.VillagerKind, "Le catalogue n'a pas de sorte d'habitant ordinaire.");
            Assert.IsTrue(catalog.VillagerKind.SleepsAtNight, "Les habitants ordinaires rentrent dormir la nuit.");
            var statistics = new List<StatisticDefinition>();
            foreach (FirstVillagerDefinition first in catalog.Opening.FirstVillagers)
            {
                Assert.IsNotNull(first, "Le déroulé du départ a un premier habitant vide.");
                Assert.IsNotNull(first.Character, first.Id + " n'a pas de fiche de personnage.");
                Assert.IsTrue(!pack || first.Character.Model != null, first.Id + " n'a pas son humain.");
                StatisticDefinition statistic = StatisticOf(catalog, first.Strength);
                Assert.IsNotNull(statistic, first.Id + " est fort dans une sous-statistique hors du catalogue.");
                Assert.IsFalse(statistics.Contains(statistic), first.Id + " partage sa statistique avec un autre.");
                statistics.Add(statistic);
            }
            Assert.AreEqual(catalog.Statistics.Count, statistics.Count, "Il faut un premier habitant par statistique.");
        }

        [Test]
        public void EveryStructureIsKnownAtStartOrBroughtByAFirstVillager()
        {
            // Une construction que personne ne sait bâtir, ni la Chimère en arrivant, ni un premier habitant par son idée,
            // ne pourrait jamais se poser (GDD, sections 6 et 13) : un contenu perdu.
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            var reachable = new List<StructureDefinition>(catalog.Opening.KnownStructures);
            foreach (FirstVillagerDefinition first in catalog.Opening.FirstVillagers)
            {
                if (first != null && first.Workplace != null)
                    reachable.Add(first.Workplace);
            }
            var lost = new List<string>();
            foreach (StructureDefinition structure in catalog.AllStructures())
            {
                if (!reachable.Contains(structure))
                    lost.Add(structure.Id);
            }
            Assert.IsEmpty(lost, "Personne ne sait bâtir : " + string.Join(", ", lost) + ".");
        }

        private static StatisticDefinition StatisticOf(Catalog catalog, SubStatisticDefinition subStatistic)
        {
            foreach (StatisticDefinition statistic in catalog.Statistics)
            {
                foreach (SubStatisticDefinition each in statistic.SubStatistics)
                {
                    if (each != null && each == subStatistic)
                        return statistic;
                }
            }
            return null;
        }

        [Test]
        public void EachSubStatisticIsInOneStatisticAndEachTaskReadsOne()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
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
            foreach (StructureDefinition structure in catalog.AllStructures())
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
                    if (cost.Amount < 1)
                        faults.Add(structure.Id + " a une ligne de coût vide.");
                }
            }

            Assert.IsEmpty(faults, string.Join("\n", faults));
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
