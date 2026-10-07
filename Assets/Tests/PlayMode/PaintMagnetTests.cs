using Diciela.Controls;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using static Diciela.Tests.PlayMode.TestPaint;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'aimant des places de charrette (décision 220), sur le petit monde des tests, dont la Place a 100 m² au plus et
    /// deux places de charrette, par les clics du joueur : près de la place libre à côté d'une place posée, la nouvelle
    /// s'y pose, côte à côte, à l'écart réglé, au cap de l'autre et non à celui de l'outil ; Ctrl tenu, elle se pose où
    /// l'on clique ; loin de toute place, aussi. La Place a une zone de 12 × 8 m au milieu du monde.
    /// </summary>
    public sealed class PaintMagnetTests
    {
        private TestWorld _test;
        private PaintTool _plaza;

        [SetUp]
        public void CreatePlaza()
        {
            _test = new TestWorld(withPlaza: true);
            _test.PaintPlaza(BoxAt(0f, 0f, 12f, 8f));
            _plaza = _test.Plaza;
            _plaza.Take();
            _plaza.Choose(PaintToolMode.CartSpot);
        }

        [TearDown]
        public void Destroy()
        {
            _test.Dispose();
        }

        [Test]
        public void ASpotNearAnotherSnapsBesideIt()
        {
            PlazaMark placed = Lay(-2f, 0f, 0f);
            _test.ClickAt(_plaza, 1.2f, 0.5f);
            PlazaMark snapped = Newest();

            float apart = placed.Footprint.Size.x + _test.Settings.CartSpotGapMeters;
            Assert.AreEqual(placed.Footprint.Center.x + apart, snapped.Footprint.Center.x, 0.01f,
                "À l'est de l'autre, côte à côte, l'écart réglé entre elles…");
            Assert.AreEqual(placed.Footprint.Center.y, snapped.Footprint.Center.y, 0.01f, "… l'avant aligné.");
            Assert.AreEqual(0f, snapped.HeadingDegrees, 0.01f);
        }

        [Test]
        public void ItTakesTheHeadingOfThePlacedSpotNotTheTools()
        {
            // Couchée vers l'est, la place posée a sa place libre au nord, à 2,8 m + l'écart de son milieu.
            PlazaMark placed = Lay(0f, -2f, 90f);
            float apart = placed.Footprint.Size.x + _test.Settings.CartSpotGapMeters;
            _test.ClickAt(_plaza, 0.4f, -2f + apart + 0.3f);
            PlazaMark snapped = Newest();

            Assert.AreEqual(90f, snapped.HeadingDegrees, 0.01f, "Le cap de la place posée, pas celui de l'outil (0°).");
            Assert.AreEqual(0f, Vector2.Distance(GroundAt(0f, -2f + apart), snapped.Footprint.Center), 0.01f);
        }

        [Test]
        public void CtrlHeldLaysItWhereTheClickIs()
        {
            Lay(-2f, 0f, 0f);
            _test.ClickAt(_plaza, 1.2f, 0.5f, magnet: false);
            PlazaMark free = Newest();

            Assert.AreEqual(0f, Vector2.Distance(GroundAt(1.2f, 0.5f), free.Footprint.Center), 0.01f, "Sans aimant.");
        }

        [Test]
        public void FarFromAnySpotNothingSnaps()
        {
            Lay(-2f, 0f, 0f);
            _plaza.RotateBy(15f);
            _test.ClickAt(_plaza, 3.5f, 0f);
            PlazaMark free = Newest();

            Assert.AreEqual(0f, Vector2.Distance(GroundAt(3.5f, 0f), free.Footprint.Center), 0.01f);
            Assert.AreEqual(15f, free.HeadingDegrees, 0.01f, "Le cap de l'outil.");
        }

        // Une place posée par son ordre, à ce point et à ce cap, jouée tout de suite.
        private PlazaMark Lay(float east, float north, float headingDegrees)
        {
            _test.World.Submit(new AddCartSpotCommand(GroundAt(east, north), headingDegrees));
            _test.World.Step();
            Assert.AreEqual(1, _test.World.Plaza.Marks.Count, "La place posée par son ordre.");
            return _test.World.Plaza.Marks[0];
        }

        // La place que le dernier clic a posée, une fois l'ordre joué.
        private PlazaMark Newest()
        {
            _test.World.Step();
            Assert.AreEqual(2, _test.World.Plaza.Marks.Count, "Le clic a posé une place.");
            return _test.World.Plaza.Marks[1];
        }
    }
}
