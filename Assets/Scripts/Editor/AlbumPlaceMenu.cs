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
    /// sa droite vue de la caméra, à un mètre de lui, puis la vue la tire au centre de l'écran comme par un glisser et s'en
    /// rapproche, assez pour la voir entière. Bâtie ou en chantier, elle se regarde ensuite par le même menu.
    /// </summary>
    public static class AlbumPlaceMenu
    {
        // Le jour entre le campement et la Place, en mètres ; la distance de la caméra, par mètre de largeur de la Place.
        private const float GapMeters = 1f;
        private const float FramingPerWidth = 2.2f;

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
            Vector2 center = AimedCenter(world, rig);
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
            rig.ZoomTo(place.WidthMeters * FramingPerWidth);
        }

        // Le milieu de la Place posée à côté du campement, sur sa droite vue de la caméra : assez loin que leurs empreintes,
        // des carrés droits, laissent un jour entre elles, quel que soit l'angle de la vue (l'écart se compte sur l'axe que
        // la direction suit le plus).
        private static Vector2 AimedCenter(World world, CameraRig rig)
        {
            Structure camp = world.Structures.ChimeraHome;
            if (camp == null)
                return Vector2.zero;
            var place = EditorAssets.Load<StructureDefinition>(ContentBuilder.PlacePath);
            Vector3 right = rig.Camera.transform.right;
            Vector2 direction = new Vector2(right.x, right.z).normalized;
            float halves = (Mathf.Max(camp.Definition.WidthMeters, camp.Definition.DepthMeters) + place.WidthMeters) * 0.5f;
            float along = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
            return camp.Placement.Center + direction * (halves / along + GapMeters);
        }
    }
}
