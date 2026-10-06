using System.Collections;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les gestes de l'outil de la Place (décision 206), joués par une souris et un clavier simulés sur les vraies actions
    /// du jeu et le petit monde des tests, dont la Place a 100 m² au plus : un glisser trace un disque, que le relâcher
    /// envoie, et qui ne part pas s'il dépasse la superficie ; le pinceau peint un trait, la gomme le reprend ; un clic
    /// pose une place de charrette, un clic sur elle la retire ; R la tourne ; Ctrl + molette grandit le pinceau ; un clic
    /// droit bref abandonne le geste en cours, puis pose l'outil de côté. Un faux panneau d'interface occupe le bas de
    /// l'écran. Le monde ne joue ses pas que quand le test le dit : rien ne part avant.
    /// </summary>
    public sealed class PlaceControlsTests : GestureFixture
    {
        private TestWorld _test;
        private PlaceControls _controls;

        public override void Setup()
        {
            base.Setup();
            _test = new TestWorld(withPlace: true);
            var gestures = _test.Keep(ScriptableObject.CreateInstance<GestureSettings>());
            Assert.That(Moved, Is.GreaterThan(gestures.DragThresholdPixels * 2f));

            GameObject holder = _test.Keep(new GameObject("Gestes de la Place"));
            holder.SetActive(false);
            _controls = holder.AddComponent<PlaceControls>();
#if UNITY_EDITOR
            TestWorld.Fill(_controls, "_actions", _test.Keep(TestActions.CopyOfTheGame()));
            TestWorld.Fill(_controls, "_settings", _test.Settings);
            TestWorld.Fill(_controls, "_gestures", gestures);
#endif
            holder.SetActive(true);
            _controls.Bind(_test.Place, new TestInterface(new Rect(0f, 0f, Screen.width, Screen.height * 0.2f)));
            _test.Place.Take();
        }

        public override void TearDown()
        {
            _controls.enabled = false;
            _test.Dispose();
            base.TearDown();
        }

        private PlaceTool Tool => _test.Place;
        private PlaceZone Zone => _test.World.Place;

        // Le point de l'écran où l'on voit le sol, à tant de mètres à l'est et au nord du milieu du monde.
        private Vector2 Ground(float east, float north)
        {
            return _test.OnScreen(TestWorld.Center + new Vector3(east, 0f, north));
        }

        private IEnumerator PointAt(Vector2 screenPoint)
        {
            Move(Mouse.position, screenPoint);
            yield return Played();
        }

        // Un glisser du bouton gauche, de point en point.
        private IEnumerator Drag(params Vector2[] points)
        {
            yield return PointAt(points[0]);
            Press(Mouse.leftButton);
            yield return Played();
            for (int i = 1; i < points.Length; i++)
                yield return PointAt(points[i]);
            Release(Mouse.leftButton);
            yield return Played();
        }

        [UnityTest]
        public IEnumerator ADragTracesADiscSentAtTheRelease()
        {
            Tool.Choose(PlaceToolMode.Disc);
            yield return PointAt(Ground(0f, 0f));
            Press(Mouse.leftButton);
            yield return Played();
            yield return PointAt(Ground(4f, 0f));
            Assert.IsTrue(Tool.IsDrawing, "On trace pendant qu'on glisse.");
            Assert.AreEqual(PlaceResult.Ok, Tool.Result, "L'aperçu dit qu'il se trace.");
            Release(Mouse.leftButton);
            yield return Played();
            Assert.IsFalse(Tool.IsDrawing);
            _test.World.Step();

            Assert.AreEqual(Mathf.PI * 16f, Zone.AreaSquareMeters, 3f, "Un disque de 4 m de rayon, à la maille près.");
            Assert.IsTrue(Zone.Contains(_test.World.Grid.CellOf(new Vector2(50f, 50f))));
        }

        [UnityTest]
        public IEnumerator AShapeTooLargeForThePlaceIsNotSent()
        {
            Tool.Choose(PlaceToolMode.Disc);
            yield return PointAt(Ground(0f, 0f));
            Press(Mouse.leftButton);
            yield return Played();
            yield return PointAt(Ground(6f, 0f));
            Assert.AreEqual(PlaceResult.TooLarge, Tool.Result, "113 m², au-delà des 100 de la Place : rouge.");
            Release(Mouse.leftButton);
            yield return Played();
            _test.World.Step();

            Assert.AreEqual(0, Zone.CellCount, "Refusée par l'aperçu, la forme ne part pas.");
        }

        [UnityTest]
        public IEnumerator TheBrushPaintsAStrokeAndTheEraserTakesItBack()
        {
            Tool.Choose(PlaceToolMode.Brush);
            yield return Drag(Ground(-3f, 0f), Ground(-1f, 0f), Ground(1f, 0f), Ground(3f, 0f));
            _test.World.Step();
            float painted = Zone.AreaSquareMeters;
            Assert.AreEqual(6f * 4f + Mathf.PI * 4f, painted, 6f, "Un trait de 6 m, large de 4, aux bouts ronds.");

            Tool.Choose(PlaceToolMode.Eraser);
            yield return Drag(Ground(-3f, 0f), Ground(-1f, 0f), Ground(1f, 0f), Ground(3f, 0f));
            _test.World.Step();
            Assert.Less(Zone.AreaSquareMeters, painted * 0.2f, "La gomme reprend le trait.");
        }

        [UnityTest]
        public IEnumerator AClickPutsACartSpotAndAClickOnItTakesItAway()
        {
            _test.World.Submit(new PaintPlaceCommand(new GroundDisc(new Vector2(50f, 50f), 5.5f)));
            _test.World.Step();
            Tool.Choose(PlaceToolMode.CartSpot);
            yield return PointAt(Ground(0f, 0f));
            Assert.AreEqual(PlaceResult.Ok, Tool.Result);
            yield return Click(Mouse.leftButton);
            _test.World.Step();
            Assert.AreEqual(1, Zone.Marks.Count, "Le clic pose une place de charrette.");

            yield return PointAt(Ground(0.5f, 0.5f));
            Assert.AreSame(Zone.Marks[0], Tool.HoveredMark, "Sur elle, un clic la retirerait.");
            yield return Click(Mouse.leftButton);
            _test.World.Step();
            Assert.AreEqual(0, Zone.Marks.Count, "Le clic sur elle la retire.");
        }

        [UnityTest]
        public IEnumerator RTurnsTheCartSpotAndCtrlWheelSizesTheBrush()
        {
            Tool.Choose(PlaceToolMode.CartSpot);
            yield return PointAt(Ground(0f, 0f));
            yield return Click(Keyboard.rKey);
            Assert.AreEqual(_test.Settings.RotationStepDegrees, Tool.HeadingDegrees, 0.01f, "R bref : un cran.");

            Tool.Choose(PlaceToolMode.Brush);
            float before = Tool.BrushRadius;
            Press(Keyboard.leftCtrlKey);
            yield return Played();
            Set(Mouse.scroll, new Vector2(0f, 120f));
            yield return Played();
            Release(Keyboard.leftCtrlKey);
            yield return Played();
            Assert.AreEqual(before + _test.Settings.BrushRadiusStepMeters, Tool.BrushRadius, 0.001f,
                "Ctrl + un cran de molette : le pinceau grandit d'un cran.");
        }

        [UnityTest]
        public IEnumerator ARightClickAbandonsTheShapeThenPutsTheToolDown()
        {
            Tool.Choose(PlaceToolMode.Disc);
            yield return PointAt(Ground(0f, 0f));
            Press(Mouse.leftButton);
            yield return Played();
            yield return PointAt(Ground(3f, 0f));
            yield return Click(Mouse.rightButton);
            Assert.IsFalse(Tool.IsDrawing, "Le clic droit abandonne le disque…");
            Assert.IsTrue(Tool.IsActive, "… et garde l'outil en main.");
            Release(Mouse.leftButton);
            yield return Played();
            _test.World.Step();
            Assert.AreEqual(0, Zone.CellCount, "Rien n'est tracé.");

            yield return Click(Mouse.rightButton);
            Assert.IsFalse(Tool.IsActive, "Sans geste, le clic droit pose l'outil de côté.");
        }
    }
}
