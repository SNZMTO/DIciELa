using System.Collections.Generic;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La courbe douce d'un tracé ou d'un polygone arrondi (décision 216) : elle passe par chacun de ses points, dans
    /// l'ordre ; sur des points alignés, elle reste sur leur ligne (ni boucle ni pointe) ; ses petits segments restent
    /// courts ; fermée, elle revient au premier point ; deux points ne font qu'un segment.
    /// </summary>
    public sealed class GroundSplineTests
    {
        private const float Step = 0.25f;
        private static readonly Vector2[] Zigzag =
        {
            new Vector2(0f, 0f), new Vector2(4f, 3f), new Vector2(7f, -1f), new Vector2(12f, 2f)
        };

        [Test]
        public void TheCurvePassesThroughEveryPointInOrder()
        {
            var curve = new List<Vector2>();
            GroundSpline.Sample(Zigzag, false, Step, curve);
            int from = 0;
            foreach (Vector2 point in Zigzag)
            {
                int at = IndexOf(curve, point, from);
                Assert.GreaterOrEqual(at, from, "Le point " + point + " est sur la courbe, après le précédent.");
                from = at;
            }
            Assert.AreEqual(Zigzag[Zigzag.Length - 1], curve[curve.Count - 1], "Ouverte, elle finit au dernier point.");
        }

        [Test]
        public void OnAlignedPointsItStaysOnTheirLine()
        {
            var aligned = new[] { new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(5f, 5f), new Vector2(6f, 6f) };
            var curve = new List<Vector2>();
            GroundSpline.Sample(aligned, false, Step, curve);
            foreach (Vector2 point in curve)
            {
                Assert.AreEqual(point.x, point.y, 1e-3f, "Sur la diagonale.");
                Assert.That(point.x, Is.InRange(-1e-3f, 6f + 1e-3f), "Sans dépasser les bouts.");
            }
        }

        [Test]
        public void ItsPiecesStayShort()
        {
            var curve = new List<Vector2>();
            GroundSpline.Sample(Zigzag, false, Step, curve);
            for (int i = 1; i < curve.Count; i++)
                Assert.LessOrEqual((curve[i] - curve[i - 1]).magnitude, 2f * Step, "Le segment n° " + i + ".");
        }

        [Test]
        public void ClosedItComesBackToItsFirstPoint()
        {
            var curve = new List<Vector2>();
            GroundSpline.Sample(Zigzag, true, Step, curve);
            Assert.AreEqual(Zigzag[0], curve[0]);
            Assert.LessOrEqual((curve[curve.Count - 1] - curve[0]).magnitude, 2f * Step, "Le dernier segment ferme la boucle.");
            Assert.GreaterOrEqual(IndexOf(curve, Zigzag[3], 0), 0, "Elle passe aussi par le dernier point.");
        }

        [Test]
        public void TwoPointsMakeOneSegment()
        {
            var curve = new List<Vector2>();
            GroundSpline.Sample(new[] { Vector2.zero, Vector2.right }, false, Step, curve);
            CollectionAssert.AreEqual(new[] { Vector2.zero, Vector2.right }, curve);
        }

        // L'indice du premier point de la courbe, à partir de from, qui tombe sur ce point ; -1 s'il n'y en a pas.
        private static int IndexOf(List<Vector2> curve, Vector2 point, int from)
        {
            for (int i = from; i < curve.Count; i++)
            {
                if ((curve[i] - point).sqrMagnitude < 1e-8f)
                    return i;
            }
            return -1;
        }
    }
}
