using System;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les sols peints dans le sol de la carte (GDD, sections 9 et 16 ; décisions 214 et 227 ; SAVOIR, 2.5) : sur la
    /// copie en mémoire du Terrain, chaque texel prend de chaque sol qui sert (bâti, ou qu'on efface) ce qu'il en
    /// couvre, son bord fondu et effiloché, puis de son liant ce qu'en couvre la bande autour de lui
    /// (FloorWeights), et de la carte le reste (FloorBlend) ; l'original n'est jamais touché. On repeint par lot, par
    /// carreaux : les changements marquent une boîte, repeinte une seule fois quand on le demande, avec tout ce qu'un
    /// sol peut toucher autour d'elle. Un carreau où rien ne sert, et où rien n'a jamais été peint, ne coûte qu'une
    /// lecture.
    /// </summary>
    internal sealed class FloorPainting
    {
        // Le côté d'un carreau, en mailles : ce qu'on lit et peint d'un coup.
        private const int TileCells = 64;

        private readonly Terrain _terrain;
        private readonly TerrainData _original;
        private readonly Floors _floors;
        private readonly NavGrid _grid;
        private readonly FloorWeights _weights = new FloorWeights();
        private readonly FloorBlend _blend = new FloorBlend();
        private readonly int _radius;
        private readonly int _reach;
        private RectInt _dirty;
        private bool _isDirty;
        private RectInt _painted;
        private bool _hasPainted;
        private bool _foresight;

        /// <summary>
        /// Peint les sols de ce monde sur ce Terrain (sa copie), dont la peinture d'origine est sur original : leurs
        /// bords lissés sur ce rayon, en mètres.
        /// </summary>
        public FloorPainting(Terrain terrain, TerrainData original, Floors floors, NavGrid grid, float roundingMeters)
        {
            _terrain = terrain != null ? terrain : throw new ArgumentNullException(nameof(terrain));
            _original = original != null ? original : throw new ArgumentNullException(nameof(original));
            _floors = floors ?? throw new ArgumentNullException(nameof(floors));
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            float size = grid.Metrics.CellSize;
            _radius = CellField.RadiusCells(roundingMeters, size);
            // Jusqu'où un sol peint : son liant le plus large et la moitié de son fondu, son bord le plus effiloché,
            // son flou, et une marge.
            float reach = FloorDefinition.MaxBorderMeters + FloorDefinition.MaxFadeMeters / 2f +
                          FloorDefinition.MaxFrayMeters;
            _reach = Mathf.CeilToInt(reach / size) + 2 * _radius + 2;
        }

        /// <summary>
        /// Ces mailles ont changé : leur sol, et ce qu'il touche autour, sera repeint au prochain Flush.
        /// </summary>
        public void Mark(RectInt cells)
        {
            _dirty = _isDirty ? FloorTexels.Union(_dirty, cells) : cells;
            _isDirty = true;
        }

        /// <summary>
        /// En prévision, les sols se peignent tels qu'ils seront une fois leurs chantiers finis (décision 228) : tout
        /// ce qui porte un sol se repeint.
        /// </summary>
        public void Foresee(bool on)
        {
            if (on == _foresight)
                return;
            _foresight = on;
            MarkFloors();
        }

        /// <summary>
        /// Marque tout ce qui porte un sol, bâti, tracé ou qu'on efface : la vue part de l'état du monde.
        /// </summary>
        public void MarkFloors()
        {
            var min = new Vector2Int(int.MaxValue, int.MaxValue);
            var max = new Vector2Int(int.MinValue, int.MinValue);
            for (int y = 0; y < _grid.Depth; y++)
            {
                for (int x = 0; x < _grid.Width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (_floors.StateAt(cell) == FloorState.None)
                        continue;
                    min = Vector2Int.Min(min, cell);
                    max = Vector2Int.Max(max, cell);
                }
            }
            if (min.x <= max.x)
                Mark(new RectInt(min, max - min + Vector2Int.one));
        }

        /// <summary>Repeint ce qui a changé depuis la dernière fois, une seule fois, carreau par carreau.</summary>
        public void Flush()
        {
            if (!_isDirty)
                return;
            _isDirty = false;
            RectInt region = FloorTexels.OnGrid(_grid, FloorTexels.Expand(_dirty, _reach));
            for (int y = region.yMin; y < region.yMax; y += TileCells)
            {
                for (int x = region.xMin; x < region.xMax; x += TileCells)
                    PaintTile(new RectInt(x, y, Mathf.Min(TileCells, region.xMax - x),
                        Mathf.Min(TileCells, region.yMax - y)));
            }
        }

        private void PaintTile(RectInt tile)
        {
            RectInt texels = FloorTexels.Of(_terrain, _grid, tile);
            if (texels.width <= 0 || texels.height <= 0)
                return;
            RectInt around = FloorTexels.OnGrid(_grid, FloorTexels.Expand(tile, _reach));
            bool serves = _weights.Build(_floors, _grid, around, _radius, _foresight);
            if (!serves && !(_hasPainted && _painted.Overlaps(texels)))
                return;
            _blend.Paint(_terrain, _original, _weights, texels);
            if (!serves)
                return;
            _painted = _hasPainted ? FloorTexels.Union(_painted, texels) : texels;
            _hasPainted = true;
        }
    }
}
