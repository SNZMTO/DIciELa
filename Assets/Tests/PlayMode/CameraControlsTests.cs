using System.Collections;
using Diciela.Controls;
using Diciela.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les gestes de la caméra, joués par une souris simulée sur les vraies actions du jeu et la caméra de test, sans
    /// lissage du zoom : la molette, le clic droit glissé, le bouton du milieu glissé, et la molette muette sur
    /// l'interface.
    /// </summary>
    public sealed class CameraControlsTests : GestureFixture
    {
        private TestCamera _camera;
        private GestureSettings _gestures;
        private InputActionAsset _actions;
        private GameObject _holder;
        private CameraControls _controls;

        public override void Setup()
        {
            base.Setup();
            _camera = new TestCamera();
            _gestures = ScriptableObject.CreateInstance<GestureSettings>();
            _actions = TestActions.CopyOfTheGame();
            Assert.That(Moved, Is.GreaterThan(_gestures.DragThresholdPixels * 2f));

            _holder = new GameObject("Gestes de test");
            _holder.SetActive(false);
            _controls = _holder.AddComponent<CameraControls>();
#if UNITY_EDITOR
            TestWorld.Fill(_controls, "_actions", _actions);
            TestWorld.Fill(_controls, "_rig", _camera.Rig);
            TestWorld.Fill(_controls, "_settings", _camera.Settings);
            TestWorld.Fill(_controls, "_gestures", _gestures);
#endif
            _holder.SetActive(true);
        }

        public override void TearDown()
        {
            _controls.enabled = false;
            Object.DestroyImmediate(_holder);
            Object.DestroyImmediate(_actions);
            Object.DestroyImmediate(_gestures);
            _camera.Dispose();
            base.TearDown();
        }

        private IEnumerator PointAt(Vector2 screenPoint)
        {
            Move(Mouse.position, screenPoint);
            yield return Played();
        }

        private IEnumerator ScrollOneNotchAt(Vector2 pointer)
        {
            yield return PointAt(pointer);
            Set(Mouse.scroll, new Vector2(0f, _camera.Settings.ScrollPerNotch));
            yield return Played();
        }

        [UnityTest]
        public IEnumerator TheWheelComesCloserAndKeepsThePointedGroundUnderThePointer()
        {
            var pointed = new Vector3(42f, 10f, 57f);
            Vector2 pointer = _camera.OnScreen(pointed);
            float before = _camera.Rig.Distance;

            yield return ScrollOneNotchAt(pointer);

            Assert.AreEqual(before * (1f - _camera.Settings.ZoomStepPerNotch), _camera.Rig.Distance, 0.01f);
            Assert.That(Vector2.Distance(pointer, _camera.OnScreen(pointed)), Is.LessThan(1f));
        }

        [UnityTest]
        public IEnumerator TheWheelOverTheInterfaceDoesNotZoom()
        {
            _controls.Bind(new TestInterface(new Rect(0f, 0f, Screen.width, Screen.height)));
            float before = _camera.Rig.Distance;

            yield return ScrollOneNotchAt(_camera.Camera.pixelRect.center);

            Assert.AreEqual(before, _camera.Rig.Distance, 1e-4f);
        }

        [UnityTest]
        public IEnumerator ARightDragPullsTheGrabbedGroundUnderThePointer()
        {
            var grabbed = new Vector3(45f, 10f, 56f);
            Vector2 pressed = _camera.OnScreen(grabbed);
            Vector2 released = pressed + new Vector2(2f * Moved, -1.5f * Moved);
            yield return PointAt(pressed);

            Press(Mouse.rightButton);
            yield return Played();
            yield return PointAt(released);

            Assert.That(Vector2.Distance(released, _camera.OnScreen(grabbed)), Is.LessThan(1f));
            Release(Mouse.rightButton);
            yield return Played();
        }

        [UnityTest]
        public IEnumerator AMiddleDragTurnsAndTiltsTheView()
        {
            Vector2 start = _camera.Camera.pixelRect.center;
            yield return PointAt(start);
            float yaw = _camera.Holder.transform.eulerAngles.y;
            float pitch = _camera.Rig.PitchDegrees;

            Press(Mouse.middleButton);
            yield return Played();
            yield return PointAt(start + new Vector2(Moved, Moved / 2f));

            Assert.AreEqual(Moved * _camera.Settings.OrbitDegreesPerPixel,
                Mathf.DeltaAngle(yaw, _camera.Holder.transform.eulerAngles.y), 0.01f);
            Assert.AreEqual(pitch - Moved / 2f * _camera.Settings.TiltDegreesPerPixel, _camera.Rig.PitchDegrees, 0.05f,
                "La souris qui monte relève la vue vers l'horizon.");
            Release(Mouse.middleButton);
            yield return Played();
        }
    }
}
