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

            gesture.Update(true, new Vector2(100f, 100f), Threshold, false);
            bool started = gesture.Update(true, new Vector2(103f, 102f), Threshold, false);
            gesture.Update(false, new Vector2(103f, 102f), Threshold, false);

            Assert.IsFalse(started);
            Assert.IsFalse(gesture.IsDragging);
        }

        [Test]
        public void BeyondTheThresholdItDragsWithTheWholeMove()
        {
            var gesture = new DragGesture();
            gesture.Update(true, new Vector2(100f, 100f), Threshold, false);

            Assert.IsTrue(gesture.Update(true, new Vector2(110f, 100f), Threshold, false));
            Assert.AreEqual(new Vector2(10f, 0f), gesture.Delta);

            Assert.IsFalse(gesture.Update(true, new Vector2(115f, 98f), Threshold, false));
            Assert.IsTrue(gesture.IsDragging);
            Assert.AreEqual(new Vector2(5f, -2f), gesture.Delta);
            Assert.AreEqual(new Vector2(100f, 100f), gesture.Start);
        }

        [Test]
        public void AReleaseWithoutDraggingIsAClickDecidedAtTheRelease()
        {
            var gesture = new DragGesture();

            gesture.Update(true, new Vector2(100f, 100f), Threshold, false);
            gesture.Update(true, new Vector2(102f, 101f), Threshold, false);
            Assert.IsFalse(gesture.Clicked);
            gesture.Update(false, new Vector2(102f, 101f), Threshold, false);
            Assert.IsTrue(gesture.Clicked);
            gesture.Update(false, new Vector2(102f, 101f), Threshold, false);
            Assert.IsFalse(gesture.Clicked);
        }

        [Test]
        public void ADragIsNeverAClick()
        {
            var gesture = new DragGesture();

            gesture.Update(true, new Vector2(100f, 100f), Threshold, false);
            gesture.Update(true, new Vector2(130f, 100f), Threshold, false);
            Assert.IsFalse(gesture.Dropped, "Tenu, le glisser n'est pas fini.");
            gesture.Update(false, new Vector2(130f, 100f), Threshold, false);

            Assert.IsFalse(gesture.Clicked);
            Assert.IsTrue(gesture.Dropped, "Relâché, il finit le glisser…");
            gesture.Update(false, new Vector2(130f, 100f), Threshold, false);
            Assert.IsFalse(gesture.Dropped, "… une seule fois.");
        }

        [Test]
        public void APressThatBeginsOnTheInterfaceBelongsToIt()
        {
            var gesture = new DragGesture();

            // Il commence sur la barre, puis quitte l'interface : ni glisser, ni clic pour la ville.
            gesture.Update(true, new Vector2(100f, 20f), Threshold, true);
            bool started = gesture.Update(true, new Vector2(160f, 300f), Threshold, false);
            gesture.Update(false, new Vector2(160f, 300f), Threshold, false);

            Assert.IsFalse(started);
            Assert.IsFalse(gesture.IsDragging);
            Assert.IsFalse(gesture.Clicked);
        }

        [Test]
        public void APressHeldElsewhereNeitherDragsNorClicksTillReleased()
        {
            // L'outil est pris pendant que le bouton est tenu : cet appui appartient à ce qui était en main.
            DragGesture gesture = DragGesture.HeldElsewhere(true);

            Assert.IsFalse(gesture.Update(true, new Vector2(100f, 100f), Threshold, false));
            Assert.IsFalse(gesture.Update(true, new Vector2(140f, 100f), Threshold, false));
            Assert.IsFalse(gesture.IsDragging, "Ni glisser…");
            gesture.Update(false, new Vector2(140f, 100f), Threshold, false);
            Assert.IsFalse(gesture.Clicked, "…ni clic au relâchement.");

            gesture.Update(true, new Vector2(200f, 200f), Threshold, false);
            gesture.Update(false, new Vector2(200f, 200f), Threshold, false);
            Assert.IsTrue(gesture.Clicked, "L'appui suivant est à l'outil.");
        }

        [Test]
        public void APressFirstSeenLateStartsWhereItIsSeen()
        {
            var gesture = new DragGesture();

            // Le bouton était déjà enfoncé quand on le regarde pour la première fois (la carte se chargeait) : pas de
            // saut depuis une vieille position.
            gesture.Update(true, new Vector2(500f, 300f), Threshold, false);
            bool started = gesture.Update(true, new Vector2(502f, 300f), Threshold, false);

            Assert.AreEqual(new Vector2(500f, 300f), gesture.Start);
            Assert.IsFalse(started);
            Assert.IsFalse(gesture.IsDragging);
        }
    }
}
