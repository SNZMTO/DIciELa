using System.Collections;
using Diciela.Data;
using Diciela.Simulation.Land;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La caméra de gestion, sur une carte plate de 100 × 100 m à 10 m de hauteur. Les gestes l'appellent comme le
    /// joueur ; les fiches ont leurs valeurs par défaut, sauf le lissage du zoom, coupé pour lire le résultat tout de
    /// suite (réglé par l'Inspector, comme le ferait le développeur).
    /// </summary>
    public sealed class CameraRigTests
    {
        private static readonly Vector3 MapCenter = new Vector3(50f, 10f, 50f);

        private GameObject _cameraObject;
        private CameraSettings _settings;
        private CameraRig _rig;

        [SetUp]
        public void CreateCamera()
        {
            _settings = ScriptableObject.CreateInstance<CameraSettings>();
            _cameraObject = new GameObject("Caméra de test", typeof(Camera));
            _rig = _cameraObject.AddComponent<CameraRig>();
#if UNITY_EDITOR
            var settings = new UnityEditor.SerializedObject(_settings);
            settings.FindProperty("_zoomSmoothing").floatValue = 0f;
            settings.ApplyModifiedPropertiesWithoutUndo();
            var rig = new UnityEditor.SerializedObject(_rig);
            rig.FindProperty("_settings").objectReferenceValue = _settings;
            rig.ApplyModifiedPropertiesWithoutUndo();
#endif
            var heights = new float[] { 10f, 10f, 10f, 10f };
            _rig.Show(new Relief(100f, 100f, 2, 2, heights));
        }

        [TearDown]
        public void DestroyCamera()
        {
            Object.DestroyImmediate(_cameraObject);
            Object.DestroyImmediate(_settings);
        }

        /// <summary>Le point du sol (à 10 m) que la caméra regarde au centre de l'écran.</summary>
        private Vector3 LookedAtGround()
        {
            Transform view = _cameraObject.transform;
            float along = (10f - view.position.y) / view.forward.y;
            return view.position + view.forward * along;
        }

        [Test]
        public void ShowsTheMapFromItsCenter()
        {
            Assert.AreEqual(_settings.FieldOfView, _cameraObject.GetComponent<Camera>().fieldOfView, 1e-4f);
            Assert.That(Vector3.Distance(MapCenter, LookedAtGround()), Is.LessThan(0.01f));
            Assert.That(_cameraObject.transform.position.y, Is.GreaterThan(10f));
        }

        [UnityTest]
        public IEnumerator ZoomingInComesCloserAndLower()
        {
            float distanceBefore = Vector3.Distance(_cameraObject.transform.position, MapCenter);
            float pitchBefore = _cameraObject.transform.eulerAngles.x;

            _rig.Zoom(3f);
            yield return null;

            Assert.That(Vector3.Distance(_cameraObject.transform.position, MapCenter), Is.LessThan(distanceBefore));
            Assert.That(_cameraObject.transform.eulerAngles.x, Is.LessThan(pitchBefore));
            Assert.That(Vector3.Distance(MapCenter, LookedAtGround()), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator RotatingTurnsAroundTheSamePoint()
        {
            _rig.Rotate(90f);
            yield return null;

            Assert.AreEqual(90f, _cameraObject.transform.eulerAngles.y, 0.01f);
            Assert.That(Vector3.Distance(MapCenter, LookedAtGround()), Is.LessThan(0.01f));
        }

        /// <summary>
        /// On tire le sol : le point saisi reste sous le pointeur. L'attendu vient de la projection inverse
        /// (WorldToScreenPoint), pas du rayon qui sert à la caméra.
        /// </summary>
        [UnityTest]
        public IEnumerator DraggingKeepsTheGrabbedGroundUnderThePointer()
        {
            Camera camera = _cameraObject.GetComponent<Camera>();
            var grabbed = new Vector3(45f, 10f, 56f);
            Vector2 pressed = camera.WorldToScreenPoint(grabbed);
            Vector2 released = pressed + new Vector2(80f, -60f);

            Assert.IsTrue(_rig.BeginDrag(pressed));
            _rig.DragTo(released);
            yield return null;

            Vector2 nowOnScreen = camera.WorldToScreenPoint(grabbed);
            Assert.That(Vector2.Distance(released, nowOnScreen), Is.LessThan(1f));
        }

        [Test]
        public void TheSkyCannotBeGrabbed()
        {
            var farAboveTheScreen = new Vector2(Screen.width * 0.5f, Screen.height * 3f);

            Assert.IsFalse(_rig.BeginDrag(farAboveTheScreen));
        }

        [UnityTest]
        public IEnumerator PanningNeverLeavesTheMap()
        {
            _rig.Pan(new Vector2(1f, 1f), 1000f);
            yield return null;

            Vector3 ground = LookedAtGround();
            Assert.AreEqual(100f, ground.x, 0.01f);
            Assert.AreEqual(100f, ground.z, 0.01f);
        }
    }
}
