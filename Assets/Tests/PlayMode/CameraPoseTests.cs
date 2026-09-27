using Diciela.Data;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'inclinaison de la caméra, avec la fiche par défaut (15 à 220 m, 25° à 60°, champ de 40°, marge de 5°, 80° au
    /// plus). Elle s'abaisse doucement : chaque cran de molette (15 % de distance en moins) l'incline d'autant, de près
    /// comme de loin ; à mi-chemin des crans (la moyenne géométrique des distances), elle est à mi-chemin des angles.
    /// </summary>
    public sealed class CameraPoseTests
    {
        private CameraSettings _settings;

        [SetUp]
        public void CreateSettings()
        {
            _settings = ScriptableObject.CreateInstance<CameraSettings>();
#if UNITY_EDITOR
            TestWorld.Fill(_settings, "_nearDistance", 15f);
            TestWorld.Fill(_settings, "_farDistance", 220f);
            TestWorld.Fill(_settings, "_nearPitch", 25f);
            TestWorld.Fill(_settings, "_farPitch", 60f);
            TestWorld.Fill(_settings, "_fieldOfView", 40f);
            TestWorld.Fill(_settings, "_horizonMarginDegrees", 5f);
            TestWorld.Fill(_settings, "_maxPitch", 80f);
#endif
        }

        [TearDown]
        public void DestroySettings()
        {
            Object.DestroyImmediate(_settings);
        }

        private float NotchStepAt(float distance)
        {
            return CameraPose.CurvePitch(_settings, distance) - CameraPose.CurvePitch(_settings, distance * 0.85f);
        }

        [Test]
        public void EveryNotchTiltsTheViewAlike()
        {
            Assert.AreEqual(NotchStepAt(200f), NotchStepAt(20f), 1e-3f);
            Assert.AreEqual(NotchStepAt(200f), NotchStepAt(60f), 1e-3f);
            Assert.AreEqual((25f + 60f) / 2f, CameraPose.CurvePitch(_settings, Mathf.Sqrt(15f * 220f)), 1e-3f);
        }

        [Test]
        public void TheTiltStaysBelowTheHorizonAndAboveTheBirdView()
        {
            Assert.AreEqual(25f, CameraPose.Pitch(_settings, 57f, -90f), 1e-3f);
            Assert.AreEqual(80f, CameraPose.Pitch(_settings, 57f, 90f), 1e-3f);
            Assert.AreEqual(25f, CameraPose.Pitch(_settings, 15f, 0f), 1e-3f);
            Assert.AreEqual(60f, CameraPose.Pitch(_settings, 220f, 0f), 1e-3f);
        }
    }
}
