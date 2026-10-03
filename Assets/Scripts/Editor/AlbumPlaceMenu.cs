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
    /// La mise en scène de la Place pour l'album (Docs/ALBUM.md ; décisions 192 et 193), en Play, une fois le campement
    /// installé, par les mêmes appels que les gestes : l'outil de pose prend la Place et la pose à côté du campement, sur
    /// sa droite vue de la caméra, puis la vue la tire au centre de l'écran comme par un glisser et s'en rapproche, assez
    /// pour la voir entière. Bâtie ou en chantier, elle se regarde ensuite par le même menu.
    /// </summary>
    public static class AlbumPlaceMenu
    {
        // L'écart entre le milieu du campement et celui de la Place, et la distance de la caméra, en mètres.
        private const float PlaceDistance = 10f;
        private const float FramingMeters = 26f;

        [MenuItem("DICIELA/Album/Poser la Place près du campement")]
        public static void PlaceThePlace()
        {
            World world = AlbumWorld.Running();
            Structure camp = world != null ? world.Structures.ChimeraHome : null;
            var controls = Object.FindAnyObjectByType<PlacementControls>();
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (camp == null || controls == null || controls.Tool == null || rig == null)
            {
                Debug.LogError("DICIELA : on pose la Place en Play, une fois le campement installé.");
                return;
            }

            Camera camera = rig.Camera;
            Vector3 right = camera.transform.right;
            Vector2 center = camp.Placement.Center + new Vector2(right.x, right.z).normalized * PlaceDistance;
            PlacementTool tool = controls.Tool;
            tool.Begin(EditorAssets.Load<StructureDefinition>(ContentBuilder.PlacePath));
            tool.AimAt(camera.WorldToScreenPoint(world.Map.Relief.GroundAt(center)));
            bool sent = tool.Confirm();
            Debug.Log("DICIELA : mise en scène de l'album : la Place " + (sent ? "est posée" : "ne se pose pas") + " (" +
                      tool.Result + ").");
            LookAtThePlace();
        }

        [MenuItem("DICIELA/Album/Regarder la Place")]
        public static void LookAtThePlace()
        {
            World world = AlbumWorld.Running();
            var place = EditorAssets.Load<StructureDefinition>(ContentBuilder.PlacePath);
            Structure placed = null;
            if (world != null)
            {
                foreach (Structure structure in world.Structures.All)
                {
                    if (ReferenceEquals(structure.Definition, place))
                        placed = structure;
                }
            }
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (rig == null || world == null)
            {
                Debug.LogError("DICIELA : on regarde la Place en Play.");
                return;
            }
            // La Place posée à ce pas n'existe qu'au pas suivant : on regarde alors là où elle va.
            Vector2 center = placed != null ? placed.Placement.Center : AimedCenter(world, rig);
            var middle = new Vector2(rig.Camera.pixelWidth / 2f, rig.Camera.pixelHeight / 2f);
            if (rig.BeginDrag(rig.Camera.WorldToScreenPoint(world.Map.Relief.GroundAt(center))))
                rig.DragTo(middle);
            rig.ZoomTo(FramingMeters);
        }

        // Le milieu de la Place qu'on vient de poser à côté du campement, avant qu'elle n'existe au pas suivant.
        private static Vector2 AimedCenter(World world, CameraRig rig)
        {
            Structure camp = world.Structures.ChimeraHome;
            if (camp == null)
                return Vector2.zero;
            Vector3 right = rig.Camera.transform.right;
            return camp.Placement.Center + new Vector2(right.x, right.z).normalized * PlaceDistance;
        }
    }
}
