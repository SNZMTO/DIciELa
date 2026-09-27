using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La caméra de gestion (la caméra de test, sans lissage du zoom) : se montrer, zoomer, tourner, tirer le sol, se
    /// déplacer. Les gestes l'appellent comme le joueur. Les attendus du pointeur viennent de la projection inverse
    /// (WorldToScreenPoint), pas du rayon qui sert à la caméra.
    /// </summary>
    public sealed class CameraRigTests
    {
        private TestCamera _test;

        [SetUp]
        public void CreateCamera()
        {
            _test = new TestCamera();
        }

        [TearDown]
        public void DestroyCamera()
        {
            _test.Dispose();
        }

        [Test]
        public void ShowsTheMapFromItsCenterAtTheStartDistance()
        {
            Assert.AreEqual(_test.Settings.FieldOfView, _test.Camera.fieldOfView, 1e-4f);
            Assert.That(Vector3.Distance(TestCamera.MapCenter, _test.LookedAtGround()), Is.LessThan(0.01f));
            Assert.AreEqual(_test.Settings.StartDistance,
                Vector3.Distance(_test.Holder.transform.position, TestCamera.MapCenter), 0.01f);
        }

        [UnityTest]
        public IEnumerator ZoomingInComesCloserAndLower()
        {
            float distanceBefore = Vector3.Distance(_test.Holder.transform.position, TestCamera.MapCenter);
            float pitchBefore = _test.Holder.transform.eulerAngles.x;

            _test.Rig.Zoom(3f, _test.Camera.pixelRect.center);
            yield return null;

            Assert.That(Vector3.Distance(_test.Holder.transform.position, TestCamera.MapCenter), Is.LessThan(distanceBefore));
            Assert.That(_test.Holder.transform.eulerAngles.x, Is.LessThan(pitchBefore));
            Assert.That(Vector3.Distance(TestCamera.MapCenter, _test.LookedAtGround()), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator ZoomingKeepsThePointedGroundUnderThePointer()
        {
            var pointed = new Vector3(42f, 10f, 57f);
            Vector2 pointer = _test.OnScreen(pointed);
            float distanceBefore = Vector3.Distance(_test.Holder.transform.position, pointed);

            _test.Rig.Zoom(3f, pointer);
            yield return null;

            Assert.That(Vector2.Distance(pointer, _test.OnScreen(pointed)), Is.LessThan(1f));
            Assert.That(Vector3.Distance(_test.Holder.transform.position, pointed), Is.LessThan(distanceBefore));
        }

        [UnityTest]
        public IEnumerator RotatingTurnsAroundTheSamePoint()
        {
            _test.Rig.Rotate(90f);
            yield return null;

            Assert.AreEqual(90f, _test.Holder.transform.eulerAngles.y, 0.01f);
            Assert.That(Vector3.Distance(TestCamera.MapCenter, _test.LookedAtGround()), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator DraggingKeepsTheGrabbedGroundUnderThePointer()
        {
            var grabbed = new Vector3(45f, 10f, 56f);
            Vector2 pressed = _test.OnScreen(grabbed);
            Vector2 released = pressed + new Vector2(80f, -60f);

            Assert.IsTrue(_test.Rig.BeginDrag(pressed));
            _test.Rig.DragTo(released);
            yield return null;

            Assert.That(Vector2.Distance(released, _test.OnScreen(grabbed)), Is.LessThan(1f));
        }

        [Test]
        public void TheSkyCannotBeGrabbed()
        {
            var farAboveTheScreen = new Vector2(Screen.width * 0.5f, Screen.height * 3f);

            Assert.IsFalse(_test.Rig.BeginDrag(farAboveTheScreen));
        }

        [UnityTest]
        public IEnumerator PanningNeverLeavesTheMap()
        {
            _test.Rig.Pan(new Vector2(1f, 1f), 1000f);
            yield return null;

            Vector3 ground = _test.LookedAtGround();
            Assert.AreEqual(100f, ground.x, 0.01f);
            Assert.AreEqual(100f, ground.z, 0.01f);
        }

        [UnityTest]
        public IEnumerator AHiddenCameraNoLongerMoves()
        {
            Vector3 before = _test.Holder.transform.position;

            _test.Rig.Hide();
            _test.Rig.Pan(new Vector2(1f, 0f), 1f);
            _test.Rig.Zoom(3f, _test.Camera.pixelRect.center);
            yield return null;

            Assert.IsFalse(_test.Rig.IsShowing);
            Assert.AreEqual(before, _test.Holder.transform.position);
        }
    }
}
