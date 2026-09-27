using System.Collections;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La caméra de test avec le lissage du zoom de sa fiche : incliner soi-même, zoomer pendant plusieurs images, et
    /// donner un cran de molette pendant qu'on tire le sol. Les attendus du pointeur viennent de la projection inverse.
    /// </summary>
    public sealed class CameraMotionTests
    {
        private const int MaxFrames = 240;

        private TestCamera _test;

        [SetUp]
        public void CreateCamera()
        {
            _test = new TestCamera(smoothZoom: true);
        }

        [TearDown]
        public void DestroyCamera()
        {
            _test.Dispose();
        }

        [UnityTest]
        public IEnumerator TiltingStaysBetweenTheHorizonMarginAndTheBirdView()
        {
            float lowest = CameraPose.LowestPitch(_test.Settings);

            _test.Rig.Tilt(1000f);
            yield return null;
            Assert.AreEqual(lowest, _test.Holder.transform.eulerAngles.x, 0.05f);

            _test.Rig.Tilt(-1000f);
            yield return null;
            Assert.AreEqual(_test.Settings.MaxPitch, _test.Holder.transform.eulerAngles.x, 0.05f);
            Assert.That(Vector3.Distance(TestCamera.MapCenter, _test.LookedAtGround()), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator TheZoomKeepsItsGroundUnderThePointerWhileItGlides()
        {
            var pointed = new Vector3(42f, 10f, 57f);
            Vector2 pointer = _test.OnScreen(pointed);
            float before = _test.Rig.Distance;

            _test.Rig.Zoom(3f, pointer);
            float worst = 0f;
            for (int frame = 0; frame < MaxFrames; frame++)
            {
                yield return null;
                worst = Mathf.Max(worst, Vector2.Distance(pointer, _test.OnScreen(pointed)));
            }

            Assert.That(worst, Is.LessThan(1f), "Le sol visé reste sous le pointeur à chaque image du zoom.");
            Assert.That(_test.Rig.Distance, Is.LessThan(before));
        }

        [UnityTest]
        public IEnumerator ANotchDuringADragFollowsTheGrip()
        {
            var grabbed = new Vector3(45f, 10f, 56f);
            Vector2 pressed = _test.OnScreen(grabbed);
            Assert.IsTrue(_test.Rig.BeginDrag(pressed));
            _test.Rig.Zoom(1f, pressed);

            float worst = 0f;
            for (int frame = 1; frame <= 30; frame++)
            {
                Vector2 pointer = pressed + new Vector2(4f * frame, -3f * frame);
                _test.Rig.DragTo(pointer);
                yield return null;
                worst = Mathf.Max(worst, Vector2.Distance(pointer, _test.OnScreen(grabbed)));
            }

            Assert.That(worst, Is.LessThan(1.5f), "Le sol saisi suit le pointeur, même pendant le zoom.");
        }
    }
}
