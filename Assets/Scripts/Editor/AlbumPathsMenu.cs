using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using Diciela.View;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les mises en scène des chemins pour l'album (Docs/ALBUM.md ; décisions 232 et 233), en Play, le campement
    /// installé, par les mêmes appels que les gestes. Tracer un chemin de terre : la vue se tourne vers le devant du
    /// campement, l'outil Chemins se prend comme par sa touche, un coup de pinceau de terre part de l'entrée vers la
    /// caméra, droit le long d'un axe de la carte (la vue de départ les suit), puis en diagonale, ses bouts en rond ; puis
    /// l'outil se lâche, et le HUD est au repos. Regarder le chemin de près : son coude et sa diagonale au milieu de
    /// l'écran, de près et de plus haut, pour juger son fondu dans l'herbe ; le journal dit combien de ses mailles sont
    /// bâties. Les gestes de la caméra et des sols restent coupés jusqu'à la fin du Play, pour que la souris ne bouge rien.
    /// </summary>
    public static class AlbumPathsMenu
    {
        // Le chemin, en mètres autour du milieu du campement, vu de la caméra (x vers la droite de l'écran, y vers le
        // fond ; l'entrée du campement est tournée vers elle) : de l'entrée vers la caméra, puis en diagonale vers la
        // droite, loin des cabanes de l'album (AlbumMenu).
        private static readonly Vector2[] Stroke = { new Vector2(0f, -3.2f), new Vector2(0f, -8f), new Vector2(4f, -12f) };

        // Le point visé pour le tracé (le campement, ses cabanes et le chemin dans le cadre) et la distance de la caméra ;
        // le temps laissé à la caméra, en secondes.
        private static readonly Vector2 FramingCenter = new Vector2(0f, -2.5f);
        private const float FramingMeters = 30f;
        private const double CameraSeconds = 0.8;

        // Le gros plan : la diagonale, du coude au bout, la caméra de près, redressée de tant de degrés vers le haut.
        private static readonly Vector2 CloseCenter = new Vector2(2.5f, -10.8f);
        private const float CloseMeters = 10f;
        private const float CloseTiltDegrees = 30f;

        // Le pas, en mètres, auquel on relit les mailles sous le milieu du chemin.
        private const float SampleMeters = 0.25f;

        [MenuItem("DICIELA/Album/Tracer un chemin de terre devant le campement")]
        public static void TraceADirtPath()
        {
            World world = AlbumWorld.Running();
            Structure home = world != null ? world.Structures.ChimeraHome : null;
            var wheel = Object.FindAnyObjectByType<ToolWheel>();
            var controls = Object.FindAnyObjectByType<PaintControls>();
            var cameraControls = Object.FindAnyObjectByType<CameraControls>();
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (home == null || wheel == null || controls == null || cameraControls == null || rig == null)
            {
                Debug.LogError("DICIELA : on trace un chemin en Play, une fois le campement installé.");
                return;
            }

            // La souris réelle ne trace plus et ne fait plus défiler : la mise en scène tient les gestes.
            controls.enabled = false;
            cameraControls.enabled = false;
            Vector2 center = home.Placement.Center;
            Camera camera = rig.Camera;
            var middle = new Vector2(camera.pixelWidth / 2f, camera.pixelHeight / 2f);
            if (rig.BeginDrag(OnScreen(world, camera, center, FramingCenter)))
                rig.DragTo(middle);
            rig.ZoomTo(FramingMeters);

            AlbumMenu.After(CameraSeconds, () =>
            {
                wheel.Choose(ToolChoice.Paths);
                PaintTool tool = controls.Tool;
                if (tool == null || tool.Entry != PaintEntry.Paths)
                {
                    Debug.LogError("DICIELA : la mise en scène ne tient pas l'outil Chemins en main.");
                    return;
                }
                tool.Choose(PaintToolMode.Brush);
                tool.ChooseInk(PaintInk.Of(EditorAssets.Load<FloorDefinition>(FloorContentBuilder.DirtPath)));
                tool.BeginDrawing(OnScreen(world, camera, center, Stroke[0]));
                for (int i = 1; i < Stroke.Length; i++)
                    tool.AimAt(OnScreen(world, camera, center, Stroke[i]));
                PaintAnswer answer = tool.Answer;
                tool.EndDrawing();
                wheel.Choose(ToolChoice.Paths);
                Debug.Log("DICIELA : mise en scène de l'album : un chemin de terre " +
                          (answer.IsOk ? "tracé" : "refusé (" + answer.ReasonKey + ")") + ".");
            });
        }

        [MenuItem("DICIELA/Album/Regarder le chemin de terre de près")]
        public static void LookCloseAtThePath()
        {
            World world = AlbumWorld.Running();
            Structure home = world != null ? world.Structures.ChimeraHome : null;
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (home == null || rig == null)
            {
                Debug.LogError("DICIELA : on regarde le chemin en Play, une fois le campement installé.");
                return;
            }

            Vector2 center = home.Placement.Center;
            Camera camera = rig.Camera;
            var middle = new Vector2(camera.pixelWidth / 2f, camera.pixelHeight / 2f);
            if (rig.BeginDrag(OnScreen(world, camera, center, CloseCenter)))
                rig.DragTo(middle);
            rig.ZoomTo(CloseMeters);
            rig.Tilt(-CloseTiltDegrees);
            Debug.Log("DICIELA : mise en scène de l'album : la vue regarde le chemin de près ; " +
                      Describe(world, camera, center) + ".");
        }

        // Combien de mailles sous le milieu du chemin sont bâties, et combien attendent encore en fantôme. La caméra n'a
        // pas tourné depuis le tracé : ses gestes sont coupés.
        private static string Describe(World world, Camera camera, Vector2 center)
        {
            var cells = new HashSet<Vector2Int>();
            for (int i = 1; i < Stroke.Length; i++)
            {
                Vector2 from = AlbumMenu.Around(camera, center, Stroke[i - 1]);
                Vector2 to = AlbumMenu.Around(camera, center, Stroke[i]);
                int samples = Mathf.CeilToInt(Vector2.Distance(from, to) / SampleMeters);
                for (int s = 0; s <= samples; s++)
                    cells.Add(world.Grid.CellOf(Vector2.Lerp(from, to, s / (float)samples)));
            }
            int built = 0;
            int planned = 0;
            foreach (Vector2Int cell in cells)
            {
                FloorState state = world.Floors.StateAt(cell);
                if (state == FloorState.Built)
                    built++;
                else if (state == FloorState.Planned)
                    planned++;
            }
            return built + " mailles bâties et " + planned + " en fantôme, sur " + cells.Count + " sous son milieu";
        }

        // Le point de l'écran où l'on voit ce point, en mètres autour de ce centre, vu de la caméra.
        private static Vector2 OnScreen(World world, Camera camera, Vector2 center, Vector2 around)
        {
            return camera.WorldToScreenPoint(world.Map.Relief.GroundAt(AlbumMenu.Around(camera, center, around)));
        }
    }
}
