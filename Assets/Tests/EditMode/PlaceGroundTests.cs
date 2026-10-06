using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La Place sur le sol de la ville (GDD, sections 9 et 11 ; décisions 204 et 205), par les ordres du joueur : ce qui
    /// se tient là (une construction, un arbre) l'arrête, et reste lui-même ; rien ne se bâtit sur elle ; elle peut
    /// s'étendre devant une porte, et une porte peut s'ouvrir sur elle ; on la traverse tout droit. La ville au travail :
    /// le campement de 3 m en (20 ; 14), porte au nord ; la cabane fait 4 × 4 m.
    /// </summary>
    public sealed class PlaceGroundTests
    {
        private TestTown _town;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void WhatStandsThereStopsTheBrushAndAPlaceMayLieBeforeADoor()
        {
            World world = _town.WorkingWorld(new[]
            {
                new NatureSpot(_town.Tree, new Placement(new Vector2(16f, 17f), 0f))
            });
            Structure camp = TestTown.CampOf(world);
            TestTown.Send(world, new PaintPlaceCommand(new GroundDisc(new Vector2(20f, 15f), 5f)));

            Assert.Greater(world.Place.CellCount, 0, "La Place est tracée autour.");
            Assert.AreEqual(camp.Id, OccupantAt(world, new Vector2(20f, 14f)), "Le campement reste le campement.");
            Assert.Less(OccupantAt(world, new Vector2(16f, 17f)), 0, "L'arbre reste l'arbre.");
            Assert.IsTrue(world.Place.Contains(world.Grid.CellOf(new Vector2(20f, 16.2f))),
                "Elle s'étend devant la porte du campement, qu'on traverse toujours.");
            Assert.Less(world.Place.AreaSquareMeters, Mathf.PI * 25f - 6f, "Le campement et l'arbre y font des trous.");
        }

        [Test]
        public void NothingIsBuiltOnThePlace()
        {
            World world = _town.WorkingWorld();
            var refused = new List<PlacementResult>();
            world.Structures.PlacementRefused += (definition, placement, result) => refused.Add(result);
            TestTown.Send(world, new PaintPlaceCommand(new GroundDisc(new Vector2(20f, 28f), 5f)));
            TestTown.Send(world, new PlaceStructureCommand(_town.Cabin, new Placement(new Vector2(22f, 28f), 0f)));

            CollectionAssert.AreEqual(new[] { PlacementResult.OnThePlace }, refused, "On la pave et la décore.");
        }

        [Test]
        public void ADoorMayOpenOntoThePlace()
        {
            // La cabane en (20 ; 22,5) : sa façade nord à 24,5 m, le devant de sa porte jusqu'à 26 m, sur la Place.
            World world = _town.WorkingWorld();
            TestTown.Send(world, new PaintPlaceCommand(new GroundDisc(new Vector2(20f, 29f), 4f)));
            Structure cabin = TestTown.Place(world, _town.Cabin, 20f, 22.5f);

            Assert.IsNotNull(cabin, "Sa porte s'ouvre sur la Place.");
            Assert.IsTrue(world.Place.Contains(world.Grid.CellOf(new Vector2(20f, 25.5f))), "Le devant de sa porte.");
        }

        [Test]
        public void PeopleCrossThePlaceStraight()
        {
            World world = _town.WorkingWorld();
            TestTown.Send(world, new PaintPlaceCommand(new GroundDisc(new Vector2(20f, 28f), 5f)));
            Vector2Int from = world.Grid.CellOf(new Vector2(14f, 28f));
            Vector2Int to = world.Grid.CellOf(new Vector2(26f, 28f));
            var path = new Path();

            Assert.IsTrue(world.PathFinder.TryFindPath(new[] { from }, new[] { to }, path), "On traverse la Place.");
            Assert.AreEqual(to.x - from.x + 1, path.Cells.Count, "Tout droit.");
            foreach (Vector2Int cell in path.Cells)
                Assert.AreEqual(from.y, cell.y, "Sur sa rangée.");
        }

        private static int OccupantAt(World world, Vector2 point)
        {
            return world.Grid.OccupantAt(world.Grid.CellOf(point));
        }
    }
}
