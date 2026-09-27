using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.View;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les mises en scène de l'album (Docs/ALBUM.md), en Play : on se rapproche du centre de la vue, puis on tient
    /// l'outil de pose à la place de la souris, par les mêmes appels que les gestes, le temps d'une capture. Les gestes
    /// de la caméra et de la pose restent coupés jusqu'à la fin du Play.
    /// </summary>
    public static class AlbumMenu
    {
        // Les crans de molette vers le centre, puis les temps laissés à la caméra et au monde (par pose), en secondes.
        private const float ZoomNotches = 2f;
        private const double ZoomSeconds = 0.8;
        private const double StepsSeconds = 0.5;

        // Trois places, en fractions de l'écran, pour trois cabanes ; une place libre devant elles.
        private static readonly Vector2[] Places =
        {
            new Vector2(0.36f, 0.5f), new Vector2(0.5f, 0.58f), new Vector2(0.64f, 0.5f)
        };
        private static readonly Vector2 FreePlace = new Vector2(0.5f, 0.34f);

        [MenuItem("DICIELA/Album/Poser trois cabanes, viser une place libre")]
        public static void StageFreePlace()
        {
            Stage(FreePlace);
        }

        [MenuItem("DICIELA/Album/Poser trois cabanes, viser une place prise")]
        public static void StageTakenPlace()
        {
            Stage(Places[1]);
        }

        private static void Stage(Vector2 aimFraction)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("DICIELA : les mises en scène de l'album se font en Play.");
                return;
            }
            var controls = Object.FindAnyObjectByType<PlacementControls>();
            var cameraControls = Object.FindAnyObjectByType<CameraControls>();
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (controls == null || controls.Tool == null || cameraControls == null || rig == null)
            {
                Debug.LogError("DICIELA : le monde n'est pas encore né.");
                return;
            }

            // La souris réelle ne vise plus et ne fait plus défiler : la mise en scène tient les gestes.
            controls.enabled = false;
            cameraControls.enabled = false;
            PlacementTool tool = controls.Tool;
            Camera camera = rig.Camera;
            StructureDefinition cabin = EditorAssets.Load<Catalog>(ContentBuilder.CatalogPath).Families[0].Structures[0];
            rig.Zoom(ZoomNotches, OnScreen(camera, new Vector2(0.5f, 0.5f)));

            // Une pose par temps laissé au monde : l'outil n'envoie pas d'ordre tant que le précédent n'est pas joué.
            After(ZoomSeconds, () => tool.Begin(cabin));
            for (int i = 0; i < Places.Length; i++)
            {
                Vector2 place = Places[i];
                After(ZoomSeconds + i * StepsSeconds, () =>
                {
                    tool.AimAt(OnScreen(camera, place));
                    tool.Confirm();
                    tool.RotateStep();
                    tool.RotateStep();
                });
            }
            After(ZoomSeconds + Places.Length * StepsSeconds, () =>
            {
                tool.AimAt(OnScreen(camera, aimFraction));
                Debug.Log("DICIELA : mise en scène de l'album prête (" + tool.Result + ").");
            });
        }

        /// <summary>Le point de l'écran à cette fraction de la vue, calculé par la caméra, qui connaît sa part d'écran.</summary>
        private static Vector2 OnScreen(Camera camera, Vector2 fraction)
        {
            return camera.ViewportToScreenPoint(fraction);
        }

        /// <summary>Joue ce geste dans ce nombre de secondes, si le Play dure encore.</summary>
        private static void After(double seconds, Action gesture)
        {
            double at = EditorApplication.timeSinceStartup + seconds;
            void WhenDue()
            {
                if (EditorApplication.timeSinceStartup < at)
                    return;
                EditorApplication.update -= WhenDue;
                if (EditorApplication.isPlaying)
                    gesture();
            }
            EditorApplication.update += WhenDue;
        }
    }
}
