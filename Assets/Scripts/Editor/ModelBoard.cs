using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Game;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La planche des modèles (Docs/ALBUM.md), en Play : chaque construction du catalogue deux fois, son modèle du pack
    /// puis son modèle préparé (sans modèle du pack, sa boîte seule), posés par la routine de pose au centre de la
    /// carte, porte au nord, et vus du côté de leur porte. On compare l'un à l'autre (la préparation ne change pas l'art) et l'on voit la porte au niveau du
    /// sol. Ce ne sont que des vues : la simulation n'en sait rien. Les gestes restent coupés jusqu'à la fin du Play.
    /// </summary>
    public static class ModelBoard
    {
        private const float GapMeters = 3f;

        // La distance de la vue, en largeurs de la planche : de quoi voir les portes de près.
        private const float FramingWidths = 0.9f;

        [MenuItem("DICIELA/Album/Planche des modèles, le pack et le préparé")]
        public static void Stage()
        {
            var rig = Object.FindAnyObjectByType<CameraRig>();
            var map = Object.FindAnyObjectByType<MapRoot>();
            if (!EditorApplication.isPlaying || rig == null || map == null)
            {
                Debug.LogError("DICIELA : la planche des modèles se fait en Play, une fois le monde né.");
                return;
            }
            var cameraControls = Object.FindAnyObjectByType<CameraControls>();
            var placementControls = Object.FindAnyObjectByType<PlacementControls>();
            if (cameraControls != null)
                cameraControls.enabled = false;
            if (placementControls != null)
                placementControls.enabled = false;

            // Le relief du monde, relu par la même conversion que lui : la planche se pose comme une construction.
            Terrain terrain = map.Terrain;
            Relief relief = MapReader.Read(terrain.terrainData, terrain.transform.position).Relief;
            Catalog catalog = EditorAssets.Load<Catalog>(ContentBuilder.CatalogPath);

            List<StructureDefinition> structures = catalog.AllStructures();
            float total = -GapMeters;
            foreach (StructureDefinition structure in structures)
                total += ModelsOf(structure).Length * (structure.WidthMeters + GapMeters);

            float x = (relief.SizeX - total) * 0.5f;
            float z = relief.SizeZ * 0.5f;
            foreach (StructureDefinition structure in structures)
            {
                foreach (GameObject model in ModelsOf(structure))
                {
                    GameObject view = ModelPose.Instantiate(model, null, "Planche : " + model.name);
                    ModelPose.Place(view.transform, new Placement(new Vector2(x + structure.WidthMeters * 0.5f, z), 0f),
                        relief);
                    x += structure.WidthMeters + GapMeters;
                }
            }

            // La vue part du centre de la carte, face au nord : un demi-tour la met face aux portes, puis elle s'approche.
            var settings = EditorAssets.Load<CameraSettings>(DataAssetBuilder.CameraSettingsPath);
            float wanted = Mathf.Max(settings.NearDistance, total * FramingWidths);
            float notches = Mathf.Log(wanted / rig.Distance) / Mathf.Log(1f - settings.ZoomStepPerNotch);
            Vector2 center = rig.Camera.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
            rig.Rotate(Mathf.DeltaAngle(rig.Camera.transform.eulerAngles.y, 180f));
            rig.Zoom(notches, center);
            Debug.Log("DICIELA : planche des modèles prête, " + total.ToString("0") + " m de large.");
        }

        /// <summary>Le modèle du pack puis le modèle préparé ; sans modèle du pack, la boîte seule.</summary>
        private static GameObject[] ModelsOf(StructureDefinition structure)
        {
            return structure.SourceModel != null
                ? new[] { structure.SourceModel, structure.Model }
                : new[] { structure.Model };
        }
    }
}
