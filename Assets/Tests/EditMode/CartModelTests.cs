using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le modèle d'une charrette (décision 198), par le chemin du joueur : chaque charrette prend au hasard l'un des
    /// modèles de sa fiche, toujours le même. La fiche du test en a quatre, des objets vides ; le campement garde une
    /// charrette, et un dépôt posé en (8 ; 30), dressé dès la pose, en garde sept.
    /// </summary>
    public sealed class CartModelTests
    {
        private TestTown _town;
        private StructureDefinition _depot;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            var models = new GameObject[4];
            for (int i = 0; i < models.Length; i++)
                models[i] = _town.Keep(EditorUtility.CreateGameObjectWithHideFlags("modèle " + i, HideFlags.HideAndDontSave));
            TestData.UseCart(_town.Catalog, _town.Keep(TestData.Cart(20, 0.5f, 2.2f, models)));
            TestData.Carts(_town.Camp, 1, 0);
            _depot = _town.KeepKnown(TestData.Structure("depot", 4f, 4f));
            TestData.BuiltWhenPlaced(_depot);
            TestData.Carts(_depot, 7, 0);
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void EachCartTakesOneOfTheModelsOfItsFicheAlwaysTheSame()
        {
            World world = _town.WorkingWorld();
            TestTown.Place(world, _depot, 8f, 30f);
            World again = _town.WorkingWorld();
            TestTown.Place(again, _depot, 8f, 30f);

            Assert.AreEqual(8, world.Carts.All.Count, "Une charrette au campement, sept au dépôt.");
            var drawn = new HashSet<int>();
            for (int i = 0; i < world.Carts.All.Count; i++)
            {
                Cart cart = world.Carts.All[i];
                Assert.That(cart.ModelIndex, Is.InRange(0, 3), "L'un des quatre modèles de sa fiche.");
                Assert.AreEqual(cart.ModelIndex, again.Carts.All[i].ModelIndex, "La même charrette, le même modèle.");
                drawn.Add(cart.ModelIndex);
            }
            Assert.Greater(drawn.Count, 1, "Au hasard : elles ne prennent pas toutes le même.");
        }
    }
}
