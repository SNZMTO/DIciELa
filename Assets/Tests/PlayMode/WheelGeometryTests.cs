using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les mesures de la roue (décisions 168, 176 et 221), dans le repère du panneau (y vers le bas) : deux outils
    /// à 132 px du centre, l'un en haut, l'autre en bas ; on vise par la direction, rien près du point d'ouverture ;
    /// ouverte au coin d'un écran de 1280 × 720, la roue est ramenée dedans, et relâcher là où elle s'est ouverte ne
    /// choisit toujours rien. Des mesures écrites ici, pas celles de la feuille de style.
    /// </summary>
    public sealed class WheelGeometryTests
    {
        private static readonly Vector2 Center = new Vector2(400f, 300f);
        private static readonly Vector2[] Tools = { new Vector2(400f, 168f), new Vector2(400f, 432f) };

        [Test]
        public void TheAimedToolIsTheOneInThePointersDirection()
        {
            Assert.AreEqual(0, WheelGeometry.Aimed(Center, Center, Tools, new Vector2(430f, 220f)),
                "Vers le haut : le premier.");
            Assert.AreEqual(1, WheelGeometry.Aimed(Center, Center, Tools, new Vector2(350f, 380f)),
                "Vers le bas : le second.");
            Assert.AreEqual(0, WheelGeometry.Aimed(Center, Center, Tools, new Vector2(400f, 20f)),
                "Au-delà de l'outil, la direction compte encore.");
        }

        [Test]
        public void NearTheCenterNothingIsAimed()
        {
            Assert.AreEqual(-1, WheelGeometry.Aimed(Center, Center, Tools, new Vector2(400f, 250f)),
                "À 50 px, en deçà de la moitié des 132 px : rien.");
            Assert.AreEqual(-1, WheelGeometry.Aimed(Center, Center, new Vector2[0], new Vector2(400f, 100f)));
        }

        [Test]
        public void PulledFromTheEdgeItAimsByTheMoveFromWhereItOpened()
        {
            // Ouverte à 80 px sous son centre (le pointeur près du bas de l'écran), la roue a été remontée.
            var openedAt = new Vector2(400f, 380f);

            Assert.AreEqual(-1, WheelGeometry.Aimed(openedAt, Center, Tools, openedAt),
                "Sans bouger, rien : relâcher la referme sans choisir.");
            Assert.AreEqual(-1, WheelGeometry.Aimed(openedAt, Center, Tools, new Vector2(420f, 330f)),
                "À 54 px du point d'ouverture, encore rien.");
            Assert.AreEqual(0, WheelGeometry.Aimed(openedAt, Center, Tools, new Vector2(400f, 310f)),
                "Monté de 70 px, le premier, bien que le pointeur reste sous le centre.");
            Assert.AreEqual(1, WheelGeometry.Aimed(openedAt, Center, Tools, new Vector2(400f, 450f)),
                "Descendu de 70 px, le second.");
        }

        [Test]
        public void TheWheelIsPulledInsideTheScreen()
        {
            var extent = Rect.MinMaxRect(-120f, -170f, 120f, 170f);
            var room = new Rect(0f, 0f, 1280f, 720f);

            Assert.AreEqual(new Vector2(1160f, 550f), WheelGeometry.Clamped(new Vector2(1250f, 700f), extent, room),
                "Au coin bas-droit, la roue remonte et revient à gauche juste assez.");
            Assert.AreEqual(new Vector2(640f, 360f), WheelGeometry.Clamped(new Vector2(640f, 360f), extent, room),
                "Au milieu, elle ne bouge pas.");
        }

        [Test]
        public void ASlotSitsOnItsCircleAtItsPlace()
        {
            AssertNear(new Vector2(0f, -132f), WheelGeometry.OnCircle(0f, 132f), "La place 0, en haut.");
            AssertNear(new Vector2(132f, 0f), WheelGeometry.OnCircle(90f, 132f), "La place 2, à droite.");
            AssertNear(new Vector2(-93.34f, -93.34f), WheelGeometry.OnCircle(315f, 132f), "La place 7, en haut à gauche.");
            Assert.AreEqual(75f, WheelGeometry.OptionDegrees(90f, 0, 2, 30f), 1e-3f, "Deux options autour de 90° : 75°…");
            Assert.AreEqual(105f, WheelGeometry.OptionDegrees(90f, 1, 2, 30f), 1e-3f, "… et 105°.");
        }

        [Test]
        public void AnOptionIsAimedOnlyWhenThePointerReachesTheFan()
        {
            // Une case à droite (90°), trois options écartées de 30° : 60°, 90° et 120°, le seuil à 180 px du centre.
            Assert.AreEqual(-1, WheelGeometry.AimedOption(Center, 90f, 3, 30f, 180f, Center + new Vector2(150f, 0f)),
                "En deçà du seuil, on vise la case, pas une option.");
            Assert.AreEqual(1, WheelGeometry.AimedOption(Center, 90f, 3, 30f, 180f, Center + new Vector2(220f, 0f)),
                "Au-delà, droit devant : l'option du milieu.");
            Assert.AreEqual(0, WheelGeometry.AimedOption(Center, 90f, 3, 30f, 180f,
                Center + WheelGeometry.OnCircle(57f, 220f)), "Vers 57° : la première, à 60°.");
            Assert.AreEqual(2, WheelGeometry.AimedOption(Center, 90f, 3, 30f, 180f,
                Center + WheelGeometry.OnCircle(128f, 220f)), "Vers 128° : la dernière, à 120°.");
            Assert.AreEqual(-1, WheelGeometry.AimedOption(Center, 90f, 3, 30f, 180f,
                Center + WheelGeometry.OnCircle(180f, 220f)), "Vers le bas, hors de l'éventail : aucune.");
            Assert.AreEqual(-1, WheelGeometry.AimedOption(Center, 90f, 0, 30f, 180f, Center + new Vector2(220f, 0f)),
                "Une case sans option n'en a aucune à viser.");
        }

        private static void AssertNear(Vector2 expected, Vector2 actual, string message)
        {
            Assert.AreEqual(0f, Vector2.Distance(expected, actual), 0.05f, message + " (" + actual + ")");
        }
    }
}
