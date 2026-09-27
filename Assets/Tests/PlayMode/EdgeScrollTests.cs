using Diciela.Controls;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le défilement au bord, dans une fenêtre de 1000 × 600 pixels avec une bande de 12 pixels. Au lancement, le
    /// pointeur vaut (0 ; 0) tant qu'il n'est pas entré dans la vue : un coin, qui ne doit pas faire filer la vue.
    /// </summary>
    public sealed class EdgeScrollTests
    {
        private static readonly Vector2 Window = new Vector2(1000f, 600f);
        private const float Margin = 12f;

        [Test]
        public void APointerStillInTheCornerAtLaunchDoesNotScroll()
        {
            var edge = new EdgeScroll();

            Assert.AreEqual(Vector2.zero, edge.Update(Vector2.zero, Window, Margin, true));
            Assert.AreEqual(Vector2.zero, edge.Update(Vector2.zero, Window, Margin, true));
        }

        [Test]
        public void OnceThePointerHasComeInTheEdgesScroll()
        {
            var edge = new EdgeScroll();

            Assert.AreEqual(Vector2.zero, edge.Update(new Vector2(500f, 300f), Window, Margin, true));

            Assert.AreEqual(new Vector2(-1f, 0f), edge.Update(new Vector2(5f, 300f), Window, Margin, true));
            Assert.AreEqual(new Vector2(1f, 1f), edge.Update(new Vector2(995f, 595f), Window, Margin, true));
            Assert.AreEqual(Vector2.zero, edge.Update(new Vector2(1200f, 300f), Window, Margin, true));
        }

        [Test]
        public void LosingTheFocusDisarmsIt()
        {
            var edge = new EdgeScroll();
            edge.Update(new Vector2(500f, 300f), Window, Margin, true);

            Assert.AreEqual(Vector2.zero, edge.Update(new Vector2(5f, 300f), Window, Margin, false));
            Assert.AreEqual(Vector2.zero, edge.Update(new Vector2(5f, 300f), Window, Margin, true));
        }
    }
}
