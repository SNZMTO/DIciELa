using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le savoir de la ville, par l'ordre « poser » (GDD, sections 6 et 13) : la ville ne bâtit que ce qu'elle sait bâtir,
    /// exactement ce que dit le déroulé du départ ; une construction inconnue est refusée, et l'aperçu le dit par la même
    /// fonction. Sans déroulé du départ (un banc d'essai), la ville sait tout bâtir. Le banc de la ville au travail :
    /// campement de 3 × 3 m, cabane de 4 × 4 m, sur un monde plat de 40 m.
    /// </summary>
    public sealed class KnowledgeTests
    {
        private readonly List<PlacementResult> _refusals = new List<PlacementResult>();
        private TestTown _town;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            _refusals.Clear();
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        private World NewWorld(Catalog catalog = null)
        {
            World world = _town.World(catalog);
            world.Structures.PlacementRefused += (definition, placement, reason) => _refusals.Add(reason);
            return world;
        }

        [Test]
        public void ACabinTheTownDoesNotKnowIsRefusedAndSaysWhy()
        {
            TestData.Knows(_town.Opening, _town.Camp);
            World world = NewWorld();

            TestTown.Place(world, _town.Cabin, 10f, 10f);

            Assert.AreEqual(0, world.Structures.All.Count, "La ville ne sait pas bâtir la cabane.");
            Assert.AreEqual(new[] { PlacementResult.Unknown }, _refusals);
            Assert.IsFalse(world.PlacementRules.IsAvailable(_town.Cabin), "Le panneau ne la propose pas.");
            Assert.AreEqual(PlacementResult.Unknown,
                world.PlacementRules.Check(_town.Cabin, new Placement(new Vector2(30f, 30f), 0f)),
                "L'aperçu le dit, par la même fonction que l'ordre.");
        }

        [Test]
        public void WhatTheOpeningKnowsIsPlaced()
        {
            TestData.Knows(_town.Opening, _town.Camp);
            World world = NewWorld();

            Structure camp = TestTown.Place(world, _town.Camp, 20f, 14f);

            Assert.IsNotNull(camp, "Le campement, la Chimère le sait en arrivant.");
            Assert.IsEmpty(_refusals);
            Assert.IsTrue(world.Knowledge.Knows(_town.Camp));
            Assert.IsFalse(world.Knowledge.Knows(_town.Cabin));
        }

        [Test]
        public void AnOpeningThatListsNothingKnowsNothing()
        {
            TestData.Knows(_town.Opening);
            World world = NewWorld();

            TestTown.Place(world, _town.Camp, 20f, 14f);

            Assert.AreEqual(0, world.Structures.All.Count, "Un déroulé qui ne dit rien : même le campement est inconnu.");
            Assert.AreEqual(new[] { PlacementResult.Unknown }, _refusals);
        }

        [Test]
        public void WithoutAnOpeningTheTownKnowsEverything()
        {
            World world = NewWorld(TestData.EmptyCatalog);

            TestTown.Place(world, _town.Cabin, 10f, 10f);

            Assert.AreEqual(1, world.Structures.All.Count, "Sans déroulé du départ, tout se bâtit.");
            Assert.IsEmpty(_refusals);
        }
    }
}
