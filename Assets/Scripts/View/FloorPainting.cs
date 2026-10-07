using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les sols peints dans le sol de la carte (GDD, sections 9 et 16 ; SAVOIR, 2.5) : sur la copie en mémoire du Terrain,
    /// chaque texel du sol peint dont le milieu tombe sur une maille où un sol sert (bâti, ou qu'on efface) prend le
    /// calque de ce sol, tout entier ; les autres reprennent la peinture de la carte, lue sur l'original, jamais touché.
    /// On repeint par lot : les changements marquent une boîte, repeinte une seule fois quand on le demande. Le calque
    /// d'un sol s'ajoute à la copie la première fois qu'il sert. Une boîte où rien ne sert, et où rien n'a jamais été
    /// peint, ne coûte qu'une lecture.
    /// </summary>
    internal sealed class FloorPainting
    {
        private readonly Terrain _terrain;
        private readonly TerrainData _original;
        private readonly Floors _floors;
        private readonly NavGrid _grid;
        private readonly List<TerrainLayer> _layers = new List<TerrainLayer>();
        private int[] _targets = Array.Empty<int>();
        private RectInt _dirty;
        private bool _isDirty;
        private RectInt _painted;
        private bool _hasPainted;

        /// <summary>
        /// Peint les sols de ce monde sur ce Terrain (sa copie), dont la peinture d'origine est sur original.
        /// </summary>
        public FloorPainting(Terrain terrain, TerrainData original, Floors floors, NavGrid grid)
        {
            _terrain = terrain != null ? terrain : throw new ArgumentNullException(nameof(terrain));
            _original = original != null ? original : throw new ArgumentNullException(nameof(original));
            _floors = floors ?? throw new ArgumentNullException(nameof(floors));
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        }

        /// <summary>Ces mailles ont changé : leur sol sera repeint au prochain Flush.</summary>
        public void Mark(RectInt cells)
        {
            _dirty = _isDirty ? Union(_dirty, cells) : cells;
            _isDirty = true;
        }

        /// <summary>Repeint ce qui a changé depuis la dernière fois, une seule fois.</summary>
        public void Flush()
        {
            if (!_isDirty)
                return;
            _isDirty = false;
            TerrainData data = _terrain.terrainData;
            RectInt texels = TexelsOf(_dirty, data);
            if (texels.width <= 0 || texels.height <= 0)
                return;
            bool serves = Target(texels, data);
            if (!serves && !(_hasPainted && _painted.Overlaps(texels)))
                return;
            Paint(texels, data);
            if (!serves)
                return;
            _painted = _hasPainted ? Union(_painted, texels) : texels;
            _hasPainted = true;
        }

        // Les texels du sol peint que touchent ces mailles, bornés au Terrain.
        private RectInt TexelsOf(RectInt cells, TerrainData data)
        {
            int resolution = data.alphamapResolution;
            float size = _grid.Metrics.CellSize;
            Vector3 origin = _terrain.transform.position;
            int fromX = Mathf.FloorToInt((cells.xMin * size - origin.x) / data.size.x * resolution);
            int toX = Mathf.CeilToInt((cells.xMax * size - origin.x) / data.size.x * resolution);
            int fromY = Mathf.FloorToInt((cells.yMin * size - origin.z) / data.size.z * resolution);
            int toY = Mathf.CeilToInt((cells.yMax * size - origin.z) / data.size.z * resolution);
            fromX = Mathf.Clamp(fromX, 0, resolution);
            fromY = Mathf.Clamp(fromY, 0, resolution);
            return new RectInt(fromX, fromY, Mathf.Clamp(toX, 0, resolution) - fromX,
                Mathf.Clamp(toY, 0, resolution) - fromY);
        }

        // Le calque que prend chaque texel (-1 : celui de la carte), rangé dans _targets ; vrai si un sol sert dans la
        // boîte. Un calque qui manque à la copie s'y ajoute.
        private bool Target(RectInt texels, TerrainData data)
        {
            int count = texels.width * texels.height;
            if (_targets.Length < count)
                _targets = new int[count];
            _layers.Clear();
            _layers.AddRange(data.terrainLayers);
            int before = _layers.Count;
            bool serves = false;
            Vector3 origin = _terrain.transform.position;
            for (int y = 0; y < texels.height; y++)
            {
                for (int x = 0; x < texels.width; x++)
                {
                    var point = new Vector2(origin.x + (texels.x + x + 0.5f) / data.alphamapResolution * data.size.x,
                        origin.z + (texels.y + y + 0.5f) / data.alphamapResolution * data.size.z);
                    int target = LayerAt(_grid.CellOf(point));
                    _targets[y * texels.width + x] = target;
                    serves |= target >= 0;
                }
            }
            if (_layers.Count > before)
                data.terrainLayers = _layers.ToArray();
            return serves;
        }

        // Le calque du sol qui sert sur cette maille, ajouté à la liste s'il y manque ; -1 sans sol qui sert.
        private int LayerAt(Vector2Int cell)
        {
            if (!_grid.Contains(cell))
                return -1;
            FloorState state = _floors.StateAt(cell);
            if (state != FloorState.Built && state != FloorState.Erasing)
                return -1;
            FloorDefinition floor = _floors.FloorAt(cell);
            TerrainLayer layer = floor != null ? floor.Layer : null;
            if (layer == null)
                return -1;
            int index = _layers.IndexOf(layer);
            if (index >= 0)
                return index;
            _layers.Add(layer);
            return _layers.Count - 1;
        }

        // Chaque texel de la boîte : tout le calque de son sol, ou la peinture de la carte (un calque ajouté n'y est pas).
        private void Paint(RectInt texels, TerrainData data)
        {
            float[,,] weights = data.GetAlphamaps(texels.x, texels.y, texels.width, texels.height);
            float[,,] original = _original.alphamapLayers > 0
                ? _original.GetAlphamaps(texels.x, texels.y, texels.width, texels.height)
                : null;
            int layers = weights.GetLength(2);
            int originalLayers = original != null ? original.GetLength(2) : 0;
            for (int y = 0; y < texels.height; y++)
            {
                for (int x = 0; x < texels.width; x++)
                {
                    int target = _targets[y * texels.width + x];
                    for (int layer = 0; layer < layers; layer++)
                    {
                        if (target >= 0)
                            weights[y, x, layer] = layer == target ? 1f : 0f;
                        else
                            weights[y, x, layer] = layer < originalLayers ? original[y, x, layer] : 0f;
                    }
                }
            }
            data.SetAlphamaps(texels.x, texels.y, weights);
        }

        private static RectInt Union(RectInt a, RectInt b)
        {
            var min = Vector2Int.Min(a.min, b.min);
            var max = Vector2Int.Max(a.max, b.max);
            return new RectInt(min, max - min);
        }
    }
}
