using System.Collections;
using Diciela.Controls;
using Diciela.Data;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les gestes de l'atelier (GDD, section 6), joués par une souris et un clavier simulés sur les vraies actions du
    /// jeu : tenir E, ou glisser vers la droite au clic gauche, fait tourner la Chimère à venir dans le sens inverse des
    /// aiguilles d'une montre, vue d'en haut (sa face va vers la droite de l'écran, la caméra la regardant de face), et
    /// elle s'arrête avec le geste. Un glisser qui part du panneau lui appartient ; délié, rien ne tourne.
    /// </summary>
    public sealed class PreviewControlsTests : GestureFixture
    {
        // Le panneau de test : la droite de l'écran, en pixels.
        private static readonly Rect Panel = new Rect(600f, 0f, 400f, 800f);

        private InputActionAsset _actions;
        private TestRig _rig;
        private GameObject _holder;
        private AppearancePreview _preview;
        private PreviewControls _controls;

        public override void Setup()
        {
            base.Setup();
            _actions = TestActions.CopyOfTheGame();
            _rig = new TestRig();
            _preview = _rig.Keep(new GameObject("Aperçu de test")).AddComponent<AppearancePreview>();
            _preview.Stand(Vector3.zero);
            _preview.Show(_rig.Plain);
            _holder = new GameObject("Gestes de test");
            _holder.SetActive(false);
            _controls = _holder.AddComponent<PreviewControls>();
#if UNITY_EDITOR
            TestWorld.Fill(_controls, "_actions", _actions);
            TestWorld.Fill(_controls, "_settings", _rig.Keep(ScriptableObject.CreateInstance<CameraSettings>()));
            TestWorld.Fill(_controls, "_gestures", _rig.Keep(ScriptableObject.CreateInstance<GestureSettings>()));
#endif
            _holder.SetActive(true);
            _controls.Bind(_preview, new TestInterface(Panel));
        }

        public override void TearDown()
        {
            _controls.enabled = false;
            Object.DestroyImmediate(_holder);
            Object.DestroyImmediate(_actions);
            _rig.Dispose();
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator HoldingETurnsHerUntilItIsReleased()
        {
            Press(Keyboard.eKey);
            yield return Played();
            yield return Played();
            Release(Keyboard.eKey);
            yield return Played();
            float turned = Yaw();
            Assert.Less(turned, 0f, "Dans le sens inverse des aiguilles d'une montre, vue d'en haut.");
            yield return Played();
            Assert.AreEqual(turned, Yaw(), 1e-4f, "E relâchée, elle ne tourne plus.");
        }

        [UnityTest]
        public IEnumerator DraggingRightOffThePanelTurnsHerTheSameWayButNotFromThePanel()
        {
            yield return Drag(new Vector2(200f, 300f));
            float turned = Yaw();
            Assert.Less(turned, 0f, "Glisser vers la droite la tourne comme E.");
            yield return Drag(new Vector2(Panel.center.x, 300f));
            Assert.AreEqual(turned, Yaw(), 1e-4f, "Un glisser parti du panneau est au panneau.");
        }

        [UnityTest]
        public IEnumerator UnboundNothingTurns()
        {
            _controls.Bind(null, null);
            Press(Keyboard.eKey);
            yield return Played();
            Release(Keyboard.eKey);
            yield return Drag(new Vector2(200f, 300f));
            Assert.AreEqual(0f, Yaw(), 1e-4f);
        }

        // Un glisser au clic gauche, de ce point vers la droite, bien au-delà du seuil.
        private IEnumerator Drag(Vector2 from)
        {
            Move(Mouse.position, from);
            yield return Played();
            Press(Mouse.leftButton);
            yield return Played();
            Move(Mouse.position, from + Vector2.right * Moved);
            yield return Played();
            Release(Mouse.leftButton);
            yield return Played();
        }

        // L'angle de la Chimère montrée, en degrés, entre -180 et 180 : positif dans le sens des aiguilles d'une montre.
        private float Yaw()
        {
            Transform shown = _preview.transform.GetChild(_preview.transform.childCount - 1);
            return Mathf.DeltaAngle(0f, shown.eulerAngles.y);
        }
    }
}
