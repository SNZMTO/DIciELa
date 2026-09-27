using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les ombres et le brouillard qui suivent le zoom, sur la caméra de test (fiche par défaut : ombres de 50 à 300 m,
    /// brouillard pas avant 1,3 fois la distance). Le brouillard de départ est posé par le test : de 100 à 500 m. La
    /// portée des ombres attendue se mesure par un rayon lancé au bord haut de l'écran, pas par le calcul de la vue.
    /// </summary>
    public sealed class CameraAtmosphereTests
    {
        private const float FogStart = 100f;
        private const float FogEnd = 500f;

        private RenderPipelineAsset _pipelineBefore;
        private float _fogStartBefore;
        private float _fogEndBefore;
        private TestCamera _test;

        [SetUp]
        public void CreateCamera()
        {
            _pipelineBefore = GraphicsSettings.currentRenderPipeline;
            _fogStartBefore = RenderSettings.fogStartDistance;
            _fogEndBefore = RenderSettings.fogEndDistance;
            RenderSettings.fogStartDistance = FogStart;
            RenderSettings.fogEndDistance = FogEnd;
            _test = new TestCamera(atmosphere: true);
        }

        [TearDown]
        public void DestroyCamera()
        {
            _test.Dispose();
            RenderSettings.fogStartDistance = _fogStartBefore;
            RenderSettings.fogEndDistance = _fogEndBefore;
        }

        /// <summary>La distance, depuis la caméra, du sol (à 10 m) vu au milieu du bord haut de l'écran.</summary>
        private float GroundAtTheTopOfTheView()
        {
            Ray ray = _test.Camera.ViewportPointToRay(new Vector3(0.5f, 1f, 0f));
            Assert.IsTrue(new Plane(Vector3.up, new Vector3(0f, 10f, 0f)).Raycast(ray, out float along));
            return Vector3.Distance(_test.Holder.transform.position, ray.GetPoint(along));
        }

        [UnityTest]
        public IEnumerator TheShadowsReachTheGroundAtTheTopOfTheView()
        {
            yield return null;

            var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            Assert.IsNotNull(pipeline, "Le projet rend avec URP.");
            Assert.AreNotSame(_pipelineBefore, pipeline, "La caméra règle une copie, jamais l'asset.");
            float expected = Mathf.Clamp(GroundAtTheTopOfTheView(), _test.Settings.ShadowMinMeters,
                _test.Settings.ShadowMaxMeters);
            Assert.AreEqual(expected, pipeline.shadowDistance, 0.6f);
        }

        [UnityTest]
        public IEnumerator TheFogStaysNearbyAndRecedesFromAfar()
        {
            yield return null;
            Assert.AreEqual(FogStart, RenderSettings.fogStartDistance, 1e-3f, "De la distance de départ, le brouillard est celui de la carte.");

            _test.Rig.Zoom(-30f, _test.Camera.pixelRect.center);
            yield return null;
            float far = _test.Settings.FarDistance * _test.Settings.FogStartPerDistance;
            Assert.AreEqual(far, RenderSettings.fogStartDistance, 0.01f);
            Assert.AreEqual(far + (FogEnd - FogStart), RenderSettings.fogEndDistance, 0.01f);
        }

        [UnityTest]
        public IEnumerator EverythingIsGivenBackWhenTheCameraStops()
        {
            yield return null;

            _test.Rig.Hide();
            yield return null;

            Assert.AreSame(_pipelineBefore, GraphicsSettings.currentRenderPipeline);
            Assert.AreEqual(FogStart, RenderSettings.fogStartDistance, 1e-3f);
            Assert.AreEqual(FogEnd, RenderSettings.fogEndDistance, 1e-3f);
        }
    }
}
