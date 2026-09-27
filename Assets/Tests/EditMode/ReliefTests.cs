using System;
using Diciela.Simulation.Land;
using NUnit.Framework;

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
        public void RefusesAMalformedRelief()
        {
            Assert.Throws<ArgumentNullException>(() => new Relief(1f, 1f, 2, 2, null));
            Assert.Throws<ArgumentException>(() => new Relief(1f, 1f, 2, 2, new float[3]));
            Assert.Throws<ArgumentException>(() => new Relief(1f, 1f, 1, 2, new float[2]));
            Assert.Throws<ArgumentException>(() => new Relief(0f, 1f, 2, 2, new float[4]));
        }
    }
}
