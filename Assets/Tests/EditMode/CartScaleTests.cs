using System.Diagnostics;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le trajet d'une charrette à l'échelle du jeu (500 m à quatre mailles par mètre ; décision 213) : une route pavée de
    /// 80 m, bâtie par la Chimère. L'estimation sait où sont les sols : loin de la route, la recherche file droit comme
    /// celle d'un piéton ; le long d'elle, elle fouille un peu plus. Un trajet de 40 m (d'un entrepôt à la Place) reste
    /// sous le budget de 5 ms ; un de 100 m, sous le double, une borne large pour qu'une machine plus lente ne la fasse pas
    /// mentir. Les temps sont rendus dans le message du test.
    /// </summary>
    public sealed class CartScaleTests
    {
        private const double BudgetMilliseconds = 5d;
        private const int Searches = 5;
        private readonly Path _path = new Path();

        [Test]
        public void ACartTripStaysWithinBudget()
        {
            var town = new TestTown(cellsPerMeter: 4);
            try
            {
                TestData.KnowsFloors(town.Opening, town.Dirt, town.Paving);
                TestData.FloorHours(town.Settings, 0.0001f);
                World world = town.World(null, null, 500f);
                Structure camp = TestTown.Place(world, town.Camp, 250f, 244f);
                TestTown.Send(world, new AssignVillagerCommand(world.Villagers.Chimera.Id, camp.Id));
                var road = new GroundBox(new RotatedRect(new Vector2(240f, 260f), new Vector2(80f, 3f), 0f));
                TestTown.Send(world, new TraceFloorCommand(road, town.Paving, FloorScope.Anywhere));
                TestGrids.StepUntil(world, () => world.ConstructionSites.Queue.Count == 0, 5000, "la route bâtie");

                // La première recherche d'une charrette après la route bâtie refait la distance de chaque bloc aux sols.
                Stopwatch first = Stopwatch.StartNew();
                Assert.IsTrue(world.PathFinder.TryFindPath(new[] { Cell(190f, 250f) }, new[] { Cell(290f, 265f) }, _path,
                    Mover.Cart));
                double refreshed = first.Elapsed.TotalMilliseconds;
                double alongTheRoad = Fastest(world, Cell(190f, 250f), Cell(290f, 265f), Mover.Cart);
                double farFromIt = Fastest(world, Cell(150f, 100f), Cell(250f, 130f), Mover.Cart);
                double onFoot = Fastest(world, Cell(150f, 100f), Cell(250f, 130f), Mover.OnFoot);
                double alongTheRoad40 = Fastest(world, Cell(215f, 255f), Cell(254f, 264f), Mover.Cart);
                double farFromIt40 = Fastest(world, Cell(150f, 100f), Cell(188f, 110f), Mover.Cart);

                string times = "100 m : charrette le long de la route " + alongTheRoad.ToString("0.00") + " ms, loin d'elle " +
                               farFromIt.ToString("0.00") + " ms, à pied " + onFoot.ToString("0.00") + " ms ; 40 m : " +
                               "le long de la route " + alongTheRoad40.ToString("0.00") + " ms, loin d'elle " +
                               farFromIt40.ToString("0.00") + " ms ; la première après la route bâtie " +
                               refreshed.ToString("0.00") + " ms.";
                Assert.LessOrEqual(alongTheRoad, 2d * BudgetMilliseconds, times);
                Assert.LessOrEqual(farFromIt, 2d * BudgetMilliseconds, times);
                Assert.LessOrEqual(alongTheRoad40, BudgetMilliseconds, times);
                Assert.LessOrEqual(farFromIt40, BudgetMilliseconds, times);
                Assert.Pass(times);
            }
            finally
            {
                town.Dispose();
            }
        }

        // Le plus court de quelques trajets de ce départ à cette arrivée, les îlots déjà faits par un premier.
        private double Fastest(World world, Vector2Int from, Vector2Int to, Mover mover)
        {
            var starts = new[] { from };
            var goals = new[] { to };
            Assert.IsTrue(world.PathFinder.TryFindPath(starts, goals, _path, mover));
            double fastest = double.MaxValue;
            for (int i = 0; i < Searches; i++)
            {
                Stopwatch watch = Stopwatch.StartNew();
                world.PathFinder.TryFindPath(starts, goals, _path, mover);
                fastest = System.Math.Min(fastest, watch.Elapsed.TotalMilliseconds);
            }
            return fastest;
        }

        private static Vector2Int Cell(float x, float z)
        {
            return new Vector2Int(Mathf.FloorToInt(x * 4f), Mathf.FloorToInt(z * 4f));
        }
    }
}
