using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les sols vus (GDD, sections 9 et 16 ; décisions 209 et 213) : un sol qui sert (bâti, ou qu'on efface) se peint dans
    /// le sol de la carte, de son calque (FloorPainting) ; un sol en chantier, en fantôme ou qu'on efface, se voit aussi en
    /// voile, de la couleur des fantômes, toujours, comme le fantôme d'une construction. La vue écoute les sols et se
    /// refait à la fin de l'image où ils ont changé, une seule fois ; un seul maillage pour tous les voiles.
    /// </summary>
    public sealed class FloorViews : MonoBehaviour
    {
        private readonly DrapedMesh _built = new DrapedMesh();
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();
        private World _world;
        private PlacementSettings _settings;
        private FloorPainting _painting;
        private GroundOverlay _ghosts;
        private bool _ghostsStale;

        /// <summary>
        /// Suit les sols de ce monde : peints sur ce Terrain (la copie en mémoire de la carte, dont la peinture d'origine
        /// est sur original), leurs fantômes en voile, du matériau translucide et de la couleur des fantômes de ces
        /// réglages.
        /// </summary>
        public void Show(World world, Terrain terrain, TerrainData original, PlacementSettings settings)
        {
            Hide();
            if (settings == null || settings.TranslucentMaterial == null)
                throw new System.InvalidOperationException("Les sols vus n'ont pas leur fiche de réglages de la pose.");
            _world = world;
            _settings = settings;
            _painting = new FloorPainting(terrain, original, world.Floors, world.Grid);
            _ghosts ??= new GroundOverlay("Fantômes des sols", transform, settings.TranslucentMaterial, 0);
            _ghosts.SetColor(settings.PlannedColor);
            _world.Floors.FloorsChanged += OnFloorsChanged;
            // Ce qui sert déjà se peint, ce qui est en chantier se voile : la vue part de l'état du monde.
            _painting.Mark(new RectInt(0, 0, world.Grid.Width, world.Grid.Depth));
            _ghostsStale = true;
        }

        /// <summary>Cesse de suivre les sols ; leur peinture reste sur la copie de la carte, qui part avec elle.</summary>
        public void Hide()
        {
            if (_world != null)
                _world.Floors.FloorsChanged -= OnFloorsChanged;
            _world = null;
            _painting = null;
            _ghostsStale = false;
            _ghosts?.SetVisible(false);
        }

        private void OnDestroy()
        {
            Hide();
            _ghosts?.Destroy();
            _ghosts = null;
        }

        private void OnFloorsChanged(RectInt cells)
        {
            _painting.Mark(cells);
            _ghostsStale = true;
        }

        private void LateUpdate()
        {
            if (_world == null)
                return;
            _painting.Flush();
            if (_ghostsStale)
                RebuildGhosts();
        }

        // Les fantômes : les mailles de chaque sol en chantier, celles de la file des chantiers.
        private void RebuildGhosts()
        {
            _ghostsStale = false;
            _built.Begin(_world.Map.Relief, _settings.PlaceLiftMeters);
            bool any = false;
            IReadOnlyList<ConstructionSite> queue = _world.ConstructionSites.Queue;
            for (int i = 0; i < queue.Count; i++)
            {
                FloorPatch patch = queue[i].Floor;
                if (patch == null)
                    continue;
                patch.CopyCells(_cells);
                _built.AddCells(_cells, _world.Grid);
                any |= _cells.Count > 0;
            }
            _ghosts.Apply(_built);
            _ghosts.SetVisible(any);
        }
    }
}
