using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le trajet d'une charrette (GDD, section 9 ; décision 209), sur le monde plat de 40 m aux mailles de 50 cm : une
    /// route pavée en U, de (6 ; 22) à (34 ; 22) par le nord (44 m), contre 28 m d'herbe tout droit. Une charrette va
    /// moitié moins vite dans l'herbe et une fois et demie plus vite sur les pavés : elle prend la route (29 contre 56) ;
    /// un piéton, qui va partout à la même vitesse, va tout droit. Une route pas encore bâtie n'accélère personne. Les
    /// routes se tracent par l'ordre du joueur, et la Chimère, au poste « à tout faire », les bâtit.
    /// </summary>
    public sealed class CartPathTests
    {
        private static readonly Vector2Int From = new Vector2Int(12, 44);
        private static readonly Vector2Int To = new Vector2Int(68, 44);
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
        public void TheCartTakesThePavedRoadAndThePedestrianGoesStraight()
        {
            World world = _town.WorkingWorld();
            TraceTheRoad(world);
            TestGrids.StepUntil(world, () => world.ConstructionSites.Queue.Count == 0, 3000, "la route bâtie");

            Assert.IsTrue(world.PathFinder.TryFindPath(new[] { From }, new[] { To }, _path));
            Assert.AreEqual(22f, Northmost(), 0.3f, "Le piéton va tout droit.");
            Assert.IsTrue(world.PathFinder.TryFindPath(new[] { From }, new[] { To }, _path, Mover.Cart));
            // Tendue sur la route de 29 à 31 m, la ficelle en longe le bord intérieur.
            Assert.GreaterOrEqual(Northmost(), 29f, "La charrette prend la route, par le nord.");
            foreach (Vector2Int cell in _path.Cells)
                Assert.AreEqual(FloorState.Built, world.Floors.StateAt(cell), "Elle reste sur les pavés : " + cell);
        }

        [Test]
        public void AGhostRoadSpeedsNobody()
        {
            // Sans poste, la Chimère ne bâtit rien : la route reste en fantôme.
            World world = _town.World();
            TraceTheRoad(world);
            Vector2Int onTheRoad = world.Grid.CellOf(new Vector2(20f, 30f));

            Assert.AreEqual(FloorState.Planned, world.Floors.StateAt(onTheRoad));
            Assert.AreEqual(0.5f, world.CartPace.SpeedAt(onTheRoad), "Le pas de l'herbe.");
            Assert.IsTrue(world.PathFinder.TryFindPath(new[] { From }, new[] { To }, _path, Mover.Cart));
            Assert.AreEqual(22f, Northmost(), 0.3f, "La charrette va tout droit, comme dans l'herbe.");
        }

        [Test]
        public void ACartGoesAtThePaceOfTheFloorItIsOn()
        {
            World world = _town.WorkingWorld();
            TestTown.Send(world, new TraceFloorCommand(Box(10f, 34f), _town.Dirt, FloorScope.Anywhere));
            TestTown.Send(world, new TraceFloorCommand(Box(30f, 34f), _town.Paving, FloorScope.Anywhere));
            TestGrids.StepUntil(world, () => world.ConstructionSites.Queue.Count == 0, 3000, "les sols bâtis");

            Assert.AreEqual(0.5f, world.CartPace.SpeedAt(world.Grid.CellOf(new Vector2(20f, 34f))), "L'herbe.");
            Assert.AreEqual(1f, world.CartPace.SpeedAt(world.Grid.CellOf(new Vector2(10f, 34f))), "La terre.");
            Assert.AreEqual(1.5f, world.CartPace.SpeedAt(world.Grid.CellOf(new Vector2(30f, 34f))), "Les pavés.");
        }

        // La route pavée en U, de 2 m de large : de (6 ; 22) au nord jusqu'à (6 ; 30), à l'est jusqu'à (34 ; 30), puis au sud.
        private void TraceTheRoad(World world)
        {
            var road = new GroundStroke(new[]
            {
                new Vector2(6f, 22f), new Vector2(6f, 30f), new Vector2(34f, 30f), new Vector2(34f, 22f)
            }, 1f);
            TestTown.Send(world, new TraceFloorCommand(road, _town.Paving, FloorScope.Anywhere));
        }

        private static GroundBox Box(float x, float z)
        {
            return new GroundBox(new RotatedRect(new Vector2(x, z), new Vector2(4f, 4f), 0f));
        }

        // Le point le plus au nord de la ligne tendue du trajet.
        private float Northmost()
        {
            float north = float.MinValue;
            IReadOnlyList<Vector2> corners = _path.Corners;
            for (int i = 0; i < corners.Count; i++)
                north = Mathf.Max(north, corners[i].y);
            return north;
        }
    }
}
