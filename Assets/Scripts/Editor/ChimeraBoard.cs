using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Game;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using Diciela.View;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La planche des têtes et des corps (Docs/ALBUM.md), en Play, en grille autour de la Chimère (cachée le temps de la
    /// planche) : les animaux, chacun sous sa tête puis sous celle du suivant, de face et de dos, la caméra libre pour
    /// s'approcher et tourner autour. Les pièces, elles, se jugent sur leurs planches en fichiers (PieceSheets). Ce ne
    /// sont que des vues : la simulation n'en sait rien. La pose et le toucher restent coupés jusqu'à la fin du Play.
    /// </summary>
    public static class ChimeraBoard
    {
        private const int Columns = 8;
        private const float ColumnMeters = 2.6f;
        private const float RowMeters = 3.4f;
        // Les personnages se tournent vers la vue, de trois quarts : on voit aussi les queues et les capes ; ou lui
        // tournent le dos, de trois quarts aussi.
        private const float FacingDegrees = 155f;
        private const float BackDegrees = -25f;
        // La distance de la vue, en largeurs de la planche : de quoi la voir entière.
        private const float FramingWidths = 1.1f;

        [MenuItem("DICIELA/Album/Planche des têtes et des corps, de face et de dos")]
        public static void StageAnimals()
        {
            if (!TryPrepare(out Board board))
                return;
            // Une silhouette A par animal : la sienne, puis sous la tête de l'animal suivant, de face puis de dos.
            int animals = Math.Min(Columns, board.Catalog.Bodies.Count / 2);
            for (int i = 0; i < animals; i++)
            {
                BodyDefinition body = board.Catalog.Bodies[2 * i];
                HeadDefinition own = board.Catalog.Heads[2 * i];
                HeadDefinition next = board.Catalog.Heads[2 * ((i + 1) % animals)];
                board.Place(Plain(own, body), own.Id + " sur " + body.Id, i, 3, FacingDegrees);
                board.Place(Plain(next, body), next.Id + " sur " + body.Id, Columns + i, 3, FacingDegrees);
                board.Place(Plain(next, body), next.Id + " sur " + body.Id + ", de dos", 2 * Columns + i, 3, BackDegrees);
            }
            Debug.Log("DICIELA : planche des têtes et des corps prête, " + animals + " animaux.");
        }

        private static Appearance Plain(HeadDefinition head, BodyDefinition body)
        {
            return new Appearance(head, head.Palette, body, body.Palette, Array.Empty<WornAttachment>());
        }

        /// <summary>Le Play, sa carte et son catalogue ; la vue face au nord, reculée pour voir la planche.</summary>
        private static bool TryPrepare(out Board board)
        {
            board = null;
            var rig = Object.FindAnyObjectByType<CameraRig>();
            var villagers = Object.FindAnyObjectByType<VillagerViews>();
            var map = Object.FindAnyObjectByType<MapRoot>();
            if (!EditorApplication.isPlaying || rig == null || map == null || villagers == null ||
                villagers.transform.childCount == 0)
            {
                Debug.LogError("DICIELA : les planches de la Chimère se font en Play, une fois la Chimère née.");
                return false;
            }
            foreach (Behaviour gestures in new Behaviour[]
                     {
                         Object.FindAnyObjectByType<PlacementControls>(), Object.FindAnyObjectByType<ChooseControls>()
                     })
            {
                if (gestures != null)
                    gestures.enabled = false;
            }

            Terrain terrain = map.Terrain;
            Transform chimera = villagers.transform.GetChild(0);
            board = new Board(MapReader.Read(terrain.terrainData, terrain.transform.position).Relief,
                EditorAssets.Load<Catalog>(ContentBuilder.CatalogPath), chimera.position);
            chimera.gameObject.SetActive(false);
            // La vue part de la Chimère, face au nord : les personnages la regardent ; elle recule pour tout voir.
            rig.Rotate(Mathf.DeltaAngle(rig.Camera.transform.eulerAngles.y, 0f));
            rig.ZoomTo(Columns * ColumnMeters * FramingWidths);
            return true;
        }

        /// <summary>Une planche en cours : où elle se pose, et ce qu'elle montre.</summary>
        private sealed class Board
        {
            private readonly Relief _relief;
            private readonly Vector3 _center;

            public Board(Relief relief, Catalog catalog, Vector3 center)
            {
                _relief = relief;
                Catalog = catalog;
                _center = center;
            }

            public Catalog Catalog { get; }

            /// <summary>Habille un personnage et le pose à cette place de la grille, tourné de tant.</summary>
            public void Place(Appearance appearance, string name, int index, int rows, float facing)
            {
                GameObject dressed = Dresser.Dress(appearance, null);
                if (dressed == null)
                    return;
                dressed.name = "Planche : " + name;
                float x = _center.x + (index % Columns - (Columns - 1) * 0.5f) * ColumnMeters;
                float z = _center.z + ((rows - 1) * 0.5f - index / Columns) * RowMeters;
                dressed.transform.SetPositionAndRotation(_relief.GroundAt(new Vector2(x, z)),
                    Quaternion.Euler(0f, facing, 0f));
            }
        }
    }
}
