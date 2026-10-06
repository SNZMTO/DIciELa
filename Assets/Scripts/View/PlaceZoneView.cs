using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La Place vue (GDD, section 11 ; décision 206) : un voile léger sur le sol de la zone, son contour, et ses places de
    /// charrette en cadres blancs, une pointe vers l'avant, comme les fantômes. Elle ne se voit qu'en mode construction :
    /// le démarrage dit quand. Elle écoute la Place et ne se refait que quand celle-ci change, et seulement si elle se
    /// voit ; un seul maillage pour le voile, un pour les lignes, quelle que soit leur taille.
    /// </summary>
    public sealed class PlaceZoneView : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la pose : le matériau translucide, le voile et les lignes de la Place.")]
        [SerializeField] private PlacementSettings _settings;

        private readonly DrapedMesh _built = new DrapedMesh();
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();
        private World _world;
        private GroundOverlay _veil;
        private GroundOverlay _lines;
        private bool _shown;
        private bool _stale;

        /// <summary>Suit la Place de ce monde, cachée tant qu'on n'est pas en mode construction.</summary>
        public void Show(World world)
        {
            Hide();
            if (_settings == null || _settings.TranslucentMaterial == null)
                throw new System.InvalidOperationException("La Place vue n'a pas sa fiche de réglages de la pose.");
            _world = world;
            _veil ??= new GroundOverlay("Voile de la Place", transform, _settings.TranslucentMaterial, 0);
            _lines ??= new GroundOverlay("Lignes de la Place", transform, _settings.TranslucentMaterial, 1);
            _veil.SetColor(_settings.PlaceVeilColor);
            _lines.SetColor(_settings.PlaceLineColor);
            _world.Place.PlaceChanged += OnChanged;
            _world.Place.MarkPlaced += OnMarkChanged;
            _world.Place.MarkRemoved += OnMarkChanged;
            _stale = true;
        }

        /// <summary>Cesse de suivre la Place, et la cache.</summary>
        public void Hide()
        {
            if (_world != null)
            {
                _world.Place.PlaceChanged -= OnChanged;
                _world.Place.MarkPlaced -= OnMarkChanged;
                _world.Place.MarkRemoved -= OnMarkChanged;
            }
            _world = null;
            SetShown(false);
        }

        /// <summary>Montre la Place en mode construction, la cache sinon.</summary>
        public void SetShown(bool shown)
        {
            _shown = shown && _world != null;
            if (_shown && _stale)
                Rebuild();
            _veil?.SetVisible(_shown);
            _lines?.SetVisible(_shown);
        }

        private void OnDestroy()
        {
            Hide();
            _veil?.Destroy();
            _lines?.Destroy();
            _veil = null;
            _lines = null;
        }

        private void OnChanged()
        {
            _stale = true;
            if (_shown)
                Rebuild();
        }

        private void OnMarkChanged(PlaceMark mark)
        {
            OnChanged();
        }

        private void Rebuild()
        {
            _stale = false;
            PlaceZone place = _world.Place;
            NavGrid grid = _world.Grid;
            Relief relief = _world.Map.Relief;
            place.CopyCells(_cells);
            _built.Begin(relief, _settings.PlaceLiftMeters);
            _built.AddCells(_cells, grid);
            _veil.Apply(_built);

            // Le contour : chaque côté de maille qui ne touche pas la Place ; les lignes un peu au-dessus du voile.
            _built.Begin(relief, _settings.PlaceLiftMeters * 2f);
            float width = _settings.PlaceLineWidthMeters, half = grid.Metrics.CellSize * 0.5f;
            for (int i = 0; i < _cells.Count; i++)
            {
                Vector2Int cell = _cells[i];
                Vector2 center = grid.CellCenter(cell);
                if (!place.Contains(cell + Vector2Int.left))
                    _built.AddSegment(center + new Vector2(-half, -half), center + new Vector2(-half, half), width);
                if (!place.Contains(cell + Vector2Int.right))
                    _built.AddSegment(center + new Vector2(half, -half), center + new Vector2(half, half), width);
                if (!place.Contains(cell + Vector2Int.down))
                    _built.AddSegment(center + new Vector2(-half, -half), center + new Vector2(half, -half), width);
                if (!place.Contains(cell + Vector2Int.up))
                    _built.AddSegment(center + new Vector2(-half, half), center + new Vector2(half, half), width);
            }
            IReadOnlyList<PlaceMark> marks = place.Marks;
            for (int i = 0; i < marks.Count; i++)
                _built.AddMark(marks[i].Footprint, width);
            _lines.Apply(_built);
        }
    }
}
