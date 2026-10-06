using System.Collections.Generic;
using Diciela.Data;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La Place du projet (GDD, sections 6 et 11 ; décision 204), lue sans rien écrire : une zone que le joueur trace,
    /// que le catalogue décrit par sa fiche, avec une superficie au plus et des places de charrette au départ, plus à
    /// chaque niveau ; ce n'est plus une construction. Le stock de départ suffit à deux cabanes. La cabane se trouve par
    /// son identifiant.
    /// </summary>
    public sealed class PlaceContentTests
    {
        [Test]
        public void ThePlaceIsAZoneThatGrowsWithItsLevels()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            PlaceDefinition place = catalog.Place;

            Assert.IsNotNull(place, "Le catalogue n'a pas de Place.");
            Assert.Greater(place.MaxAreaAt(0), 0f, "On peut la tracer dès le départ.");
            Assert.Greater(place.CartSpotsAt(0), 0, "Elle a des places de charrette dès le départ.");
            Assert.Greater(place.MaxAreaAt(1), place.MaxAreaAt(0), "Chaque niveau l'agrandit…");
            Assert.Greater(place.CartSpotsAt(1), place.CartSpotsAt(0), "… et lui donne une place de charrette.");
            foreach (StructureDefinition structure in catalog.AllStructures())
                Assert.AreNotEqual("place", structure.Id, "La Place n'est plus une construction.");
        }

        [Test]
        public void ItsCardLeadsAFamilyOfTheBuildPanel()
        {
            // Sa carte, première d'une famille du panneau Construire (GDD, section 16), avec son icône faute de modèle.
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            PlaceDefinition place = catalog.Place;

            Assert.IsNotNull(place.Family, "La fiche de la Place nomme la famille de sa carte.");
            CollectionAssert.Contains(catalog.Families, place.Family, "Cette famille est au catalogue.");
            Assert.IsNotNull(place.Icon, "Sa carte a son icône.");
            Assert.IsNotEmpty(place.NameKey);
        }

        [Test]
        public void TheStartingStockBuildsTwoCabins()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            var needed = new Dictionary<GoodDefinition, int>();
            Need(needed, Find(catalog, "cabane"), 2);

            foreach (KeyValuePair<GoodDefinition, int> goods in needed)
            {
                Assert.GreaterOrEqual(StartingAmountOf(catalog, goods.Key), goods.Value,
                    "Le stock de départ ne suffit pas à deux cabanes en " + goods.Key.Id + ".");
            }
        }

        private static StructureDefinition Find(Catalog catalog, string id)
        {
            foreach (StructureDefinition structure in catalog.AllStructures())
            {
                if (structure.Id == id)
                    return structure;
            }
            Assert.Fail("Le catalogue n'a pas de construction « " + id + " ».");
            return null;
        }

        private static void Need(Dictionary<GoodDefinition, int> needed, StructureDefinition structure, int count)
        {
            foreach (GoodAmount cost in structure.Cost)
            {
                needed.TryGetValue(cost.Good, out int already);
                needed[cost.Good] = already + count * cost.Amount;
            }
        }

        private static int StartingAmountOf(Catalog catalog, GoodDefinition good)
        {
            int amount = 0;
            foreach (GoodAmount goods in catalog.Opening.StartingGoods)
            {
                if (ReferenceEquals(goods.Good, good))
                    amount += goods.Amount;
            }
            return amount;
        }
    }
}
