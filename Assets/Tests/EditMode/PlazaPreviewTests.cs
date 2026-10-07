using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Ce que l'aperçu de l'outil de la Place lit (décision 206), sur le monde plat de 40 m aux mailles de 50 cm : la même
    /// fonction que l'ordre dit quelles mailles il ajouterait ou retirerait, et ce sont celles qu'il change ; la Place
    /// donne toutes ses mailles, pour son voile. Un trait sinueux couvre, chacune une fois, les mailles dont le milieu est
    /// à moins de sa demi-largeur de sa ligne.
    /// </summary>
    public sealed class PlazaPreviewTests
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
        public void TheCheckListsTheCellsTheOrderChanges()
        {
            World world = _town.World();
            var disc = new GroundDisc(new Vector2(20f, 20f), 5f);
            var change = new List<Vector2Int>();
            Assert.AreEqual(PlazaResult.Ok, world.Plaza.CheckPaint(disc, change));
            TestTown.Send(world, new PaintPlazaCommand(disc));

            var cells = new List<Vector2Int>();
            world.Plaza.CopyCells(cells);
            CollectionAssert.AreEquivalent(change, cells, "Peint, la Place a les mailles que l'aperçu montrait.");
            Assert.AreEqual(world.Plaza.CellCount, cells.Count);

            var eraser = new GroundStroke(new[] { new Vector2(16f, 20f), new Vector2(20f, 20f) }, 1f);
            Assert.AreEqual(PlazaResult.Ok, world.Plaza.CheckErase(eraser, change));
            int before = world.Plaza.CellCount;
            TestTown.Send(world, new ErasePlazaCommand(eraser));
            Assert.AreEqual(before - change.Count, world.Plaza.CellCount, "Gommée, elle perd ce que l'aperçu montrait.");
            foreach (Vector2Int cell in change)
                Assert.IsFalse(world.Plaza.Contains(cell));

            Assert.AreEqual(PlazaResult.Nothing, world.Plaza.CheckPaint(new GroundDisc(new Vector2(20f, 23f), 1f), change),
                "Sur la Place déjà peinte, rien ne change…");
            Assert.IsEmpty(change, "… et l'aperçu ne montre rien.");
        }

        [Test]
        public void ASinuousStrokeCoversEachCellOnce()
        {
            World world = _town.World();
            var points = new List<Vector2>();
            for (int i = 0; i <= 40; i++)
                points.Add(new Vector2(8f + i * 0.6f, 20f + 4f * Mathf.Sin(i * 0.35f)));
            const float HalfWidth = 1.2f;
            var change = new List<Vector2Int>();
            world.Plaza.CheckPaint(new GroundStroke(points, HalfWidth), change);

            var expected = new HashSet<Vector2Int>();
            for (int y = 0; y < world.Grid.Depth; y++)
            {
                for (int x = 0; x < world.Grid.Width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (Distance(world.Grid.CellCenter(cell), points) <= HalfWidth)
                        expected.Add(cell);
                }
            }
            Assert.AreEqual(expected.Count, change.Count, "Chaque maille une fois.");
            CollectionAssert.AreEquivalent(expected, change);
        }

        private static float Distance(Vector2 point, List<Vector2> line)
        {
            float best = float.MaxValue;
            for (int i = 1; i < line.Count; i++)
            {
                Vector2 a = line[i - 1], segment = line[i] - a;
                float along = Mathf.Clamp01(Vector2.Dot(point - a, segment) / segment.sqrMagnitude);
                best = Mathf.Min(best, (point - (a + segment * along)).magnitude);
            }
            return best;
        }
    }
}
