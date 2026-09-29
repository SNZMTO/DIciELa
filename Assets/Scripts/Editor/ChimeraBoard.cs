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
    /// Les planches de la Chimère (Docs/ALBUM.md), en Play, en grille autour de la Chimère (cachée le temps de la planche) :
    /// chaque pièce du catalogue une fois, dans sa couleur d'origine, portée par un animal ; ou les animaux, chacun dans
    /// sa tête puis sous la tête du suivant, de face et de dos. On y juge où tombe chaque pièce et chaque tête, la caméra
    /// libre pour s'approcher et tourner autour ; on règle un point d'accroche dans sa fiche, puis on relance la planche.
    /// Ce ne sont que des vues : la simulation n'en sait rien. La pose et le toucher restent coupés jusqu'à la fin du Play.
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

        [MenuItem("DICIELA/Album/Planche de la Chimère, chaque pièce sur un corps")]
        public static void Stage()
        {
            if (!TryPrepare(out Board board))
                return;
            int count = board.Catalog.Attachments.Count;
            int rows = (count + Columns - 1) / Columns;
            for (int i = 0; i < count; i++)
            {
                AttachmentDefinition piece = board.Catalog.Attachments[i];
                HeadDefinition head = board.Catalog.Heads[i % board.Catalog.Heads.Count];
                BodyDefinition body = board.Catalog.Bodies[i % board.Catalog.Bodies.Count];
                board.Place(new Appearance(head, head.Palette, body, body.Palette,
                    new[] { new WornAttachment(piece, piece.Palette) }), piece.Id + " sur " + body.Id, i, rows, FacingDegrees);
            }
            Debug.Log("DICIELA : planche de la Chimère prête, " + count + " pièces.");
        }

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
                dressed.transform.SetPositionAndRotation(new Vector3(x, _relief.HeightAt(x, z), z),
                    Quaternion.Euler(0f, facing, 0f));
            }
        }
    }
}
