using System.Collections.Generic;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// L'anneau carré : l'anneau 0 est la maille elle-même ; l'anneau n compte 8 × n décalages, tous différents, à n
    /// mailles sur un axe au moins et pas plus sur l'autre, de la rangée du bas à celle du haut.
    /// </summary>
    public sealed class SquareRingTests
    {
        [Test]
        public void RingZeroIsTheCellItself()
        {
            var offsets = new List<Vector2Int>();
            foreach (Vector2Int offset in new SquareRing(0))
                offsets.Add(offset);

            Assert.AreEqual(new[] { Vector2Int.zero }, offsets.ToArray());
        }

        [Test]
        public void EachRingHoldsItsBorderOnceFromBottomToTop()
        {
            for (int ring = 1; ring <= 4; ring++)
            {
                var seen = new HashSet<Vector2Int>();
                int lastRow = -ring;
                foreach (Vector2Int offset in new SquareRing(ring))
                {
                    Assert.AreEqual(ring, Mathf.Max(Mathf.Abs(offset.x), Mathf.Abs(offset.y)), "Sur l'anneau " + ring);
                    Assert.GreaterOrEqual(offset.y, lastRow, "De la rangée du bas à celle du haut.");
                    Assert.IsTrue(seen.Add(offset), "Vu une seule fois : " + offset);
                    lastRow = offset.y;
                }
                Assert.AreEqual(8 * ring, seen.Count, "L'anneau " + ring + " entier.");
            }
        }
    }
}
