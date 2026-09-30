using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La taille de l'interface selon l'écran (décision 144), avec les mesures de la maquette données par le test (900 px
    /// de haut, 1280 × 720 de place au moins) ; les attendus sont calculés à la main.
    /// </summary>
    public sealed class InterfaceScaleTests
    {
        private const float ReferenceHeight = 900f;
        private static readonly Vector2 MinimumRoom = new Vector2(1280f, 720f);

        [TestCase(1600f, 900f, 1f, 1f, TestName = "L'écran de la maquette : taille de référence")]
        [TestCase(1920f, 1080f, 1f, 1.2f, TestName = "1080p : grossit comme la hauteur")]
        [TestCase(2560f, 1440f, 1f, 1.6f, TestName = "1440p")]
        [TestCase(3840f, 2160f, 1f, 2.4f, TestName = "4K")]
        [TestCase(2560f, 1080f, 1f, 1.2f, TestName = "Écran large 21:9 : comme la hauteur")]
        [TestCase(3440f, 1440f, 1f, 1.6f, TestName = "Écran large 3440 × 1440")]
        [TestCase(1280f, 720f, 1f, 1f, TestName = "720p : jamais sous la taille de référence")]
        [TestCase(1280f, 800f, 1f, 1f, TestName = "Steam Deck : taille de référence")]
        [TestCase(1920f, 1080f, 0.8f, 1f, TestName = "80 % en 1080p : pas sous la référence")]
        [TestCase(3840f, 2160f, 0.8f, 1.92f, TestName = "80 % en 4K")]
        [TestCase(1920f, 1080f, 1.5f, 1.5f, TestName = "150 % en 1080p : plafonné pour garder 1280 × 720")]
        [TestCase(3840f, 2160f, 1.5f, 3f, TestName = "150 % en 4K : plafonné pour garder 1280 × 720")]
        [TestCase(1024f, 768f, 1f, 1f, TestName = "Écran plus petit que 1280 × 720 : la référence l'emporte")]
        public void TheScaleFollowsTheScreenHeight(float width, float height, float size, float expected)
        {
            Assert.AreEqual(expected, InterfaceScale.Resolve(width, height, ReferenceHeight, MinimumRoom, size), 1e-4f);
        }
    }
}
