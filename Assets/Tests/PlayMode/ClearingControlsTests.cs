using System.Collections;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation.Land;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les gestes de l'outil « Retirer », joués par une souris simulée sur les vraies actions du jeu, au-dessus des arbres
    /// d'essai (TestPaintedNature) vus d'une caméra droit au-dessus d'eux. L'outil en main, un clic sur un arbre le
    /// marque ; un glisser tire un cadre, dessiné tant qu'on glisse, et le relâcher marque les deux arbres qu'il prend.
    /// Un clic droit bref pose l'outil de côté ; un glisser du clic droit, laissé à la caméra, ne le pose pas. Sans
    /// l'outil en main, un clic ne marque rien. Un faux panneau d'interface occupe le bas de l'écran.
    /// </summary>
    public sealed class ClearingControlsTests : GestureFixture
    {
        private sealed class NotedFrame : ISelectionFrame
        {
            public bool Shown;
            public int Hidden;

            public void ShowFrame(Vector2 corner, Vector2 opposite)
            {
                Shown = true;
            }

            public void HideFrame()
            {
                Hidden++;
            }
        }

        private readonly List<Object> _created = new List<Object>();
        private TestPaintedNature _painted;
        private TestCampTown _town;
        private Camera _camera;
        private ClearingControls _controls;
        private NotedFrame _frame;

        public override void Setup()
        {
            base.Setup();
            _painted = new TestPaintedNature();
            var instances = new List<int>();
            LogAssert.ignoreFailingMessages = true;
            try
            {
                _town = new TestCampTown(true, _painted.Read(instances));
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
            NatureViews views = _painted.Show(_town.World.Nature, instances, _town.World.Map.Relief);
            _camera = Keep(new GameObject("Caméra de l'outil")).AddComponent<Camera>();
            _camera.transform.SetPositionAndRotation(new Vector3(20f, 40f, 20f), Quaternion.Euler(90f, 0f, 0f));

            var gestures = Keep(ScriptableObject.CreateInstance<GestureSettings>());
            Assert.That(Moved, Is.GreaterThan(gestures.DragThresholdPixels * 2f));
            GameObject holder = Keep(new GameObject("Gestes de test"));
            holder.SetActive(false);
            _controls = holder.AddComponent<ClearingControls>();
#if UNITY_EDITOR
            TestWorld.Fill(_controls, "_actions", Keep(TestActions.CopyOfTheGame()));
            TestWorld.Fill(_controls, "_gestures", gestures);
#endif
            holder.SetActive(true);
            _frame = new NotedFrame();
            var bar = new TestInterface(new Rect(0f, 0f, Screen.width, Screen.height * 0.2f));
            _controls.Bind(new ClearingTool(_town.World, _camera, views), _frame, bar);
        }

        public override void TearDown()
        {
            _controls.enabled = false;
            foreach (Object created in _created)
                Object.Destroy(created);
            _created.Clear();
            _town.Dispose();
            _painted.Dispose();
            base.TearDown();
        }

        private NatureItem First => _town.World.Nature.Find(1);
        private NatureItem Second => _town.World.Nature.Find(2);

        private Vector2 OnScreen(float x, float y, float z)
        {
            return _camera.WorldToScreenPoint(new Vector3(x, y, z));
        }

        private IEnumerator PointAt(Vector2 screenPoint)
        {
            Move(Mouse.position, screenPoint);
            yield return Played();
        }

        [UnityTest]
        public IEnumerator WithTheToolInHandAClickMarksTheTree()
        {
            _controls.Tool.Take();
            yield return PointAt(OnScreen(28f, 1.5f, 22f));
            yield return Click(Mouse.leftButton);
            _town.World.Step();

            Assert.IsTrue(First.IsMarked);
            Assert.IsFalse(Second.IsMarked);
        }

        [UnityTest]
        public IEnumerator ADragDrawsAFrameThenMarksWhatItHolds()
        {
            _controls.Tool.Take();
            yield return PointAt(OnScreen(8f, 0f, 33f));
            Press(Mouse.leftButton);
            yield return Played();
            yield return PointAt(OnScreen(31f, 0f, 19f));
            Assert.IsTrue(_frame.Shown, "Le cadre se dessine pendant qu'on glisse.");
            Release(Mouse.leftButton);
            yield return Played();
            _town.World.Step();

            Assert.AreEqual(1, _frame.Hidden, "Relâché, le cadre s'efface.");
            Assert.IsTrue(First.IsMarked && Second.IsMarked, "Le cadre a pris les deux arbres.");
            Assert.AreSame(Second, _town.World.Nature.Marked[0], "Le plus proche du premier coin d'abord.");
        }

        [UnityTest]
        public IEnumerator ABriefRightClickPutsTheToolAsideButARightDragDoesNot()
        {
            _controls.Tool.Take();
            yield return PointAt(OnScreen(20f, 0f, 30f));
            Press(Mouse.rightButton);
            yield return Played();
            yield return PointAt(OnScreen(20f, 0f, 30f) + new Vector2(Moved, 0f));
            Release(Mouse.rightButton);
            yield return Played();
            Assert.IsTrue(_controls.Tool.IsActive, "Un glisser du clic droit reste à la caméra : l'outil reste en main.");

            yield return Click(Mouse.rightButton);
            Assert.IsFalse(_controls.Tool.IsActive, "Un clic droit bref pose l'outil de côté, comme une carte.");
        }

        [UnityTest]
        public IEnumerator WithoutTheToolAClickMarksNothing()
        {
            yield return PointAt(OnScreen(28f, 1.5f, 22f));
            yield return Click(Mouse.leftButton);
            _town.World.Step();

            Assert.IsFalse(First.IsMarked);
        }

        private T Keep<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }
    }
}
