using System.Collections.Generic;
using Diciela.Data;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les sols du projet (GDD, sections 9 et 16 ; décisions 209 et 210), lus sans rien écrire : chacun a son identifiant,
    /// une seule fois, et son calque, celui où il se peint ; la ville sait tracer au départ un sol du catalogue, et rien
    /// hors de lui ; sur un sol, une charrette ne va jamais moins vite que dans l'herbe.
    /// </summary>
    public sealed class FloorContentTests
    {
        [Test]
        public void EachFloorHasItsOwnIdAndItsLayer()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            var ids = new HashSet<string>();
            foreach (FloorDefinition floor in catalog.Floors)
            {
                Assert.IsNotNull(floor, "Une case vide dans la liste des sols du catalogue.");
                Assert.IsNotEmpty(floor.Id, floor.name + " n'a pas d'identifiant.");
                Assert.IsTrue(ids.Add(floor.Id), "Deux sols portent l'identifiant " + floor.Id + ".");
                Assert.IsNotNull(floor.Layer, floor.name + " n'a pas de calque : il ne se verrait pas.");
            }
        }

        [Test]
        public void TheTownKnowsAFloorOfTheCatalogAtStart()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            IReadOnlyList<FloorDefinition> known = catalog.Opening.KnownFloors;
            Assert.IsNotEmpty(known, "Au départ, la ville sait tracer la terre (GDD, section 9).");
            foreach (FloorDefinition floor in known)
                CollectionAssert.Contains(catalog.Floors, floor, floor.name + " est connu au départ, hors du catalogue.");
        }

        [Test]
        public void ACartIsNeverSlowerOnAFloorThanOnGrass()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            foreach (FloorDefinition floor in catalog.Floors)
                Assert.GreaterOrEqual(floor.CartSpeedFactor, catalog.Cart.SpeedFactor, floor.name + " ralentit.");
        }
    }
}
