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
    /// La planche de la Chimère (Docs/ALBUM.md), en Play : chaque pièce du catalogue une fois, portée par un corps, les
    /// corps pris l'un après l'autre, en grille autour de la Chimère (cachée le temps de la planche), de trois quarts face
    /// à la vue. On y juge où tombe chaque pièce, la caméra libre pour s'approcher et tourner autour ; on règle son point
    /// d'accroche dans sa fiche, puis on relance la planche. Ce ne sont que des vues : la simulation n'en sait rien. La
    /// pose et le toucher restent coupés jusqu'à la fin du Play.
    /// </summary>
    public static class ChimeraBoard
    {
        private const int Columns = 8;
        private const float ColumnMeters = 2.6f;
        private const float RowMeters = 3.4f;
        // Les personnages se tournent vers la vue, de trois quarts : on voit aussi les queues et les capes.
        private const float FacingDegrees = 155f;
        // La distance de la vue, en largeurs de la planche : de quoi la voir entière.
        private const float FramingWidths = 1.1f;

        [MenuItem("DICIELA/Album/Planche de la Chimère, chaque pièce sur un corps")]
        public static void Stage()
        {
            var rig = Object.FindAnyObjectByType<CameraRig>();
            var villagers = Object.FindAnyObjectByType<VillagerViews>();
            var map = Object.FindAnyObjectByType<MapRoot>();
            if (!EditorApplication.isPlaying || rig == null || map == null || villagers == null ||
                villagers.transform.childCount == 0)
            {
                Debug.LogError("DICIELA : la planche de la Chimère se fait en Play, une fois la Chimère née.");
                return;
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
            Relief relief = MapReader.Read(terrain.terrainData, terrain.transform.position).Relief;
            Catalog catalog = EditorAssets.Load<Catalog>(ContentBuilder.CatalogPath);
            Transform chimera = villagers.transform.GetChild(0);
            Vector3 center = chimera.position;
            chimera.gameObject.SetActive(false);

            int count = catalog.Attachments.Count;
            int rows = (count + Columns - 1) / Columns;
            for (int i = 0; i < count; i++)
            {
                AttachmentDefinition piece = catalog.Attachments[i];
                BodyDefinition body = catalog.Bodies[i % catalog.Bodies.Count];
                GameObject dressed = Dresser.Dress(new Appearance(body, body.Palette, new[] { piece }), null);
                if (dressed == null)
                    continue;
                dressed.name = "Planche : " + piece.Id + " sur " + body.Id;
                float x = center.x + (i % Columns - (Columns - 1) * 0.5f) * ColumnMeters;
                float z = center.z + ((rows - 1) * 0.5f - i / Columns) * RowMeters;
                dressed.transform.SetPositionAndRotation(new Vector3(x, relief.HeightAt(x, z), z),
                    Quaternion.Euler(0f, FacingDegrees, 0f));
            }

            // La vue part de la Chimère, face au nord : les personnages la regardent ; elle recule pour tout voir.
            rig.Rotate(Mathf.DeltaAngle(rig.Camera.transform.eulerAngles.y, 0f));
            rig.ZoomTo(Columns * ColumnMeters * FramingWidths);
            Debug.Log("DICIELA : planche de la Chimère prête, " + count + " pièces sur " + Math.Min(count,
                catalog.Bodies.Count) + " corps.");
        }
    }
}
