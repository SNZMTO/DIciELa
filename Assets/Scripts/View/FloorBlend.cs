using System;
using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Le mélange des calques d'une boîte de texels (décision 227) : chaque texel prend ce que couvrent les sols qui
    /// servent (FloorWeights), puis ce que couvrent leurs liants (le plus large, quand deux sols partagent le leur),
    /// puis la peinture de la carte, lue sur l'original à la finesse de la copie (MapPaint), pour le reste ; ses poids
    /// font toujours un. Le calque d'un sol, ou celui de son liant, qui manque à la copie s'y ajoute. Ses tableaux
    /// servent d'une boîte à l'autre.
    /// </summary>
    internal sealed class FloorBlend
    {
        private readonly List<TerrainLayer> _layers = new List<TerrainLayer>();
        private int[] _floorLayers = Array.Empty<int>();
        private int[] _borderLayers = Array.Empty<int>();
        private float[] _cover = Array.Empty<float>();
        private float[] _borderByLayer = Array.Empty<float>();

        /// <summary>
        /// Peint ces texels de la copie de ce Terrain d'après ces sols ; sans sol, la peinture de la carte revient.
        /// </summary>
        public void Paint(Terrain terrain, TerrainData original, FloorWeights weights, RectInt texels)
        {
            TerrainData data = terrain.terrainData;
            PrepareLayers(data, weights);
            float[,,] mixed = data.GetAlphamaps(texels.x, texels.y, texels.width, texels.height);
            float[,,] map = original.alphamapLayers > 0
                ? MapPaint.Read(original, data.alphamapResolution, texels)
                : null;
            int layers = mixed.GetLength(2), mapLayers = map != null ? map.GetLength(2) : 0;
            for (int y = 0; y < texels.height; y++)
            {
                for (int x = 0; x < texels.width; x++)
                {
                    float rest = Weigh(weights, FloorTexels.Center(terrain, texels.x + x, texels.y + y),
                        out float spread, out float share);
                    for (int layer = 0; layer < layers; layer++)
                        mixed[y, x, layer] = layer < mapLayers ? map[y, x, layer] * rest : 0f;
                    for (int k = 0; k < weights.Count; k++)
                    {
                        mixed[y, x, _floorLayers[k]] += _cover[k] * spread;
                        int layer = _borderLayers[k];
                        if (layer < 0)
                            continue;
                        mixed[y, x, layer] += _borderByLayer[layer] * share;
                        _borderByLayer[layer] = 0f;
                    }
                }
            }
            data.SetAlphamaps(texels.x, texels.y, mixed);
        }

        // Ce que couvrent en ce point les sols (notés dans _cover) et leurs liants (dans _borderByLayer) ; rend la part
        // qui reste à la carte, et ce par quoi multiplier les sols (spread) et les liants (share) pour que tout fasse
        // un.
        private float Weigh(FloorWeights weights, Vector2 point, out float spread, out float share)
        {
            float floors = 0f;
            for (int k = 0; k < weights.Count; k++)
            {
                _cover[k] = weights.FloorAt(k, point);
                floors += _cover[k];
            }
            spread = floors > 1f ? 1f / floors : 1f;
            floors = Mathf.Min(floors, 1f);
            float borders = 0f;
            for (int k = 0; k < weights.Count; k++)
            {
                int layer = _borderLayers[k];
                if (layer < 0)
                    continue;
                float border = weights.BorderAt(k, point);
                if (border <= _borderByLayer[layer])
                    continue;
                borders += border - _borderByLayer[layer];
                _borderByLayer[layer] = border;
            }
            float room = 1f - floors;
            share = borders > room ? room / borders : 1f;
            return Mathf.Max(0f, room - borders * share);
        }

        // Le rang, sur la copie, du calque de chaque sorte et de son liant ; un calque qui manque s'y ajoute.
        private void PrepareLayers(TerrainData data, FloorWeights weights)
        {
            int count = weights.Count;
            Grow(ref _floorLayers, count);
            Grow(ref _borderLayers, count);
            Grow(ref _cover, count);
            _layers.Clear();
            _layers.AddRange(data.terrainLayers);
            int before = _layers.Count;
            for (int k = 0; k < count; k++)
            {
                FloorDefinition floor = weights.FloorOf(k);
                _floorLayers[k] = LayerIndex(floor.Layer);
                _borderLayers[k] = floor.BorderMeters > 0f ? LayerIndex(floor.BorderLayer) : -1;
            }
            if (_layers.Count > before)
                data.terrainLayers = _layers.ToArray();
            Grow(ref _borderByLayer, _layers.Count);
        }

        private int LayerIndex(TerrainLayer layer)
        {
            int index = _layers.IndexOf(layer);
            if (index >= 0)
                return index;
            _layers.Add(layer);
            return _layers.Count - 1;
        }

        private static void Grow<T>(ref T[] array, int count)
        {
            if (array.Length < count)
                array = new T[count];
        }
    }
}
