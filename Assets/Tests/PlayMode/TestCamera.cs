using System;
using Diciela.Data;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Une caméra de gestion pour les tests, sur une carte plate de 100 × 100 m à 10 m de hauteur. Sa fiche a ses valeurs
    /// par défaut, remplie par SerializedObject, le chemin de l'Inspector ; le lissage du zoom est coupé, sauf demande,
    /// pour lire le résultat tout de suite. Avec ses ombres et son brouillard, sur demande.
    /// </summary>
    internal sealed class TestCamera : IDisposable
    {
        public static readonly Vector3 MapCenter = new Vector3(50f, 10f, 50f);

        public TestCamera(bool smoothZoom = false, bool atmosphere = false)
        {
            Settings = ScriptableObject.CreateInstance<CameraSettings>();
            Holder = new GameObject("Caméra de test", typeof(Camera));
            Rig = Holder.AddComponent<CameraRig>();
            if (atmosphere)
                Atmosphere = Holder.AddComponent<CameraAtmosphere>();
#if UNITY_EDITOR
            if (!smoothZoom)
                TestWorld.Fill(Settings, "_zoomSmoothing", 0f);
            TestWorld.Fill(Rig, "_settings", Settings);
            if (atmosphere)
                TestWorld.Fill(Atmosphere, "_settings", Settings);
#endif
            Rig.Show(new Relief(100f, 100f, 2, 2, new[] { 10f, 10f, 10f, 10f }));
        }

        public CameraSettings Settings { get; }
        public GameObject Holder { get; }
        public CameraRig Rig { get; }
        public CameraAtmosphere Atmosphere { get; }
        public Camera Camera => Rig.Camera;

        /// <summary>Le point de l'écran où l'on voit ce point du sol.</summary>
        public Vector2 OnScreen(Vector3 ground)
        {
            return Camera.WorldToScreenPoint(ground);
        }

        /// <summary>Le point du sol (à 10 m) que la caméra regarde au centre de l'écran.</summary>
        public Vector3 LookedAtGround()
        {
            Transform view = Holder.transform;
            float along = (10f - view.position.y) / view.forward.y;
            return view.position + view.forward * along;
        }

        public void Dispose()
        {
            Object.DestroyImmediate(Holder);
            Object.DestroyImmediate(Settings);
        }
    }
}
