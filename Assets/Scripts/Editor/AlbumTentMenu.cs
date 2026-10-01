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
    /// Les mises en scène du campement (le logis de départ, d'identifiant « tente ») pour l'album (Docs/ALBUM.md), en
    /// Play, par les mêmes appels que les gestes. L'installer : on se rapproche de la Chimère, puis on tient l'outil de
    /// pose à la place de la souris ; le campement s'installe devant elle, entrée tournée vers la caméra, et les gestes de
    /// la caméra et de la pose restent coupés jusqu'à la fin du Play. Le toucher : comme un clic sur lui, qui envoie la
    /// Chimère dormir ou la réveille.
    /// </summary>
    public static class AlbumTentMenu
    {
        // Les crans de molette vers la Chimère, le temps laissé à la caméra, en secondes, et la distance de la tente, en
        // mètres, devant elle vers le haut de l'écran.
        private const float ZoomNotches = 4f;
        private const double ZoomSeconds = 0.8;
        private const float TentDistance = 6f;

        // Où l'on vise la tente pour la toucher : à cette hauteur au-dessus de son centre, en mètres.
        private const float TouchHeight = 1f;

        [MenuItem("DICIELA/Album/Installer le campement près de la Chimère")]
        public static void PlantTheTent()
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
            var tent = EditorAssets.Load<StructureDefinition>(ContentBuilder.TentPath);
            rig.Zoom(ZoomNotches, camera.WorldToScreenPoint(Ground(world, chimera.Position)));

            AlbumMenu.After(ZoomSeconds, () =>
            {
                Vector3 forward = camera.transform.forward;
                Vector2 ahead = new Vector2(forward.x, forward.z).normalized;
                Vector2 center = chimera.Position + ahead * TentDistance;
                tool.Begin(tent);
                // L'angle se compte depuis le nord, vers l'est : la porte regarde vers la caméra.
                tool.RotateBy(Mathf.Atan2(-ahead.x, -ahead.y) * Mathf.Rad2Deg);
                tool.AimAt(camera.WorldToScreenPoint(Ground(world, center)));
                bool sent = tool.Confirm();
                Debug.Log("DICIELA : mise en scène de l'album : le campement " + (sent ? "est installé" : "ne s'installe pas") +
                          " (" + tool.Result + ").");
            });
        }

        [MenuItem("DICIELA/Album/Toucher le campement")]
        public static void TouchTheTent()
        {
            World world = AlbumWorld.Running();
            Structure home = world != null ? world.Structures.ChimeraHome : null;
            var choose = Object.FindAnyObjectByType<ChooseControls>();
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (home == null || choose == null || choose.Tool == null || rig == null)
            {
                Debug.LogError("DICIELA : on touche la tente en Play, une fois plantée.");
                return;
            }

            Vector3 top = Ground(world, home.Placement.Center) + Vector3.up * TouchHeight;
            bool sent = choose.Tool.TouchAt(rig.Camera.WorldToScreenPoint(top));
            Debug.Log("DICIELA : mise en scène de l'album : le campement est touché (" +
                      (sent ? "un ordre part" : "aucun ordre") + ").");
        }

        private static Vector3 Ground(World world, Vector2 at)
        {
            return new Vector3(at.x, world.Map.Relief.HeightAt(at.x, at.y), at.y);
        }
    }
}
