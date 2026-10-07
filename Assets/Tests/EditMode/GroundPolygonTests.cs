using System;
using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using static Diciela.Tests.EditMode.TestFloors;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le polygone peint au sol (décision 216), par l'ordre du joueur « tracer un sol », sur le monde plat et nu de 40 m
    /// aux mailles de 50 cm : il couvre les mailles dont le milieu est dedans, selon la règle pair-impair. L'oracle compte
    /// à la main, maille à maille, par le rayon qui part vers l'est et les bords qu'il croise ; des polygones tirés au
    /// hasard (graine fixe), même croisés, et un carré, qui couvre ce que couvre le rectangle de mêmes coins.
    /// </summary>
    public sealed class GroundPolygonTests
    {
        private const int RandomPolygons = 12;
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
        public void ASquarePolygonCoversWhatTheBoxOfTheSameCornersCovers()
        {
            World polygon = _town.World();
            Trace(polygon, new GroundPolygon(new[]
            {
                new Vector2(15.2f, 16.1f), new Vector2(24.8f, 16.1f), new Vector2(24.8f, 23.7f), new Vector2(15.2f, 23.7f)
            }), _town.Dirt);
            World box = _town.World();
            Trace(box, Box(20f, 19.9f, 9.6f, 7.6f), _town.Dirt);
            CollectionAssert.AreEquivalent(PlannedOf(box), PlannedOf(polygon));
        }

        [Test]
        public void ItCoversTheCellsWhoseMiddleIsInsideEvenWhenItCrossesItself()
        {
            var random = new System.Random(216);
            for (int n = 0; n < RandomPolygons; n++)
            {
                var corners = new Vector2[3 + random.Next(6)];
                for (int i = 0; i < corners.Length; i++)
                    corners[i] = new Vector2(8f + 24f * (float)random.NextDouble(), 8f + 24f * (float)random.NextDouble());
                World world = _town.World();
                Trace(world, new GroundPolygon(corners), _town.Dirt);
                CollectionAssert.AreEquivalent(Inside(world, corners), PlannedOf(world), "Le polygone n° " + n + ".");
            }
        }

        [Test]
        public void APolygonHasThreeCornersAtLeast()
        {
            Assert.Throws<ArgumentException>(() => new GroundPolygon(new[] { Vector2.zero, Vector2.one }));
        }

        // L'oracle : les mailles dont le milieu a, à sa droite, un nombre impair de bords.
        private static List<Vector2Int> Inside(World world, IReadOnlyList<Vector2> corners)
        {
            var inside = new List<Vector2Int>();
            for (int x = 0; x < world.Grid.Width; x++)
            {
                for (int y = 0; y < world.Grid.Depth; y++)
                {
                    var cell = new Vector2Int(x, y);
                    Vector2 middle = world.Grid.CellCenter(cell);
                    bool odd = false;
                    for (int i = 0, j = corners.Count - 1; i < corners.Count; j = i++)
                    {
                        Vector2 a = corners[i], b = corners[j];
                        if ((a.y > middle.y) != (b.y > middle.y) &&
                            middle.x < a.x + (middle.y - a.y) * (b.x - a.x) / (b.y - a.y))
                            odd = !odd;
                    }
                    if (odd)
                        inside.Add(cell);
                }
            }
            return inside;
        }
    }
}
