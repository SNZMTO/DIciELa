using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les fantômes des sols en chantier (décisions 216 et 227) : chaque sorte de sol qu'on trace a son calque, à la
    /// texture de sa fiche (Diciela/SolFantome : la tuile posée comme le Terrain pose ses calques, depuis son origine,
    /// mêlée à la teinte des fantômes) ; un sol qu'on efface, et une sorte sans texture, se voient en voile de la
    /// couleur des fantômes. Leurs formes sont lissées comme celles des sols peints (CellField). Chaque sorte a sa
    /// copie du matériau, faite à son premier fantôme ; tout est détruit avec la vue.
    /// </summary>
    internal sealed class FloorGhosts
    {
        private const string VeilName = "Voile des sols";
        private const string GhostPrefix = "Fantôme du sol ";
        // La plus petite tuile admise, en mètres : un calque mal réglé ne divise pas par zéro.
        private const float MinTileMeters = 0.01f;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

        private readonly Transform _parent;
        private readonly PlacementSettings _settings;
        private readonly GroundOverlay _veil;
        private readonly Dictionary<FloorDefinition, Ghost> _ghosts = new Dictionary<FloorDefinition, Ghost>();
        private readonly List<FloorDefinition> _kinds = new List<FloorDefinition>();
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();
        private readonly List<Vector2Int> _patch = new List<Vector2Int>();
        private readonly CellField _field = new CellField();
        private readonly ContourMesh _fill = new ContourMesh();
        private readonly DrapedMesh _built = new DrapedMesh();
        private Vector3 _origin;

        // Le calque d'une sorte de sol, et sa copie du matériau.
        private sealed class Ghost
        {
            public GroundOverlay Overlay;
            public Material Material;
            public TerrainLayer Layer;
        }

        public FloorGhosts(Transform parent, PlacementSettings settings)
        {
            _parent = parent;
            _settings = settings;
            _veil = new GroundOverlay(VeilName, parent, settings.TranslucentMaterial, 0);
            _veil.SetColor(settings.PlannedColor);
        }

        /// <summary>Les tuiles se posent depuis l'origine de ce Terrain, comme ses calques.</summary>
        public void PlaceOn(Terrain terrain)
        {
            _origin = terrain.transform.position;
            foreach (KeyValuePair<FloorDefinition, Ghost> pair in _ghosts)
                Tile(pair.Value);
        }

        /// <summary>Refait les fantômes d'après les chantiers de sol de la file de ce monde.</summary>
        public void Rebuild(World world)
        {
            IReadOnlyList<ConstructionSite> queue = world.ConstructionSites.Queue;
            // Le voile : ce qu'on efface, et les sortes sans texture ; les sortes à texture sont notées pour la suite.
            _kinds.Clear();
            _cells.Clear();
            for (int i = 0; i < queue.Count; i++)
            {
                FloorPatch patch = queue[i].Floor;
                if (patch == null)
                    continue;
                if (HasTexture(patch))
                {
                    if (!_kinds.Contains(patch.Floor))
                        _kinds.Add(patch.Floor);
                    continue;
                }
                patch.CopyCells(_patch);
                _cells.AddRange(_patch);
            }
            _veil.SetVisible(Draw(world, _veil));

            foreach (KeyValuePair<FloorDefinition, Ghost> pair in _ghosts)
                pair.Value.Overlay.SetVisible(false);
            for (int k = 0; k < _kinds.Count; k++)
                RebuildKind(world, _kinds[k]);
        }

        public void Hide()
        {
            _veil.SetVisible(false);
            foreach (KeyValuePair<FloorDefinition, Ghost> pair in _ghosts)
                pair.Value.Overlay.SetVisible(false);
        }

        public void Destroy()
        {
            _veil.Destroy();
            foreach (KeyValuePair<FloorDefinition, Ghost> pair in _ghosts)
            {
                pair.Value.Overlay.Destroy();
                Object.Destroy(pair.Value.Material);
            }
            _ghosts.Clear();
        }

        // Les fantômes d'une sorte de sol, sur son calque.
        private void RebuildKind(World world, FloorDefinition kind)
        {
            IReadOnlyList<ConstructionSite> queue = world.ConstructionSites.Queue;
            _cells.Clear();
            for (int i = 0; i < queue.Count; i++)
            {
                FloorPatch patch = queue[i].Floor;
                if (patch == null || patch.IsErase || !ReferenceEquals(patch.Floor, kind))
                    continue;
                patch.CopyCells(_patch);
                _cells.AddRange(_patch);
            }
            Ghost ghost = GhostOf(kind);
            ghost.Overlay.SetVisible(Draw(world, ghost.Overlay));
        }

        // Dessine les mailles notées dans _cells sur ce calque, lissées ; vrai s'il y en a.
        private bool Draw(World world, GroundOverlay overlay)
        {
            float size = world.Grid.Metrics.CellSize;
            _field.Build(_cells, size, CellField.RadiusCells(_settings.GroundRoundingMeters, size));
            _built.Begin(world.Map.Relief, _settings.OverlayLiftMeters);
            _fill.AddFill(_built, _field);
            overlay.Apply(_built);
            return _cells.Count > 0;
        }

        private static bool HasTexture(FloorPatch patch)
        {
            return !patch.IsErase && patch.Floor.Layer != null && patch.Floor.Layer.diffuseTexture != null;
        }

        private Ghost GhostOf(FloorDefinition kind)
        {
            if (_ghosts.TryGetValue(kind, out Ghost ghost))
                return ghost;
            var material = new Material(_settings.FloorGhostMaterial) { name = GhostPrefix + kind.Id };
            material.SetTexture(BaseMapId, kind.Layer.diffuseTexture);
            // La teinte des fantômes, sans son opacité : celle du fantôme d'un sol est un réglage de son matériau.
            Color tint = _settings.PlannedColor;
            tint.a = 1f;
            material.SetColor(BaseColorId, tint);
            ghost = new Ghost
            {
                Overlay = new GroundOverlay(GhostPrefix + kind.Id, _parent, material, 0),
                Material = material,
                Layer = kind.Layer
            };
            Tile(ghost);
            _ghosts.Add(kind, ghost);
            return ghost;
        }

        // La tuile du calque en mètres du monde, comme le Terrain : (position - origine + décalage) / taille.
        private void Tile(Ghost ghost)
        {
            Vector2 size = ghost.Layer.tileSize;
            size = new Vector2(Mathf.Max(size.x, MinTileMeters), Mathf.Max(size.y, MinTileMeters));
            Vector2 offset = ghost.Layer.tileOffset;
            ghost.Material.SetTextureScale(BaseMapId, new Vector2(1f / size.x, 1f / size.y));
            ghost.Material.SetTextureOffset(BaseMapId,
                new Vector2((offset.x - _origin.x) / size.x, (offset.y - _origin.z) / size.y));
        }
    }
}
