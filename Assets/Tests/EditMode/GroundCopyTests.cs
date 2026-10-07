using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using static Diciela.Tests.EditMode.TestFloors;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La copie d'une forme par symétrie (décision 219), et l'union qui peint d'un geste une forme et ses copies, sur le
    /// monde plat de 40 m aux mailles de 50 cm : un point et un cap tournés ou retournés comme à la main ; un rectangle
    /// tourné ou retourné couvre le rectangle écrit à la main ; une union peint chaque maille une fois.
    /// </summary>
    public sealed class GroundCopyTests
    {
        private static readonly Vector2 Pin = new Vector2(10f, 10f);
        private TestTown _town;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void APointTurnsAndMirrorsAsByHand()
        {
            AssertNear(new Vector2(15f, 10f), GroundCopy.Turned(Pin, 90f).Apply(new Vector2(10f, 15f)),
                "Un quart de tour : le nord passe à l'est.");
            AssertNear(new Vector2(8f, 7f), GroundCopy.Turned(Pin, 180f).Apply(new Vector2(12f, 13f)), "Un demi-tour.");
            AssertNear(new Vector2(8f, 13f), GroundCopy.Mirrored(Pin, 0f).Apply(new Vector2(12f, 13f)),
                "L'axe nord-sud retourne l'est en ouest.");
            AssertNear(new Vector2(12f, 7f), GroundCopy.Mirrored(Pin, 90f).Apply(new Vector2(12f, 13f)),
                "L'axe est-ouest retourne le nord en sud.");
            AssertNear(new Vector2(10f, 12f), GroundCopy.Mirrored(Pin, 45f).Apply(new Vector2(12f, 10f)),
                "La diagonale échange l'est et le nord.");
        }

        [Test]
        public void AHeadingTurnsAndMirrorsAsByHand()
        {
            Assert.AreEqual(120f, GroundCopy.Turned(Pin, 90f).HeadingOf(30f), 1e-4f);
            Assert.AreEqual(330f, GroundCopy.Mirrored(Pin, 0f).HeadingOf(30f), 1e-4f, "Le nord-nord-est devient nord-nord-ouest.");
            Assert.AreEqual(150f, GroundCopy.Mirrored(Pin, 90f).HeadingOf(30f), 1e-4f, "… ou sud-sud-est, par l'axe est-ouest.");
        }

        [Test]
        public void ABoxTurnedOrMirroredCoversTheBoxWrittenByHand()
        {
            // Un rectangle de 6 × 2 m, tourné d'un quart de tour autour de son coin sud-ouest (17, 19) : 2 × 6 m, centré
            // en (18, 16).
            World turned = _town.World();
            Trace(turned, Box(20f, 20f, 6f, 2f).Copied(GroundCopy.Turned(new Vector2(17f, 19f), 90f)), _town.Dirt);
            World turnedByHand = _town.World();
            Trace(turnedByHand, Box(18f, 16f, 2f, 6f), _town.Dirt);
            CollectionAssert.AreEquivalent(PlannedOf(turnedByHand), PlannedOf(turned));

            // Penché de 30°, à l'ouest du milieu, retourné par l'axe nord-sud : à l'est, penché de -30°.
            World mirrored = _town.World();
            var leaning = new GroundBox(new RotatedRect(new Vector2(15f, 20f), new Vector2(4f, 2f), 30f));
            Trace(mirrored, leaning.Copied(GroundCopy.Mirrored(Middle, 0f)), _town.Dirt);
            World mirroredByHand = _town.World();
            Trace(mirroredByHand, new GroundBox(new RotatedRect(new Vector2(25f, 20f), new Vector2(4f, 2f), 330f)),
                _town.Dirt);
            CollectionAssert.AreEquivalent(PlannedOf(mirroredByHand), PlannedOf(mirrored));
        }

        [Test]
        public void AUnionPaintsEachCellOnce()
        {
            // Le disque de 2 m au milieu couvre 52 mailles de 0,25 m² (comptées à la main) : 13 m².
            var disc = new GroundDisc(Middle, 2f);
            World onItself = _town.World();
            Trace(onItself, new GroundUnion(new GroundShape[] { disc, disc.Copied(GroundCopy.Turned(Middle, 90f)) }),
                _town.Dirt);
            Assert.AreEqual(13f, OnlySite(onItself).Floor.AreaSquareMeters, 0.001f, "Sa copie sur lui-même ne compte pas.");

            World apart = _town.World();
            Trace(apart, new GroundUnion(new GroundShape[] { disc, disc.Copied(GroundCopy.Mirrored(new Vector2(25f, 20f), 0f)) }),
                _town.Dirt);
            Assert.AreEqual(26f, OnlySite(apart).Floor.AreaSquareMeters, 0.001f, "Deux disques loin l'un de l'autre.");
            Assert.AreEqual(FloorState.Planned, StateAt(apart, new Vector2(30f, 20f)), "La copie, de l'autre côté de l'axe.");
        }

        private static void AssertNear(Vector2 expected, Vector2 actual, string message)
        {
            Assert.AreEqual(0f, Vector2.Distance(expected, actual), 1e-4f, message + " (" + actual + ")");
        }
    }
}
