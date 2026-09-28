using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Game;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La planche des trajets (Docs/ALBUM.md), en Play, sur le monde en cours : le trajet d'un piéton du devant de la
    /// porte de la première construction posée au devant de celle de la dernière, dessiné au sol : l'escalier de mailles
    /// que trouve la recherche, en trait fin, et la ligne tendue qu'on en tire, en trait épais. La console dit les
    /// longueurs et le temps de la recherche, ou que la porte est injoignable. On pose ses cabanes à la souris, puis on
    /// lance la planche : elle efface la précédente, pose l'outil de côté et penche la vue vers la verticale. Ce ne sont
    /// que des traits : la simulation n'en sait rien.
    /// </summary>
    public static class PathBoard
    {
        private const string BoardName = "Planche des trajets";

        // La hauteur des traits au-dessus du sol, leur largeur et le pas auquel ils suivent le relief, en mètres.
        private const float LiftMeters = 0.06f;
        private const float StairsWidthMeters = 0.07f;
        private const float TautWidthMeters = 0.2f;
        private const float SampleMeters = 0.5f;

        // Un angle plus grand que tout l'écart permis : la vue s'arrête à sa borne (CameraPose).
        private const float TiltBeyondTheBoundDegrees = 90f;

        private static readonly Color StairsColor = new Color(0.95f, 0.95f, 0.95f);
        private static readonly Color TautColor = new Color(1f, 0.72f, 0.1f);

        [MenuItem("DICIELA/Album/Planche des trajets, de la première porte à la dernière")]
        public static void Stage()
        {
            var runner = Object.FindAnyObjectByType<GameRunner>();
            World world = runner != null ? runner.World : null;
            if (!EditorApplication.isPlaying || world == null)
            {
                Debug.LogError("DICIELA : la planche des trajets se fait en Play, une fois le monde né.");
                return;
            }
            var doors = new List<Structure>();
            foreach (Structure structure in world.Structures.All)
            {
                if (structure.Definition.HasDoor)
                    doors.Add(structure);
            }
            if (doors.Count < 2)
            {
                Debug.LogError("DICIELA : la planche des trajets va d'une porte à une autre ; pose d'abord deux cabanes.");
                return;
            }

            // L'outil de pose est posé de côté, comme par Échap : son aperçu ne cache pas les traits. La vue se penche
            // jusqu'à sa borne, au plus près de la verticale : les toits des fantômes ne les cachent pas non plus.
            var controls = Object.FindAnyObjectByType<PlacementControls>();
            if (controls != null && controls.Tool != null)
                controls.Tool.Cancel();
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (rig != null)
                rig.Tilt(-TiltBeyondTheBoundDegrees);

            Clear();
            Structure first = doors[0];
            Structure last = doors[doors.Count - 1];
            string trip = "de la porte de la construction n° " + first.Id + " à celle de la n° " + last.Id;
            var path = new Path();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            bool found = world.PathFinder.TryFindPath(Doorway(world, first), Doorway(world, last), path);
            watch.Stop();
            string took = watch.Elapsed.TotalMilliseconds.ToString("0.0") + " ms";
            if (!found)
            {
                Debug.Log("DICIELA : planche des trajets : injoignable, " + trip + " (dit en " + took + ").");
                return;
            }

            var centers = new List<Vector2>();
            foreach (Vector2Int cell in path.Cells)
                centers.Add(world.Grid.CellCenter(cell));
            var board = new GameObject(BoardName);
            Relief relief = world.Map.Relief;
            Draw(board.transform, "Escalier", centers, relief, StairsWidthMeters, StairsColor);
            Draw(board.transform, "Ligne tendue", path.Corners, relief, TautWidthMeters, TautColor);
            Debug.Log("DICIELA : planche des trajets prête, " + trip + " : escalier de " + path.Cells.Count +
                      " mailles, " + LengthOf(centers).ToString("0.0") + " m ; ligne tendue de " + path.Corners.Count +
                      " coins, " + path.Length.ToString("0.0") + " m ; cherché et tendu en " + took + ".");
        }

        /// <summary>Les mailles laissées libres devant la porte de cette construction.</summary>
        private static List<Vector2Int> Doorway(World world, Structure structure)
        {
            var cells = new List<Vector2Int>();
            foreach (Vector2Int cell in world.Grid.CellsCoveredBy(
                         world.PlacementRules.Doorway(structure.Definition, structure.Placement)))
                cells.Add(cell);
            return cells;
        }

        /// <summary>Efface les planches des trajets d'avant, et leurs matériaux.</summary>
        private static void Clear()
        {
            // Une planche par appel au plus : la boucle a sa borne.
            for (int i = 0; i < 64; i++)
            {
                GameObject old = GameObject.Find(BoardName);
                if (old == null)
                    return;
                foreach (LineRenderer line in old.GetComponentsInChildren<LineRenderer>())
                    Object.Destroy(line.sharedMaterial);
                Object.DestroyImmediate(old);
            }
        }

        /// <summary>Un trait posé à plat sur le sol, qui suit le relief entre ses points.</summary>
        private static void Draw(Transform parent, string name, IReadOnlyList<Vector2> points, Relief relief,
            float width, Color color)
        {
            var positions = new List<Vector3>();
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 from = points[i];
                positions.Add(OnGround(from, relief));
                if (i == points.Count - 1)
                    break;
                Vector2 to = points[i + 1];
                int samples = Mathf.FloorToInt(Vector2.Distance(from, to) / SampleMeters);
                for (int sample = 1; sample < samples; sample++)
                    positions.Add(OnGround(Vector2.Lerp(from, to, (float)sample / samples), relief));
            }

            var line = new GameObject(name);
            line.transform.SetParent(parent, false);
            // Le trait fait face à l'axe z de son objet, tourné vers le ciel : il est posé à plat.
            line.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var renderer = line.AddComponent<LineRenderer>();
            renderer.useWorldSpace = true;
            renderer.alignment = LineAlignment.TransformZ;
            renderer.widthMultiplier = width;
            renderer.numCornerVertices = 4;
            renderer.numCapVertices = 4;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = color };
            renderer.positionCount = positions.Count;
            renderer.SetPositions(positions.ToArray());
        }

        private static Vector3 OnGround(Vector2 point, Relief relief)
        {
            return new Vector3(point.x, relief.HeightAt(point.x, point.y) + LiftMeters, point.y);
        }

        private static float LengthOf(List<Vector2> points)
        {
            float length = 0f;
            for (int i = 1; i < points.Count; i++)
                length += Vector2.Distance(points[i - 1], points[i]);
            return length;
        }
    }
}
