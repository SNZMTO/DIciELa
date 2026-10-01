using System.Collections;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le geste « toucher », joué par une souris simulée sur les vraies actions du jeu et le petit monde des tests : la
    /// Chimère au centre, son campement installé en (50 ; 45), une boîte de 2,5 m de haut. Un clic gauche sur le campement la
    /// touche (en jeu, sa fiche s'ouvre) ; un glisser n'est pas un clic ; outil de pose en main, le clic est à lui. Un
    /// faux panneau d'interface occupe le bas de l'écran.
    /// </summary>
    public sealed class ChooseControlsTests : GestureFixture
    {
        private static readonly Vector3 CampTop = new Vector3(50f, 12.5f, 45f);

        private readonly List<Structure> _touched = new List<Structure>();
        private TestWorld _test;
        private ChooseControls _controls;
        private ChooseTool _tool;
        private Hand _hand;

        public override void Setup()
        {
            base.Setup();
            _test = new TestWorld();
            var kind = _test.Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            var gestures = _test.Keep(ScriptableObject.CreateInstance<GestureSettings>());
            Assert.That(Moved, Is.GreaterThan(gestures.DragThresholdPixels * 2f));

            GameObject holder = _test.Keep(new GameObject("Geste de test"));
            holder.SetActive(false);
            _controls = holder.AddComponent<ChooseControls>();
#if UNITY_EDITOR
            TestWorld.Fill(kind, "_id", "chimere");
            TestWorld.Fill(_controls, "_actions", _test.Keep(TestActions.CopyOfTheGame()));
            TestWorld.Fill(_controls, "_gestures", gestures);
#endif
            holder.SetActive(true);
            _test.World.Submit(new CreateChimeraCommand(kind, TestWorld.PlainAppearance(o => _test.Keep(o))));
            _test.World.Submit(new PlaceStructureCommand(_test.Camp, new Placement(new Vector2(50f, 45f), 0f)));
            _test.World.Step();
            var bar = new TestInterface(new Rect(0f, 0f, Screen.width, Screen.height * 0.2f));
            _hand = new Hand(_test.Tool);
            _touched.Clear();
            _tool = new ChooseTool(_test.Camera, _test.Views);
            _tool.Touched += _touched.Add;
            _controls.Bind(_tool, _hand, bar);
        }

        public override void TearDown()
        {
            _controls.enabled = false;
            _hand.Dispose();
            _test.Dispose();
            base.TearDown();
        }

        private bool TheCampIsTouched => _touched.Contains(_test.World.Structures.ChimeraHome);

        private IEnumerator PointAt(Vector2 screenPoint)
        {
            Move(Mouse.position, screenPoint);
            yield return Played();
        }

        [UnityTest]
        public IEnumerator ALeftClickOnTheCampTouchesIt()
        {
            yield return PointAt(_test.OnScreen(CampTop));
            yield return Click(Mouse.leftButton);

            Assert.IsTrue(TheCampIsTouched);
        }

        [UnityTest]
        public IEnumerator ADragIsNotATouch()
        {
            yield return PointAt(_test.OnScreen(CampTop));
            Press(Mouse.leftButton);
            yield return Played();
            yield return PointAt(_test.OnScreen(CampTop) + new Vector2(Moved, 0f));
            Release(Mouse.leftButton);
            yield return Played();

            Assert.IsEmpty(_touched);
        }

        [UnityTest]
        public IEnumerator WithAStructureInHandTheClickIsNotATouch()
        {
            _test.Tool.Begin(_test.Cabin);
            yield return PointAt(_test.OnScreen(CampTop));
            yield return Click(Mouse.leftButton);

            Assert.IsEmpty(_touched);
        }
    }
}
