using System;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La distance de chaque maille d'une région à un ensemble de mailles, en mètres, de milieu à milieu (décisions
    /// 227 et 232) : celle du chanfrein, en deux passes, un pas droit valant un côté de maille, un pas en diagonale sa
    /// diagonale. Prise aux mailles d'un sol et aux autres, elle donne le bord d'un sol peint, son fondu et la bande
    /// de son liant (FloorWeights). Ses tableaux servent d'une distance à l'autre.
    /// </summary>
    internal sealed class CellDistance
    {
        private float[] _distances = Array.Empty<float>();
        private RectInt _cells;
        private float _size;

        /// <summary>
        /// Commence sur ces mailles, de ce côté en mètres : toutes loin de l'ensemble, qui est encore vide.
        /// </summary>
        public void Begin(RectInt cells, float cellSize)
        {
            _cells = cells;
            _size = cellSize;
            int count = cells.width * cells.height;
            if (_distances.Length < count)
                _distances = new float[count];
            for (int i = 0; i < count; i++)
                _distances[i] = float.PositiveInfinity;
        }

        /// <summary>Ajoute cette maille à l'ensemble ; hors de la région, elle ne compte pas.</summary>
        public void Add(Vector2Int cell)
        {
            int x = cell.x - _cells.xMin, y = cell.y - _cells.yMin;
            if (x >= 0 && y >= 0 && x < _cells.width && y < _cells.height)
                _distances[x + y * _cells.width] = 0f;
        }

        /// <summary>La distance au milieu de cette maille ; l'infini hors de la région.</summary>
        public float At(Vector2Int cell)
        {
            int x = cell.x - _cells.xMin, y = cell.y - _cells.yMin;
            if (x < 0 || y < 0 || x >= _cells.width || y >= _cells.height)
                return float.PositiveInfinity;
            return _distances[x + y * _cells.width];
        }

        /// <summary>
        /// Répand la distance depuis l'ensemble : une passe du bas à gauche vers le haut, une en retour.
        /// </summary>
        public void Spread()
        {
            float straight = _size, diagonal = _size * Mathf.Sqrt(2f);
            int width = _cells.width, height = _cells.height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float best = _distances[x + y * width];
                    if (x > 0)
                        best = Mathf.Min(best, _distances[x - 1 + y * width] + straight);
                    if (y > 0)
                    {
                        best = Mathf.Min(best, _distances[x + (y - 1) * width] + straight);
                        if (x > 0)
                            best = Mathf.Min(best, _distances[x - 1 + (y - 1) * width] + diagonal);
                        if (x < width - 1)
                            best = Mathf.Min(best, _distances[x + 1 + (y - 1) * width] + diagonal);
                    }
                    _distances[x + y * width] = best;
                }
            }
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = width - 1; x >= 0; x--)
                {
                    float best = _distances[x + y * width];
                    if (x < width - 1)
                        best = Mathf.Min(best, _distances[x + 1 + y * width] + straight);
                    if (y < height - 1)
                    {
                        best = Mathf.Min(best, _distances[x + (y + 1) * width] + straight);
                        if (x < width - 1)
                            best = Mathf.Min(best, _distances[x + 1 + (y + 1) * width] + diagonal);
                        if (x > 0)
                            best = Mathf.Min(best, _distances[x - 1 + (y + 1) * width] + diagonal);
                    }
                    _distances[x + y * width] = best;
                }
            }
        }

        /// <summary>
        /// La distance en ce point du sol, entre les milieux des quatre mailles autour ; l'infini hors de la région.
        /// </summary>
        public float Sample(Vector2 point)
        {
            float u = point.x / _size - 0.5f - _cells.xMin, v = point.y / _size - 0.5f - _cells.yMin;
            int x = Mathf.FloorToInt(u), y = Mathf.FloorToInt(v);
            if (x < 0 || y < 0 || x >= _cells.width - 1 || y >= _cells.height - 1)
                return float.PositiveInfinity;
            float tu = u - x, tv = v - y;
            int at = x + y * _cells.width;
            float bottom = Mathf.Lerp(_distances[at], _distances[at + 1], tu);
            float top = Mathf.Lerp(_distances[at + _cells.width], _distances[at + _cells.width + 1], tu);
            return Mathf.Lerp(bottom, top, tv);
        }
    }
}
