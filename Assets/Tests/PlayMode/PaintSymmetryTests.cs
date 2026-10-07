using Diciela.Controls;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using static Diciela.Tests.PlayMode.TestPaint;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La symétrie des outils de sol (décision 219), sur le petit monde des tests, par les gestes du joueur : l'étoile
    /// peint ses copies autour de l'épingle, glissée au milieu du monde ; le miroir peint de l'autre côté de son axe ;
    /// l'épingle de la Place part du milieu de sa zone ; R tourne les axes, sauf au mode Place de charrette, où il tourne
    /// la place ; une place de charrette se pose avec sa copie, son cap retourné. Le monde ne joue ses pas que quand le
    /// test le dit.
    /// </summary>
    public sealed class PaintSymmetryTests
    {
        private TestWorld _test;

        [TearDown]
        public void Destroy()
        {
            _test?.Dispose();
        }

        [Test]
        public void AStarPaintsItsCopiesAroundThePin()
        {
            _test = new TestWorld(withPlaza: true, withFloor: true);
            PaintTool paths = _test.Paths;
            paths.Take();
            paths.Choose(PaintToolMode.Brush);
            paths.Options.Choose(PaintSymmetry.Star);
            paths.Options.ChooseStar(4);
            DragPin(paths, 0f, 0f);
            Assert.AreEqual(0f, Vector2.Distance(GroundAt(0f, 0f), paths.Options.PinCenter), 0.01f,
                "L'épingle glissée suit le pointeur.");

            _test.ClickAt(paths, 6f, 0f);
            _test.World.Step();
            Assert.AreEqual(FloorState.Planned, _test.FloorAt(6f, 0f), "Le rond du pinceau…");
            Assert.AreEqual(FloorState.Planned, _test.FloorAt(0f, -6f), "… et ses copies, d'un quart de tour en quart de tour…");
            Assert.AreEqual(FloorState.Planned, _test.FloorAt(-6f, 0f));
            Assert.AreEqual(FloorState.Planned, _test.FloorAt(0f, 6f));
            Assert.AreEqual(FloorState.None, _test.FloorAt(4.2f, 4.2f), "… et rien entre elles.");
        }

        [Test]
        public void AMirrorPaintsTheOtherSideOfItsAxis()
        {
            _test = new TestWorld(withPlaza: true, withFloor: true);
            PaintTool paths = _test.Paths;
            paths.Take();
            paths.Choose(PaintToolMode.Brush);
            paths.Options.Choose(PaintSymmetry.Mirror);
            DragPin(paths, 0f, 0f);
            paths.RotateBy(-paths.Options.AxisDegrees);
            Assert.AreEqual(0f, paths.Options.AxisDegrees, 0.001f, "R tourne l'axe, ici jusqu'au nord.");

            _test.ClickAt(paths, 4f, 3f);
            _test.World.Step();
            Assert.AreEqual(FloorState.Planned, _test.FloorAt(4f, 3f), "Le rond du pinceau…");
            Assert.AreEqual(FloorState.Planned, _test.FloorAt(-4f, 3f), "… et son reflet, de l'autre côté de l'axe nord-sud.");
            Assert.AreEqual(FloorState.None, _test.FloorAt(4f, -3f), "Rien au sud : l'axe ne retourne que l'est et l'ouest.");
            Assert.AreEqual(FloorState.None, _test.FloorAt(-4f, -3f));
        }

        [Test]
        public void ThePinOfThePlazaStartsInTheMiddleOfItsZone()
        {
            _test = new TestWorld(withPlaza: true);
            _test.PaintPlaza(BoxAt(5f, -3f, 6f, 4f));
            PaintTool plaza = _test.Plaza;
            plaza.Take();
            Assert.IsFalse(plaza.Options.HasPin, "Sans symétrie, pas d'épingle.");
            plaza.Options.Choose(PaintSymmetry.Mirror);
            Assert.IsTrue(plaza.Options.HasPin);
            Assert.AreEqual(0f, Vector2.Distance(GroundAt(5f, -3f), plaza.Options.PinCenter), 0.01f, "Au milieu de la Place.");
        }

        [Test]
        public void RTurnsTheAxesExceptForACartSpotWhichItTurns()
        {
            _test = new TestWorld(withPlaza: true);
            PaintTool plaza = _test.Plaza;
            plaza.Take();
            plaza.Choose(PaintToolMode.Brush);
            plaza.Options.Choose(PaintSymmetry.Mirror);
            float axis = plaza.Options.AxisDegrees;
            plaza.RotateBy(15f);
            Assert.AreEqual(Mathf.Repeat(axis + 15f, 360f), plaza.Options.AxisDegrees, 0.001f,
                "Hors des places de charrette, R tourne l'axe…");
            Assert.AreEqual(0f, plaza.Options.HeadingDegrees, "… et pas la place.");

            plaza.Choose(PaintToolMode.CartSpot);
            plaza.RotateBy(15f);
            Assert.AreEqual(15f, plaza.Options.HeadingDegrees, 0.001f, "Au mode Place de charrette, R tourne la place…");
            Assert.AreEqual(Mathf.Repeat(axis + 15f, 360f), plaza.Options.AxisDegrees, 0.001f, "… et pas l'axe.");
        }

        [Test]
        public void ACartSpotIsLaidWithItsMirroredCopy()
        {
            _test = new TestWorld(withPlaza: true);
            _test.PaintPlaza(BoxAt(0f, 0f, 12f, 8f));
            PaintTool plaza = _test.Plaza;
            plaza.Take();
            plaza.Choose(PaintToolMode.Brush);
            plaza.Options.Choose(PaintSymmetry.Mirror);
            plaza.RotateBy(-plaza.Options.AxisDegrees);
            plaza.Choose(PaintToolMode.CartSpot);
            plaza.RotateBy(30f);

            _test.ClickAt(plaza, 3.2f, 0f);
            _test.World.Step();
            Assert.AreEqual(2, _test.World.Plaza.Marks.Count, "La place et sa copie.");
            PlazaMark first = _test.World.Plaza.Marks[0];
            PlazaMark second = _test.World.Plaza.Marks[1];
            PlazaMark east = first.Footprint.Center.x > second.Footprint.Center.x ? first : second;
            PlazaMark west = east == first ? second : first;
            Assert.AreEqual(0f, Vector2.Distance(GroundAt(3.2f, 0f), east.Footprint.Center), 0.01f);
            Assert.AreEqual(30f, east.HeadingDegrees, 0.01f);
            Assert.AreEqual(0f, Vector2.Distance(GroundAt(-3.2f, 0f), west.Footprint.Center), 0.01f,
                "La copie, de l'autre côté de l'axe nord-sud…");
            Assert.AreEqual(330f, west.HeadingDegrees, 0.01f, "… son cap retourné.");
        }

        // Glisse l'épingle jusqu'à ce point du sol, comme le joueur : appuyer dessus, glisser, lâcher.
        private void DragPin(PaintTool tool, float east, float north)
        {
            Vector2 pin = tool.Options.PinCenter;
            tool.BeginDrawing(_test.OnScreen(new Vector3(pin.x, TestWorld.Center.y, pin.y)));
            tool.AimAt(_test.OnScreenAt(east, north));
            tool.EndDrawing();
        }
    }
}
