using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les gestes pointés des outils de sol (décision 216), sur le petit monde des tests, appelés comme leurs gestes les
    /// appellent : un tracé pose ses points d'un clic chacun, et part au clic sur son dernier point, en segments droits ou
    /// en courbe ; un point posé se déplace en le glissant ; un polygone se ferme au clic sur son premier coin. Le monde
    /// ne joue ses pas que quand le test le dit : rien ne part avant.
    /// </summary>
    public sealed class PaintPointsTests
    {
        private TestWorld _test;

        [TearDown]
        public void Destroy()
        {
            _test?.Dispose();
        }

        [Test]
        public void ATraceLaysItsStraightSegmentsWhenItsLastPointIsClicked()
        {
            _test = new TestWorld(withPlaza: true, withFloor: true);
            PaintTool paths = _test.Paths;
            paths.Take();
            paths.Choose(PaintToolMode.Trace);
            Click(paths, -6f, 0f);
            Click(paths, 0f, 0f);
            Click(paths, 0f, 6f);
            _test.World.Step();
            Assert.IsEmpty(_test.World.ConstructionSites.Queue, "Tant qu'on pose des points, rien ne part.");
            Assert.IsTrue(paths.IsDrawing);

            Click(paths, 0f, 6f);
            _test.World.Step();
            Assert.AreEqual(FloorState.Planned, StateAt(-3f, 0f), "Le premier segment…");
            Assert.AreEqual(FloorState.Planned, StateAt(0f, 3f), "… et le second…");
            Assert.AreEqual(FloorState.None, StateAt(-3f, 3f), "… droits : rien dans le coin d'en face.");
            Assert.IsFalse(paths.IsDrawing, "Parti, le tracé est fini.");
        }

        [Test]
        public void AHeldPointFollowsThePointerBeforeTheTraceLeaves()
        {
            _test = new TestWorld(withPlaza: true, withFloor: true);
            PaintTool paths = _test.Paths;
            paths.Take();
            paths.Choose(PaintToolMode.Trace);
            Click(paths, -6f, 0f);
            Click(paths, 0f, 0f);
            paths.BeginDrawing(OnScreen(-6f, 0f));
            paths.AimAt(OnScreen(-6f, 4f));
            paths.EndDrawing();
            Click(paths, 0f, 0f);
            _test.World.Step();
            Assert.AreEqual(FloorState.Planned, StateAt(-3f, 2f), "Le segment part du point déplacé…");
            Assert.AreEqual(FloorState.None, StateAt(-3f, 0f), "… et non plus de là où il était.");
        }

        [Test]
        public void ACurvedTraceIsNotTheStraightOneAndKeepsItsPoints()
        {
            List<Vector2Int> straight = Traced(PaintTraceStyle.Straight);
            List<Vector2Int> curved = Traced(PaintTraceStyle.Curved);
            CollectionAssert.AreNotEquivalent(straight, curved, "La courbe n'est pas la ligne brisée.");
            foreach (float[] point in new[] { new[] { -6f, 0f }, new[] { 0f, 4f }, new[] { 6f, 0f } })
                CollectionAssert.Contains(curved, CellAt(point[0], point[1]), "Elle passe par ses points.");
        }

        [Test]
        public void APolygonClosesOnItsFirstCornerAndPaintsTheZoneInside()
        {
            _test = new TestWorld(withPlaza: true);
            PaintTool plaza = _test.Plaza;
            plaza.Take();
            plaza.Choose(PaintToolMode.Shape);
            plaza.Options.Choose(PaintShape.Polygon);
            Click(plaza, -3f, -3f);
            Click(plaza, 3f, -3f);
            Click(plaza, 3f, 3f);
            Click(plaza, -3f, 3f);
            _test.World.Step();
            Assert.AreEqual(0, _test.World.Plaza.CellCount, "Ouvert, il ne part pas.");
            Click(plaza, -3f, -3f);
            _test.World.Step();
            Assert.IsTrue(_test.World.Plaza.Contains(CellAt(0f, 0f)), "Fermé, la zone est dedans…");
            Assert.IsFalse(_test.World.Plaza.Contains(CellAt(4f, 0f)), "… et pas dehors.");
        }

        // Le même tracé, de trois points, droit ou en courbe, dans un monde neuf : les mailles qu'il pose.
        private List<Vector2Int> Traced(PaintTraceStyle style)
        {
            _test?.Dispose();
            _test = new TestWorld(withPlaza: true, withFloor: true);
            PaintTool paths = _test.Paths;
            paths.Take();
            paths.Choose(PaintToolMode.Trace);
            paths.Options.Choose(style);
            Click(paths, -6f, 0f);
            Click(paths, 0f, 4f);
            Click(paths, 6f, 0f);
            Click(paths, 6f, 0f);
            _test.World.Step();
            var cells = new List<Vector2Int>();
            for (int x = 0; x < _test.World.Grid.Width; x++)
            {
                for (int y = 0; y < _test.World.Grid.Depth; y++)
                {
                    if (_test.World.Floors.StateAt(new Vector2Int(x, y)) == FloorState.Planned)
                        cells.Add(new Vector2Int(x, y));
                }
            }
            return cells;
        }

        private Vector2 OnScreen(float east, float north)
        {
            return _test.OnScreen(TestWorld.Center + new Vector3(east, 0f, north));
        }

        private Vector2Int CellAt(float east, float north)
        {
            Vector3 point = TestWorld.Center + new Vector3(east, 0f, north);
            return _test.World.Grid.CellOf(new Vector2(point.x, point.z));
        }

        // Un clic du joueur à ce point du sol, à tant de mètres à l'est et au nord du milieu du monde.
        private void Click(PaintTool tool, float east, float north)
        {
            tool.AimAt(OnScreen(east, north));
            tool.Click();
        }

        private FloorState StateAt(float east, float north = 0f)
        {
            return _test.World.Floors.StateAt(CellAt(east, north));
        }
    }
}
