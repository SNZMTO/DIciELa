using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les trajets d'une charrette contre l'oracle (SAVOIR, 4) : 40 terrains tirés au hasard (graine fixe), de 16 m aux
    /// mailles de 50 cm, avec le campement, des arbres, et des rectangles de terre et de pavés que la Chimère bâtit. Les
    /// prix sont écrits ici à la main, l'inverse du pas : l'herbe 24 (moitié moins vite), la terre 12, les pavés 8 (une
    /// fois et demie plus vite). L'escalier trouvé pour la charrette est juste pas à pas et coûte ce que coûte le moins
    /// cher selon le Dijkstra ; sa ligne tendue ne touche que des mailles libres, et n'est jamais plus lente que
    /// l'escalier : la ficelle ne coupe pas l'herbe entre deux bouts de chemin.
    /// </summary>
    public sealed class CartPathOracleTests
    {
        private const int Grounds = 40;
        private const int Seed = 20261007;
        private const float Size = 16f;
        private const int Grass = 24;
        private const int DirtPrice = 12;
        private const int PavingPrice = 8;

        private readonly Path _path = new Path();
        private TestTown _town;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            TestData.KnowsFloors(_town.Opening, _town.Dirt, _town.Paving);
            TestData.FloorHours(_town.Settings, 0.0001f);
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void TheCartTakesTheCheapestWayAndItsTautLineIsNeverSlower()
        {
            var random = new System.Random(Seed);
            int reachable = 0, onFloors = 0;
            for (int n = 0; n < Grounds; n++)
            {
                World world = RandomWorld(random, out List<Vector2Int> starts, out List<Vector2Int> goals);
                bool[,] blocked = TestGrids.Blocked(world.Grid);
                int[,] prices = Prices(world);
                int expected = SlowPaths.CheapestCost(blocked, starts, goals, prices);
                bool found = world.PathFinder.TryFindPath(starts, goals, _path, Mover.Cart);
                string where = "Terrain " + n;

                if (expected == SlowPaths.NoPath)
                {
                    Assert.IsFalse(found, where);
                    continue;
                }
                Assert.IsTrue(found, where);
                string fault = SlowPaths.Judge(blocked, starts, goals, _path.Cells, out int cost, prices);
                Assert.IsNull(fault, where + " : " + fault);
                Assert.AreEqual(expected, cost, where);
                fault = SlowLines.Judge(blocked, TestChimera.CellsPerMeter, _path);
                Assert.IsNull(fault, where + " : " + fault);
                float stair = TimeAlong(world, prices, Centers(world, _path.Cells));
                float line = TimeAlong(world, prices, _path.Corners);
                Assert.LessOrEqual(line, stair * 1.001f + 0.01f, where + " : la ficelle est plus lente que l'escalier.");
                reachable++;
                if (Crosses(world, _path.Cells))
                    onFloors++;
            }

            Assert.GreaterOrEqual(reachable, 20, "Trop peu de terrains avec un trajet.");
            Assert.GreaterOrEqual(onFloors, 10, "Trop peu de trajets qui passent sur un sol.");
        }

        // Un monde de 16 m : des arbres au nord (la Chimère naît au centre, libre), le campement en (8 ; 3), la Chimère à
        // son poste, deux à cinq rectangles de terre ou de pavés bâtis ; un à trois départs et une à trois arrivées, libres
        // ou non.
        private World RandomWorld(System.Random random, out List<Vector2Int> starts, out List<Vector2Int> goals)
        {
            var trees = new List<NatureSpot>();
            int treeCount = random.Next(0, 30);
            for (int i = 0; i < treeCount; i++)
            {
                var at = new Vector2(1f + (float)random.NextDouble() * 14f, 10f + (float)random.NextDouble() * 5f);
                trees.Add(new NatureSpot(_town.Tree, new Placement(at, 0f)));
            }
            World world = _town.World(null, trees, Size);
            Structure camp = TestTown.Place(world, _town.Camp, 8f, 3f);
            TestTown.Send(world, new AssignVillagerCommand(world.Villagers.Chimera.Id, camp.Id));
            int floorCount = random.Next(2, 6);
            for (int i = 0; i < floorCount; i++)
            {
                var center = new Vector2((float)random.NextDouble() * Size, (float)random.NextDouble() * Size);
                var size = new Vector2(1f + (float)random.NextDouble() * 7f, 1f + (float)random.NextDouble() * 7f);
                FloorDefinition floor = random.Next(2) == 0 ? _town.Dirt : _town.Paving;
                var box = new GroundBox(new RotatedRect(center, size, (float)random.NextDouble() * 90f));
                TestTown.Send(world, new TraceFloorCommand(box, floor, FloorScope.Anywhere));
            }
            // Un sol que personne ne rejoint reste en fantôme : l'oracle lit ce qui est bâti.
            for (int step = 0; step < 2000 && world.ConstructionSites.Queue.Count > 0; step++)
                world.Step();

            int cells = world.Grid.Width;
            starts = RandomCells(random, cells);
            goals = RandomCells(random, cells);
            return world;
        }

        private static List<Vector2Int> RandomCells(System.Random random, int size)
        {
            int count = random.Next(1, 4);
            var cells = new List<Vector2Int>(count);
            for (int i = 0; i < count; i++)
                cells.Add(new Vector2Int(random.Next(size), random.Next(size)));
            return cells;
        }

        // Le prix de chaque maille pour la charrette, d'après le sol qui y sert ; un fantôme ne compte pas.
        private int[,] Prices(World world)
        {
            var prices = new int[world.Grid.Width, world.Grid.Depth];
            for (int x = 0; x < world.Grid.Width; x++)
            {
                for (int y = 0; y < world.Grid.Depth; y++)
                    prices[x, y] = PriceAt(world, new Vector2Int(x, y));
            }
            return prices;
        }

        private int PriceAt(World world, Vector2Int cell)
        {
            FloorState state = world.Floors.StateAt(cell);
            if (state != FloorState.Built && state != FloorState.Erasing)
                return Grass;
            return world.Floors.FloorAt(cell) == _town.Paving ? PavingPrice : DirtPrice;
        }

        private bool Crosses(World world, IReadOnlyList<Vector2Int> cells)
        {
            foreach (Vector2Int cell in cells)
            {
                if (PriceAt(world, cell) != Grass)
                    return true;
            }
            return false;
        }

        private static List<Vector2> Centers(World world, IReadOnlyList<Vector2Int> cells)
        {
            var centers = new List<Vector2>(cells.Count);
            foreach (Vector2Int cell in cells)
                centers.Add(world.Grid.CellCenter(cell));
            return centers;
        }

        // Le temps de parcours d'une ligne brisée, au prix de la maille sous chaque centimètre : un juge lent, à part.
        private static float TimeAlong(World world, int[,] prices, IReadOnlyList<Vector2> points)
        {
            float time = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                Vector2 from = points[i - 1];
                Vector2 to = points[i];
                float length = Vector2.Distance(from, to);
                int samples = Mathf.Max(1, Mathf.CeilToInt(length / 0.01f));
                for (int s = 0; s < samples; s++)
                {
                    Vector2Int cell = world.Grid.CellOf(Vector2.Lerp(from, to, (s + 0.5f) / samples));
                    time += length / samples * prices[cell.x, cell.y];
                }
            }
            return time;
        }
    }
}
