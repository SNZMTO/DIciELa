using System;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le relief. Les hauteurs et les pentes attendues sont calculées à la main : sur un plan qui monte d'un mètre par
    /// mètre, la pente est de 45° ; d'un demi-mètre, atan(0,5) = 26,565° ; en diagonale, atan(√2) = 54,736°.
    /// </summary>
    public sealed class ReliefTests
    {
        private const float Tolerance = 1e-3f;

        /// <summary>Une carte de 4 × 4 m, un point par mètre, dont la hauteur vaut riseX · x + riseZ · z.</summary>
        private static Relief Plane(float riseX, float riseZ)
        {
            var heights = new float[25];
            for (int z = 0; z < 5; z++)
            {
                for (int x = 0; x < 5; x++)
                    heights[z * 5 + x] = riseX * x + riseZ * z;
            }
            return new Relief(4f, 4f, 5, 5, heights);
        }

        [Test]
        public void ReadsTheHeightBetweenFourPoints()
        {
            // Une seule maille : 0 et 1 devant, 2 et 3 derrière.
            var relief = new Relief(1f, 1f, 2, 2, new[] { 0f, 1f, 2f, 3f });

            Assert.AreEqual(1.5f, relief.HeightAt(0.5f, 0.5f), Tolerance);
            Assert.AreEqual(1.75f, relief.HeightAt(0.25f, 0.75f), Tolerance);
            Assert.AreEqual(3f, relief.HeightAt(1f, 1f), Tolerance);
        }

        [Test]
        public void OutsideTheMapReadsTheNearestEdge()
        {
            Relief relief = Plane(1f, 0f);

            Assert.AreEqual(0f, relief.HeightAt(-3f, 2f), Tolerance);
            Assert.AreEqual(4f, relief.HeightAt(9f, 2f), Tolerance);
            Assert.IsFalse(relief.Contains(-0.1f, 2f));
            Assert.IsTrue(relief.Contains(4f, 4f));
        }

        [Test]
        public void MeasuresTheSlopeInDegrees()
        {
            Assert.AreEqual(0f, Plane(0f, 0f).SlopeDegreesAt(2.5f, 1.5f), Tolerance);
            Assert.AreEqual(45f, Plane(1f, 0f).SlopeDegreesAt(2.5f, 1.5f), Tolerance);
            Assert.AreEqual(45f, Plane(0f, 1f).SlopeDegreesAt(0.2f, 3.7f), Tolerance);
            Assert.AreEqual(26.565f, Plane(0.5f, 0f).SlopeDegreesAt(1.1f, 2.9f), Tolerance);
            Assert.AreEqual(54.736f, Plane(1f, 1f).SlopeDegreesAt(3.3f, 0.6f), Tolerance);
        }

        [Test]
        public void KeepsItsOwnCopyOfTheHeights()
        {
            var heights = new[] { 0f, 0f, 0f, 0f };
            var relief = new Relief(1f, 1f, 2, 2, heights);

            heights[3] = 100f;

            Assert.AreEqual(0f, relief.HeightAt(1f, 1f), Tolerance);
        }

        [Test]
        public void ARayFromAboveHitsTheGroundBelow()
        {
            Assert.IsTrue(Plane(0f, 0f).TryRaycast(new Vector3(1.5f, 10f, 2.5f), Vector3.down, 100f, out Vector3 hit));

            AssertNear(new Vector3(1.5f, 0f, 2.5f), hit);
        }

        [Test]
        public void ASlantedRayFollowsTheSlope()
        {
            // Le sol monte d'un demi-mètre par mètre vers l'est ; le rayon part de (0 ; 4,5 ; 2) et descend à 45° vers
            // l'est : il touche le sol là où 4,5 − x = 0,5 x, en x = 3, à 1,5 m de haut.
            Relief slope = Plane(0.5f, 0f);

            Assert.IsTrue(slope.TryRaycast(new Vector3(0f, 4.5f, 2f), new Vector3(1f, -1f, 0f), 100f, out Vector3 hit));

            AssertNear(new Vector3(3f, 1.5f, 2f), hit);
        }

        [Test]
        public void ARayThatDoesNotReachTheMapGroundHitsNothing()
        {
            Relief flat = Plane(0f, 0f);
            var above = new Vector3(2f, 10f, 2f);

            Assert.IsFalse(flat.TryRaycast(above, Vector3.up, 100f, out _));
            Assert.IsFalse(flat.TryRaycast(above, Vector3.down, 5f, out _));
            Assert.IsFalse(flat.TryRaycast(above, new Vector3(1f, -0.1f, 0f), 1000f, out _));
        }

        private static void AssertNear(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
            Assert.AreEqual(expected.z, actual.z, Tolerance);
        }

        [Test]
        public void RefusesAMalformedRelief()
        {
            Assert.Throws<ArgumentNullException>(() => new Relief(1f, 1f, 2, 2, null));
            Assert.Throws<ArgumentException>(() => new Relief(1f, 1f, 2, 2, new float[3]));
            Assert.Throws<ArgumentException>(() => new Relief(1f, 1f, 1, 2, new float[2]));
            Assert.Throws<ArgumentException>(() => new Relief(0f, 1f, 2, 2, new float[4]));
        }
    }
}
