using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La Place vue (GDD, section 11 ; décisions 206 et 227) : un voile léger sur le sol de la zone et son contour,
    /// lissés (CellField : leurs coins arrondis, sans escalier), et ses places de charrette en cadres blancs, une
    /// pointe vers l'avant, comme les fantômes. Elle ne se voit qu'en mode construction : le démarrage dit quand. Elle
    /// écoute la Place et ne se refait que quand celle-ci change, et seulement si elle se voit ; un seul maillage pour
    /// le voile, un pour les lignes, quelle que soit leur taille.
    /// </summary>
    public sealed class PlazaZoneView : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la pose : le matériau translucide, le voile et les lignes de la Place.")]
        [SerializeField] private PlacementSettings _settings;

        private readonly DrapedMesh _built = new DrapedMesh();
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();
        private readonly CellField _field = new CellField();
        private readonly ContourMesh _fill = new ContourMesh();
        private readonly ContourLines _outline = new ContourLines();
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
            _veil.SetColor(_settings.ZoneVeilColor);
            _lines.SetColor(_settings.OutlineColor);
            _world.Plaza.PlazaChanged += OnChanged;
            _world.Plaza.MarkPlaced += OnMarkChanged;
            _world.Plaza.MarkRemoved += OnMarkChanged;
            _stale = true;
        }

        /// <summary>Cesse de suivre la Place, et la cache.</summary>
        public void Hide()
        {
            if (_world != null)
            {
                _world.Plaza.PlazaChanged -= OnChanged;
                _world.Plaza.MarkPlaced -= OnMarkChanged;
                _world.Plaza.MarkRemoved -= OnMarkChanged;
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

        private void OnMarkChanged(PlazaMark mark)
        {
            OnChanged();
        }

        private void Rebuild()
        {
            _stale = false;
            PlazaZone plaza = _world.Plaza;
            NavGrid grid = _world.Grid;
            Relief relief = _world.Map.Relief;
            plaza.CopyCells(_cells);
            float size = grid.Metrics.CellSize;
            _field.Build(_cells, size, CellField.RadiusCells(_settings.GroundRoundingMeters, size));
            _built.Begin(relief, _settings.OverlayLiftMeters);
            _fill.AddFill(_built, _field);
            _veil.Apply(_built);

            // Le contour, lissé comme le voile ; les lignes un peu au-dessus de lui.
            _built.Begin(relief, _settings.OverlayLiftMeters * 2f);
            float width = _settings.OutlineWidthMeters;
            _outline.AddOutline(_built, _field, width);
            IReadOnlyList<PlazaMark> marks = plaza.Marks;
            for (int i = 0; i < marks.Count; i++)
                _built.AddMark(marks[i].Footprint, width);
            _lines.Apply(_built);
        }
    }
}
