using Diciela.Controls;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Clic ou glisser (SAVOIR, 2.8), avec un seuil de 6 pixels. Les positions attendues sont écrites à la main.
    /// </summary>
    public sealed class DragGestureTests
    {
        private const float Threshold = 6f;

        [Test]
        public void ABriefPressNeverDrags()
        {
            var gesture = new DragGesture();

            gesture.Update(true, new Vector2(100f, 100f), Threshold);
            bool started = gesture.Update(true, new Vector2(103f, 102f), Threshold);
            gesture.Update(false, new Vector2(103f, 102f), Threshold);

            Assert.IsFalse(started);
            Assert.IsFalse(gesture.IsDragging);
        }

        [Test]
        public void BeyondTheThresholdItDragsWithTheWholeMove()
        {
            var gesture = new DragGesture();
            gesture.Update(true, new Vector2(100f, 100f), Threshold);

            Assert.IsTrue(gesture.Update(true, new Vector2(110f, 100f), Threshold));
            Assert.AreEqual(new Vector2(10f, 0f), gesture.Delta);

            Assert.IsFalse(gesture.Update(true, new Vector2(115f, 98f), Threshold));
            Assert.IsTrue(gesture.IsDragging);
            Assert.AreEqual(new Vector2(5f, -2f), gesture.Delta);
            Assert.AreEqual(new Vector2(100f, 100f), gesture.Start);
        }

        [Test]
        public void AReleaseWithoutDraggingIsAClickDecidedAtTheRelease()
        {
            var gesture = new DragGesture();

            gesture.Update(true, new Vector2(100f, 100f), Threshold);
            Assert.IsTrue(gesture.Began);
            gesture.Update(true, new Vector2(102f, 101f), Threshold);
            Assert.IsFalse(gesture.Clicked);
            gesture.Update(false, new Vector2(102f, 101f), Threshold);
            Assert.IsTrue(gesture.Clicked);
            gesture.Update(false, new Vector2(102f, 101f), Threshold);
            Assert.IsFalse(gesture.Clicked);
        }

        [Test]
        public void ADragIsNeverAClick()
        {
            var gesture = new DragGesture();

            gesture.Update(true, new Vector2(100f, 100f), Threshold);
            gesture.Update(true, new Vector2(130f, 100f), Threshold);
            gesture.Update(false, new Vector2(130f, 100f), Threshold);

            Assert.IsFalse(gesture.Clicked);
        }

        [Test]
        public void APressFirstSeenLateStartsWhereItIsSeen()
        {
            var gesture = new DragGesture();

            // Le bouton était déjà enfoncé quand on le regarde pour la première fois (la carte se chargeait) : pas de
            // saut depuis une vieille position.
            gesture.Update(true, new Vector2(500f, 300f), Threshold);
            bool started = gesture.Update(true, new Vector2(502f, 300f), Threshold);

            Assert.AreEqual(new Vector2(500f, 300f), gesture.Start);
            Assert.IsFalse(started);
            Assert.IsFalse(gesture.IsDragging);
        }
    }
}
