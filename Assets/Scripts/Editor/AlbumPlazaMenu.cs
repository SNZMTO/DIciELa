using Diciela.Controls;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les mises en scène de la Place pour l'album (Docs/ALBUM.md ; décision 206), en Play, le campement installé, par
    /// les mêmes appels que les gestes de son outil. La tracer : la vue se tourne vers un endroit à droite du campement,
    /// l'outil de la Place se prend, puis un disque, un rectangle qui le touche, un coup de pinceau, et deux places de
    /// charrette côte à côte ; le pinceau reste visé au bord. Tirer un disque trop grand : son aperçu rouge, et la raison.
    /// Les gestes de la caméra et de la Place restent coupés jusqu'à la fin du Play, pour que la souris ne bouge rien.
    /// </summary>
    public static class AlbumPlazaMenu
    {
        // La distance de la Place au campement, vers la droite de l'écran, et celle de la caméra, en mètres ; le temps
        // laissé à la caméra et à chaque ordre pour être joué, en secondes.
        private const float Offset = 15f;
        private const float FramingMeters = 34f;
        private const double CameraSeconds = 0.8;
        private const double OrderSeconds = 0.4;

        // Le disque, le rectangle (de son premier coin à l'autre, en mètres le long de la droite de l'écran et vers
        // le haut de l'écran) et le trait de pinceau, depuis le milieu de la Place.
        private const float DiscRadius = 7f;
        private static readonly Vector2 BoxFrom = new Vector2(-3f, 4f);
        private static readonly Vector2 BoxTo = new Vector2(8f, 10f);
        private static readonly Vector2[] Stroke = { new Vector2(-4f, -6f), new Vector2(-2f, -8.5f), new Vector2(1f, -9f) };

        // Les deux places de charrette, de part et d'autre du milieu, l'avant vers la droite de l'écran.
        private const float MarkSpacing = 1.6f;

        [MenuItem("DICIELA/Album/Tracer la Place près du campement")]
        public static void TraceThePlaza()
        {
            if (!TryStage(out World world, out PaintTool tool, out Camera camera, out Frame frame))
                return;
            AlbumMenu.After(CameraSeconds, () =>
            {
                tool.Choose(PaintShape.Disc);
                Draw(tool, world, camera, PaintToolMode.Shape, frame.At(Vector2.zero),
                    frame.At(new Vector2(DiscRadius, 0f)));
                AlbumMenu.After(OrderSeconds, () =>
                {
                    tool.Choose(PaintShape.Box);
                    Draw(tool, world, camera, PaintToolMode.Shape, frame.At(BoxFrom), frame.At(BoxTo));
                    AlbumMenu.After(OrderSeconds, () =>
                    {
                        Draw(tool, world, camera, PaintToolMode.Brush, frame.At(Stroke[0]), frame.At(Stroke[1]),
                            frame.At(Stroke[2]));
                        AlbumMenu.After(OrderSeconds, () => PlaceTheMarks(tool, world, camera, frame));
                    });
                });
            });
        }

        [MenuItem("DICIELA/Album/Tirer un disque trop grand sur la Place")]
        public static void DrawATooLargeDisc()
        {
            if (!TryStage(out World world, out PaintTool tool, out Camera camera, out Frame frame))
                return;
            AlbumMenu.After(CameraSeconds, () =>
            {
                tool.Choose(PaintToolMode.Shape);
                tool.Choose(PaintShape.Disc);
                tool.BeginDrawing(OnScreen(world, camera, frame.At(Vector2.zero)));
                tool.AimAt(OnScreen(world, camera, frame.At(new Vector2(DiscRadius * 2f, 0f))));
                Debug.Log("DICIELA : mise en scène de l'album : un disque de " + DiscRadius * 2f + " m, " + tool.Answer + ".");
            });
        }

        private static void PlaceTheMarks(PaintTool tool, World world, Camera camera, Frame frame)
        {
            tool.Choose(PaintToolMode.CartSpot);
            tool.RotateBy(Mathf.DeltaAngle(tool.Options.HeadingDegrees, Heading.Of(frame.Right)));
            tool.AimAt(OnScreen(world, camera, frame.At(new Vector2(0f, MarkSpacing))));
            tool.Click();
            AlbumMenu.After(OrderSeconds, () =>
            {
                tool.AimAt(OnScreen(world, camera, frame.At(new Vector2(0f, -MarkSpacing))));
                tool.Click();
                AlbumMenu.After(OrderSeconds, () =>
                {
                    tool.Choose(PaintToolMode.Brush);
                    tool.AimAt(OnScreen(world, camera, frame.At(new Vector2(-DiscRadius, 0f))));
                    Debug.Log("DICIELA : mise en scène de l'album : la Place a " +
                              Mathf.RoundToInt(world.Plaza.AreaSquareMeters) + " m² et " + world.Plaza.Marks.Count +
                              " place(s) de charrette.");
                });
            });
        }

        // Coupe les gestes de la caméra et de la Place, tourne la vue vers la droite du campement, prend l'outil.
        private static bool TryStage(out World world, out PaintTool tool, out Camera camera, out Frame frame)
        {
            world = AlbumWorld.Running();
            var controls = Object.FindAnyObjectByType<PaintControls>();
            var cameraControls = Object.FindAnyObjectByType<CameraControls>();
            var rig = Object.FindAnyObjectByType<CameraRig>();
            Structure home = world != null ? world.Structures.ChimeraHome : null;
            tool = controls != null ? controls.Tool : null;
            camera = rig != null ? rig.Camera : null;
            frame = default;
            if (home == null || tool == null || cameraControls == null || rig == null)
            {
                Debug.LogError("DICIELA : on met la Place en scène en Play, une fois le campement installé.");
                return false;
            }
            controls.enabled = false;
            cameraControls.enabled = false;
            Vector3 forward = camera.transform.forward;
            var ahead = new Vector2(forward.x, forward.z).normalized;
            frame = new Frame(home.Placement.Center + new Vector2(ahead.y, -ahead.x) * Offset, ahead);
            var middle = new Vector2(camera.pixelWidth / 2f, camera.pixelHeight / 2f);
            if (rig.BeginDrag(OnScreen(world, camera, frame.At(Vector2.zero))))
                rig.DragTo(middle);
            rig.ZoomTo(FramingMeters);
            tool.Take();
            return true;
        }

        private static void Draw(PaintTool tool, World world, Camera camera, PaintToolMode mode, params Vector2[] points)
        {
            tool.Choose(mode);
            tool.BeginDrawing(OnScreen(world, camera, points[0]));
            for (int i = 1; i < points.Length; i++)
                tool.AimAt(OnScreen(world, camera, points[i]));
            PaintAnswer result = tool.Answer;
            tool.EndDrawing();
            Debug.Log("DICIELA : mise en scène de l'album : " + mode + ", " + result + ".");
        }

        private static Vector2 OnScreen(World world, Camera camera, Vector2 ground)
        {
            return camera.WorldToScreenPoint(world.Map.Relief.GroundAt(ground));
        }

        // Le repère de la mise en scène : le milieu de la Place, la droite de l'écran et son haut, au sol.
        private readonly struct Frame
        {
            private readonly Vector2 _center;
            private readonly Vector2 _ahead;

            public Frame(Vector2 center, Vector2 ahead)
            {
                _center = center;
                _ahead = ahead;
            }

            public Vector2 Right => new Vector2(_ahead.y, -_ahead.x);

            public Vector2 At(Vector2 local)
            {
                return _center + Right * local.x + _ahead * local.y;
            }
        }
    }
}
