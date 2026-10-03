using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La mise en scène du déplacement pour l'album (Docs/ALBUM.md ; décisions 199 à 201), en Play, une fois la Place
    /// bâtie, par les mêmes appels que les gestes : l'outil Déplacer en main, on touche la Place en visant son modèle, son
    /// fantôme suit le pointeur jusqu'à quatre mètres plus loin, à droite vue de la caméra, et un clic l'y envoie.
    /// </summary>
    public static class AlbumMoveMenu
    {
        // De combien on la déplace, en mètres, et à quelle hauteur on vise son modèle.
        private const float Shift = 4f;
        private const float AimHeight = 0.1f;

        [MenuItem("DICIELA/Album/Déplacer la Place de quatre mètres")]
        public static void MoveThePlace()
        {
            World world = AlbumWorld.Running();
            var controls = Object.FindAnyObjectByType<PlacementControls>();
            var rig = Object.FindAnyObjectByType<CameraRig>();
            Structure place = world != null ? PlaceOf(world) : null;
            if (place == null || controls == null || controls.Mover == null || rig == null)
            {
                Debug.LogError("DICIELA : on déplace la Place en Play, une fois posée.");
                return;
            }

            Camera camera = rig.Camera;
            Relief relief = world.Map.Relief;
            MoveTool mover = controls.Mover;
            mover.Take();
            mover.AimAt(camera.WorldToScreenPoint(relief.GroundAt(place.Placement.Center) + Vector3.up * AimHeight));
            bool picked = mover.Confirm();
            Vector3 right = camera.transform.right;
            Vector2 target = place.Placement.Center + new Vector2(right.x, right.z).normalized * Shift;
            mover.AimAt(camera.WorldToScreenPoint(relief.GroundAt(target)));
            bool sent = picked && mover.Confirm();
            Debug.Log("DICIELA : mise en scène de l'album : la Place " + (sent ? "part" : "ne part pas") + " (touchée : " +
                      picked + ", " + mover.Result + ").");
        }

        private static Structure PlaceOf(World world)
        {
            var definition = EditorAssets.Load<StructureDefinition>(ContentBuilder.PlacePath);
            foreach (Structure structure in world.Structures.All)
            {
                if (ReferenceEquals(structure.Definition, definition))
                    return structure;
            }
            return null;
        }
    }
}
