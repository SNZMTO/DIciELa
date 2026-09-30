using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Simulation.Land;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'outil « Retirer » (décision 161), sur les arbres d'essai (TestPaintedNature) vus d'une caméra droit au-dessus
    /// d'eux : en main, un clic sur le haut d'un arbre le marque, un second le démarque ; un clic sur le sol nu ne fait
    /// rien ; un cadre qui prend les deux arbres les marque tous deux, le plus proche de son premier coin d'abord ; hors de
    /// la main, il ne fait rien ; Échap le lâche, et un second Échap passe au cran suivant.
    /// </summary>
    public sealed class ClearingToolTests
    {
        private TestPaintedNature _painted;
        private TestPileTown _town;
        private Camera _camera;
        private ClearingTool _tool;

        [SetUp]
        public void CreateTool()
        {
            _painted = new TestPaintedNature();
            var instances = new List<int>();
            LogAssert.ignoreFailingMessages = true;
            try
            {
                _town = new TestPileTown(true, _painted.Read(instances));
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
            NatureViews views = _painted.Show(_town.World.Nature, instances, _town.World.Map.Relief);
            _camera = new GameObject("Caméra de l'outil").AddComponent<Camera>();
            _camera.transform.SetPositionAndRotation(new Vector3(20f, 40f, 20f), Quaternion.Euler(90f, 0f, 0f));
            _tool = new ClearingTool(_town.World, _camera, views);
        }

        [TearDown]
        public void DestroyTool()
        {
            Object.Destroy(_camera.gameObject);
            _town.Dispose();
            _painted.Dispose();
        }

        [Test]
        public void AClickMarksATreeAndASecondUnmarksIt()
        {
            NatureItem tree = _town.World.Nature.Find(1);
            Vector2 top = _camera.WorldToScreenPoint(new Vector3(28f, 1.5f, 22f));

            Assert.IsFalse(_tool.TouchAt(top), "Hors de la main, l'outil ne fait rien.");
            _tool.Take();
            Assert.IsTrue(_tool.TouchAt(top));
            _town.World.Step();
            Assert.IsTrue(tree.IsMarked, "Le clic l'a marqué.");

            Assert.IsTrue(_tool.TouchAt(top));
            _town.World.Step();
            Assert.IsFalse(tree.IsMarked, "Un second clic le démarque.");

            Assert.IsFalse(_tool.TouchAt(_camera.WorldToScreenPoint(new Vector3(20f, 0f, 5f))), "Le sol nu.");
        }

        [Test]
        public void AFrameMarksWhatItHoldsFromItsFirstCorner()
        {
            Vector2 nearTheSecond = _camera.WorldToScreenPoint(new Vector3(8f, 0f, 33f));
            Vector2 beyondTheFirst = _camera.WorldToScreenPoint(new Vector3(31f, 0f, 19f));
            Assert.AreEqual(0, _tool.MarkWithin(nearTheSecond, beyondTheFirst), "Hors de la main, rien.");

            _tool.Take();
            Assert.AreEqual(2, _tool.MarkWithin(nearTheSecond, beyondTheFirst));
            _town.World.Step();

            IReadOnlyList<NatureItem> marked = _town.World.Nature.Marked;
            Assert.AreEqual(2, marked.Count);
            Assert.AreEqual(2, marked[0].Id, "L'arbre le plus proche du premier coin est marqué le premier.");
            Assert.AreEqual(1, marked[1].Id);
        }

        [Test]
        public void EscapeDropsTheTool()
        {
            int changes = 0;
            _tool.ActiveChanged += () => changes++;
            _tool.Take();
            Assert.IsTrue(_tool.IsActive);

            Assert.IsTrue(_tool.Escape());
            Assert.IsFalse(_tool.IsActive);
            Assert.IsFalse(_tool.Escape(), "Déjà lâché : Échap passe au cran suivant.");
            Assert.AreEqual(2, changes);
        }
    }
}
