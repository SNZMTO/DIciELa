using System.Collections;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les gestes de la pose, joués par une souris et un clavier simulés sur les vraies actions du jeu et le petit monde
    /// des tests : viser, poser au clic gauche, tourner à R bref ou tenu, poser l'outil de côté au clic droit bref ou à
    /// Échap. Un faux panneau d'interface occupe le bas de l'écran.
    /// </summary>
    public sealed class PlacementControlsTests : GestureFixture
    {
        private TestWorld _test;
        private PlacementControls _controls;
        private TestInterface _interface;

        public override void Setup()
        {
            base.Setup();
            _test = new TestWorld();
            var gestures = _test.Keep(ScriptableObject.CreateInstance<GestureSettings>());
            Assert.That(Moved, Is.GreaterThan(gestures.DragThresholdPixels * 2f));

            GameObject holder = _test.Keep(new GameObject("Gestes de test"));
            holder.SetActive(false);
            _controls = holder.AddComponent<PlacementControls>();
#if UNITY_EDITOR
            TestWorld.Fill(_controls, "_actions", _test.Keep(TestActions.CopyOfTheGame()));
            TestWorld.Fill(_controls, "_settings", _test.Settings);
            TestWorld.Fill(_controls, "_gestures", gestures);
#endif
            holder.SetActive(true);
            _interface = new TestInterface(new Rect(0f, 0f, Screen.width, Screen.height * 0.2f));
            _controls.Bind(_test.Tool, _interface);
            _test.Tool.Begin(_test.Cabin);
        }

        public override void TearDown()
        {
            _controls.enabled = false;
            _test.Dispose();
            base.TearDown();
        }

        private Transform Shown => _test.Preview.transform.GetChild(0);

        private Vector2 Center => _test.OnScreen(TestWorld.Center);

        private IEnumerator PointAt(Vector2 screenPoint)
        {
            Move(Mouse.position, screenPoint);
            yield return Played();
        }

        [UnityTest]
        public IEnumerator ThePointerAimsAndALeftClickPlaces()
        {
            yield return PointAt(Center);
            Assert.That(Vector3.Distance(TestWorld.Center, Shown.position), Is.LessThan(0.05f));

            yield return Click(Mouse.leftButton);
            _test.World.Step();

            Assert.AreEqual(1, _test.World.Structures.All.Count);
        }

        [UnityTest]
        public IEnumerator AClickThatBeginsOnTheInterfaceDoesNotPlace()
        {
            var onTheBar = new Vector2(Center.x, _interface.Area.yMax - 1f);
            yield return PointAt(onTheBar);
            Press(Mouse.leftButton);
            yield return Played();
            yield return PointAt(onTheBar + new Vector2(0f, 3f));
            Assert.AreEqual(PlacementResult.Ok, _test.Tool.Result, "Là où l'on relâche, la pose serait permise.");

            Release(Mouse.leftButton);
            yield return Played();
            _test.World.Step();

            Assert.AreEqual(0, _test.World.Structures.All.Count, "Le clic appartient à l'interface dès l'enfoncement.");
        }

        [UnityTest]
        public IEnumerator ABriefPressOnRTurnsByOneStep()
        {
            yield return PointAt(Center);

            yield return Click(Keyboard.rKey);
            yield return Click(Mouse.leftButton);
            _test.World.Step();

            Assert.AreEqual(_test.Settings.RotationStepDegrees,
                _test.World.Structures.All[0].Placement.RotationDegrees, 1e-3f);
        }

        [UnityTest]
        public IEnumerator HoldingRWhileMovingTheMouseTurnsByTheDegreeAndKeepsTheAim()
        {
            yield return PointAt(Center);
            Vector3 aimed = Shown.position;

            Press(Keyboard.rKey);
            yield return Played();
            yield return PointAt(Center + new Vector2(Moved, 0f));
            yield return PointAt(Center + new Vector2(Moved * 1.5f, 0f));

            float turned = Moved * 1.5f * _test.Settings.RotationDegreesPerPixel;
            Assert.AreEqual(0f, Mathf.DeltaAngle(turned, Shown.eulerAngles.y), 0.01f);
            Assert.That(Vector3.Distance(aimed, Shown.position), Is.LessThan(0.01f), "La visée attend pendant qu'on tourne.");
            Release(Keyboard.rKey);
            yield return Played();
        }

        [UnityTest]
        public IEnumerator ABriefRightClickPutsTheToolAside()
        {
            yield return PointAt(Center);

            yield return Click(Mouse.rightButton);

            Assert.IsFalse(_test.Tool.IsActive);
        }

        [UnityTest]
        public IEnumerator ARightDragKeepsTheTool()
        {
            yield return PointAt(Center);

            Press(Mouse.rightButton);
            yield return Played();
            yield return PointAt(Center + new Vector2(Moved, 0f));
            Release(Mouse.rightButton);
            yield return Played();

            Assert.IsTrue(_test.Tool.IsActive, "Un clic droit glissé tire la vue, il ne pose pas l'outil de côté.");
        }

        [UnityTest]
        public IEnumerator EscapePutsTheToolAside()
        {
            yield return Click(Keyboard.escapeKey);

            Assert.IsFalse(_test.Tool.IsActive);
        }
    }
}
