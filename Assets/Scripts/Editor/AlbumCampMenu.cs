using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using Diciela.View;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les mises en scène du campement pour l'album (Docs/ALBUM.md), en Play, par les mêmes appels que les gestes.
    /// L'installer : on se rapproche de la Chimère, puis on tient l'outil de pose à la place de la souris ; le campement
    /// s'installe devant elle, entrée tournée vers la caméra, et les gestes de la caméra et de la pose restent coupés
    /// jusqu'à la fin du Play. Le toucher : comme un clic sur son modèle, qui ouvre sa fiche. Y mettre la Chimère, et
    /// l'envoyer dormir : les ordres mêmes que les boutons de la fiche. Se rapprocher de la couche : des crans de molette
    /// vers là où la Chimère dort, comme le ferait la souris posée sur elle.
    /// </summary>
    public static class AlbumCampMenu
    {
        // Les crans de molette vers la Chimère, le temps laissé à la caméra, en secondes, et la distance du campement, en
        // mètres, devant elle vers le haut de l'écran.
        private const float ZoomNotches = 4f;
        private const double ZoomSeconds = 0.8;
        private const float CampDistance = 6f;

        // Où l'on cherche le modèle du campement pour le toucher : au-dessus de son centre, à ces hauteurs, en mètres
        // (il est bas, et son milieu est un foyer creux).
        private static readonly float[] TouchHeights = { 0.05f, 0.3f, 1f };

        // Les crans de molette vers la couche.
        private const float CouchZoomNotches = 4f;

        [MenuItem("DICIELA/Album/Installer le campement près de la Chimère")]
        public static void InstallTheCamp()
        {
            World world = AlbumWorld.Running();
            Villager chimera = world != null ? world.Villagers.Chimera : null;
            var controls = Object.FindAnyObjectByType<PlacementControls>();
            var cameraControls = Object.FindAnyObjectByType<CameraControls>();
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (chimera == null || controls == null || controls.Tool == null || cameraControls == null || rig == null)
            {
                Debug.LogError("DICIELA : on installe le campement en Play, une fois la Chimère née.");
                return;
            }

            controls.enabled = false;
            cameraControls.enabled = false;
            PlacementTool tool = controls.Tool;
            Camera camera = rig.Camera;
            var camp = EditorAssets.Load<StructureDefinition>(ContentBuilder.CampPath);
            rig.Zoom(ZoomNotches, camera.WorldToScreenPoint(Ground(world, chimera.Position)));

            AlbumMenu.After(ZoomSeconds, () =>
            {
                Vector3 forward = camera.transform.forward;
                Vector2 ahead = new Vector2(forward.x, forward.z).normalized;
                Vector2 center = chimera.Position + ahead * CampDistance;
                tool.Begin(camp);
                // L'angle se compte depuis le nord, vers l'est : l'entrée regarde vers la caméra. L'outil garde l'angle de
                // la pose d'avant : on tourne de ce qui manque.
                float facing = Mathf.Atan2(-ahead.x, -ahead.y) * Mathf.Rad2Deg;
                tool.RotateBy(Mathf.DeltaAngle(tool.RotationDegrees, facing));
                tool.AimAt(camera.WorldToScreenPoint(Ground(world, center)));
                bool sent = tool.Confirm();
                Debug.Log("DICIELA : mise en scène de l'album : le campement " + (sent ? "est installé" : "ne s'installe pas") +
                          " (" + tool.Result + ").");
            });
        }

        [MenuItem("DICIELA/Album/Toucher le campement")]
        public static void TouchTheCamp()
        {
            World world = AlbumWorld.Running();
            Structure home = world != null ? world.Structures.ChimeraHome : null;
            var choose = Object.FindAnyObjectByType<ChooseControls>();
            var views = Object.FindAnyObjectByType<StructureViews>();
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (home == null || choose == null || choose.Tool == null || views == null || rig == null)
            {
                Debug.LogError("DICIELA : on touche le campement en Play, une fois installé.");
                return;
            }

            foreach (float height in TouchHeights)
            {
                Vector3 screen = rig.Camera.WorldToScreenPoint(Ground(world, home.Placement.Center) + Vector3.up * height);
                if (!views.TryPick(rig.Camera.ScreenPointToRay(screen), out Structure picked) ||
                    !ReferenceEquals(picked, home))
                    continue;
                choose.Tool.TouchAt(screen);
                Debug.Log("DICIELA : mise en scène de l'album : le campement est touché, à " + height + " m.");
                return;
            }
            Debug.LogError("DICIELA : la mise en scène ne trouve pas le modèle du campement sous la caméra.");
        }

        [MenuItem("DICIELA/Album/Mettre la Chimère au campement")]
        public static void AssignTheChimera()
        {
            World world = AlbumWorld.Running();
            Structure home = world != null ? world.Structures.ChimeraHome : null;
            Villager chimera = world != null ? world.Villagers.Chimera : null;
            if (home == null || chimera == null)
            {
                Debug.LogError("DICIELA : on met la Chimère au campement en Play, une fois installé.");
                return;
            }
            world.Submit(new AssignVillagerCommand(chimera.Id, home.Id));
            Debug.Log("DICIELA : mise en scène de l'album : la Chimère est mise au campement.");
        }

        [MenuItem("DICIELA/Album/Envoyer la Chimère dormir")]
        public static void SendTheChimeraToBed()
        {
            World world = AlbumWorld.Running();
            if (world == null || world.Villagers.Chimera == null)
            {
                Debug.LogError("DICIELA : on envoie la Chimère dormir en Play, une fois née.");
                return;
            }
            world.Submit(new SendChimeraToBedCommand());
            Debug.Log("DICIELA : mise en scène de l'album : la Chimère est envoyée dormir.");
        }

        [MenuItem("DICIELA/Album/Se rapprocher de la couche")]
        public static void ZoomOnTheCouch()
        {
            World world = AlbumWorld.Running();
            Structure home = world != null ? world.Structures.ChimeraHome : null;
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (home == null || !home.Definition.HasBed || rig == null)
            {
                Debug.LogError("DICIELA : on se rapproche de la couche en Play, une fois le campement installé.");
                return;
            }

            Vector3 couch = ModelPose.PointOf(home.Placement, world.Map.Relief, home.Definition.BedFeet);
            rig.Zoom(CouchZoomNotches, rig.Camera.WorldToScreenPoint(couch));
            Debug.Log("DICIELA : mise en scène de l'album : la vue se rapproche de la couche, en " + couch + ".");
        }

        private static Vector3 Ground(World world, Vector2 at)
        {
            return world.Map.Relief.GroundAt(at);
        }
    }
}
