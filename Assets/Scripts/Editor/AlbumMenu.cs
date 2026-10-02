using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Game;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using Diciela.View;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les mises en scène de l'album (Docs/ALBUM.md), en Play. Poser trois cabanes : on se rapproche du centre de la vue,
    /// puis on tient l'outil de pose à la place de la souris, par les mêmes appels que les gestes, le temps d'une
    /// capture ; les cabanes vont autour du logis de la Chimère (ou d'elle), à gauche, derrière et à droite vu de la
    /// caméra, loin du campement ; les gestes de la caméra et de la pose restent coupés jusqu'à la fin du Play. Se rapprocher de la
    /// Chimère : des crans de molette vers là où on la voit, comme le ferait la souris posée sur elle. Tourner la Chimère
    /// de l'atelier : de trois quarts, par le même appel que le glisser.
    /// </summary>
    public static class AlbumMenu
    {
        // Les crans de molette vers le centre, puis les temps laissés à la caméra et au monde (par pose), en secondes.
        private const float ZoomNotches = 2f;
        private const double ZoomSeconds = 0.8;
        private const double StepsSeconds = 0.5;

        // Les crans de molette vers la Chimère : assez pour la voir presque de face.
        private const float ChimeraZoomNotches = 7f;

        // De trois quarts, sa face vers la droite de l'écran (comme un glisser vers la droite).
        private const float ThreeQuarterDegrees = -35f;

        // Trois places pour trois cabanes, en mètres autour du logis de la Chimère, vues de la caméra (x à droite de
        // l'écran, y vers le fond) : à gauche, derrière, à droite ; une place libre devant.
        private static readonly Vector2[] Places =
        {
            new Vector2(-8f, 2f), new Vector2(0f, 9f), new Vector2(8f, 2f)
        };
        private static readonly Vector2 FreePlace = new Vector2(0f, -10f);

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

        [MenuItem("DICIELA/Album/Se rapprocher de la Chimère")]
        public static void ZoomOnChimera()
        {
            var runner = Object.FindAnyObjectByType<GameRunner>();
            World world = runner != null ? runner.World : null;
            Villager chimera = world != null ? world.Villagers.Chimera : null;
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (!EditorApplication.isPlaying || chimera == null || rig == null)
            {
                Debug.LogError("DICIELA : on se rapproche de la Chimère en Play, une fois qu'elle est née.");
                return;
            }

            PutPlacementToolAside();
            Vector2 at = chimera.Position;
            var ground = world.Map.Relief.GroundAt(at);
            rig.Zoom(ChimeraZoomNotches, rig.Camera.WorldToScreenPoint(ground));
            Debug.Log("DICIELA : la vue se rapproche de la Chimère, en " + at + ".");
        }

        [MenuItem("DICIELA/Album/Tourner la Chimère de l'atelier")]
        public static void TurnChimeraInAtelier()
        {
            var preview = Object.FindAnyObjectByType<AppearancePreview>();
            if (!EditorApplication.isPlaying || preview == null || preview.transform.childCount == 0)
            {
                Debug.LogError("DICIELA : on tourne la Chimère en Play, à l'écran de création.");
                return;
            }
            preview.Turn(ThreeQuarterDegrees);
        }

        /// <summary>Pose de côté l'outil de pose, comme par Échap : son aperçu ne cache plus ce qu'on veut montrer.</summary>
        internal static void PutPlacementToolAside()
        {
            var controls = Object.FindAnyObjectByType<PlacementControls>();
            if (controls != null && controls.Tool != null)
                controls.Tool.Cancel();
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
            var runner = Object.FindAnyObjectByType<GameRunner>();
            if (controls == null || controls.Tool == null || cameraControls == null || rig == null || runner == null ||
                runner.World == null || runner.World.Villagers.Chimera == null)
            {
                Debug.LogError("DICIELA : le monde ou la Chimère n'est pas encore né.");
                return;
            }

            // La souris réelle ne vise plus et ne fait plus défiler : la mise en scène tient les gestes.
            controls.enabled = false;
            cameraControls.enabled = false;
            PlacementTool tool = controls.Tool;
            Camera camera = rig.Camera;
            var cabin = EditorAssets.Load<StructureDefinition>(ContentBuilder.CabinPath);
            rig.Zoom(ZoomNotches, camera.ViewportToScreenPoint(new Vector2(0.5f, 0.5f)));

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

        /// <summary>
        /// Le point de l'écran où l'on voit cette place, en mètres autour du logis de la Chimère (ou d'elle, sans logis),
        /// vue de la caméra : x vers la droite de l'écran, y vers le fond.
        /// </summary>
        private static Vector2 OnScreen(Camera camera, Vector2 around)
        {
            World world = Object.FindAnyObjectByType<GameRunner>().World;
            Structure home = world.Structures.ChimeraHome;
            Vector2 anchor = home != null ? home.Placement.Center : world.Villagers.Chimera.Position;
            Vector3 right = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;
            Vector3 ahead = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
            Vector3 flat = right * around.x + ahead * around.y;
            Vector2 at = anchor + new Vector2(flat.x, flat.z);
            return camera.WorldToScreenPoint(world.Map.Relief.GroundAt(at));
        }

        /// <summary>Joue ce geste dans ce nombre de secondes, si le Play dure encore.</summary>
        internal static void After(double seconds, Action gesture)
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
