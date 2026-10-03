using System.Collections.Generic;
using Diciela.Data;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La Place du projet (GDD, sections 6, 9 et 11 ; décisions 192 et 193), lue sans rien écrire : un sol qu'on
    /// traverse, où arriveront les visiteurs, unique, sans porte, que la Chimère sait bâtir en arrivant, composée d'objets
    /// du pack ; le stock de départ suffit à la Place et à deux cabanes. La Place et la cabane se trouvent par leur
    /// identifiant.
    /// </summary>
    public sealed class PlaceContentTests
    {
        [Test]
        public void ThePlaceIsAUniqueDoorlessFloorTheChimeraKnows()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            StructureDefinition place = Find(catalog, "place");

            Assert.IsTrue(place.IsWalkable, "On marche sur la Place : les visiteurs y arrivent.");
            Assert.IsTrue(place.IsUnique, "Une seule Place dans la ville.");
            Assert.IsFalse(place.HasDoor, "Un sol n'a pas de porte.");
            CollectionAssert.Contains(catalog.Opening.KnownStructures, place, "La Chimère sait bâtir la Place en arrivant.");
            Assert.IsNotNull(place.Model, "La Place n'a pas de modèle : on ne la verrait pas.");
            Assert.IsNotNull(place.SourceModel, "La Place est composée d'objets du pack (DICIELA/Contenu/Composer les modèles).");
        }

        [Test]
        public void TheStartingStockBuildsThePlaceAndTwoCabins()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            var needed = new Dictionary<GoodDefinition, int>();
            Need(needed, Find(catalog, "place"), 1);
            Need(needed, Find(catalog, "cabane"), 2);

            foreach (KeyValuePair<GoodDefinition, int> goods in needed)
            {
                Assert.GreaterOrEqual(StartingAmountOf(catalog, goods.Key), goods.Value,
                    "Le stock de départ ne suffit pas à la Place et à deux cabanes en " + goods.Key.Id + ".");
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
