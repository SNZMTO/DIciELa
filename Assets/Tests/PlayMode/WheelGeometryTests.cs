using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les mesures de la roue des outils (décision 168), dans le repère du panneau (y vers le bas) : deux outils à 132 px
    /// du centre, l'un en haut, l'autre en bas ; on vise par la direction, rien près du centre ; ouverte au coin d'un
    /// écran de 1280 × 720, la roue est ramenée dedans. Des mesures écrites ici, pas celles de la feuille de style.
    /// </summary>
    public sealed class WheelGeometryTests
    {
        private static readonly Vector2 Center = new Vector2(400f, 300f);
        private static readonly Vector2[] Tools = { new Vector2(400f, 168f), new Vector2(400f, 432f) };

        [Test]
        public void TheAimedToolIsTheOneInThePointersDirection()
        {
            Assert.AreEqual(0, WheelGeometry.Aimed(Center, Tools, new Vector2(430f, 220f)), "Vers le haut : le premier.");
            Assert.AreEqual(1, WheelGeometry.Aimed(Center, Tools, new Vector2(350f, 380f)), "Vers le bas : le second.");
            Assert.AreEqual(0, WheelGeometry.Aimed(Center, Tools, new Vector2(400f, 20f)),
                "Au-delà de l'outil, la direction compte encore.");
        }

        [Test]
        public void NearTheCenterNothingIsAimed()
        {
            Assert.AreEqual(-1, WheelGeometry.Aimed(Center, Tools, new Vector2(400f, 250f)),
                "À 50 px, en deçà de la moitié des 132 px : rien.");
            Assert.AreEqual(-1, WheelGeometry.Aimed(Center, new Vector2[0], new Vector2(400f, 100f)));
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
    }
}
