using System;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les mailles que couvre un rectangle : toutes celles qu'il touche, même en partie, sur plus qu'un bord (SAVOIR,
    /// 2.1 ; décision 87). Une maille libre ne touche donc aucune construction : rien ne se chevauche, et aucune fente
    /// plus étroite qu'une maille ne se traverse. Un rectangle sans surface couvre la maille de son centre. Le seul
    /// parcours du maillage par un rectangle, pour poser comme pour vérifier. C'est une structure qui se parcourt
    /// elle-même avec <c>foreach</c>, comme l'énumérateur d'une <c>List</c> : le parcours n'alloue rien, même à chaque
    /// image.
    /// </summary>
    public struct CoveredCells
    {
        private readonly NavGrid _grid;
        private readonly RotatedRect _rect;
        private readonly float _halfCell;
        private readonly int _minX;
        private readonly int _minZ;
        private readonly int _columns;
        private readonly int _count;
        private readonly Vector2Int _centerCell;
        private readonly bool _centerInGrid;
        private int _next;
        private bool _covered;
        private bool _centerGiven;
        private Vector2Int _current;

        internal CoveredCells(NavGrid grid, RotatedRect rect)
        {
            // La boîte du rectangle, ramenée sur le maillage : seules ses mailles peuvent le toucher.
            float lowX = float.MaxValue, lowZ = float.MaxValue, highX = float.MinValue, highZ = float.MinValue;
            for (int corner = 0; corner < 4; corner++)
            {
                Vector2 point = rect.Corner(corner);
                lowX = Mathf.Min(lowX, point.x);
                lowZ = Mathf.Min(lowZ, point.y);
                highX = Mathf.Max(highX, point.x);
                highZ = Mathf.Max(highZ, point.y);
            }

            GridMetrics metrics = grid.Metrics;
            int minX = Math.Max(0, metrics.CellOf(lowX));
            int minZ = Math.Max(0, metrics.CellOf(lowZ));
            int maxX = Math.Min(grid.Width - 1, metrics.CellOf(highX));
            int maxZ = Math.Min(grid.Depth - 1, metrics.CellOf(highZ));

            _grid = grid;
            _rect = rect;
            _halfCell = metrics.CellSize * 0.5f;
            _minX = minX;
            _minZ = minZ;
            // Une boîte hors du maillage ne compte aucune maille : le parcours est vide.
            _columns = Math.Max(0, maxX - minX + 1);
            _count = _columns * Math.Max(0, maxZ - minZ + 1);
            _centerCell = new Vector2Int(metrics.CellOf(rect.Center.x), metrics.CellOf(rect.Center.y));
            _centerInGrid = grid.Contains(_centerCell);
            _next = 0;
            _covered = false;
            _centerGiven = false;
            _current = default;
        }

        /// <summary>La maille en cours : en x sa colonne, en y sa rangée sur l'axe z du monde.</summary>
        public Vector2Int Current => _current;

        public CoveredCells GetEnumerator()
        {
            return this;
        }

        /// <summary>Passe à la maille couverte suivante, rangée par rangée ; chaque maille est vue une fois.</summary>
        public bool MoveNext()
        {
            while (_next < _count)
            {
                var cell = new Vector2Int(_minX + _next % _columns, _minZ + _next / _columns);
                _next++;
                if (_rect.Overlaps(_grid.CellCenter(cell), _halfCell))
                {
                    _covered = true;
                    _current = cell;
                    return true;
                }
            }

            // Un rectangle sans surface ne recouvre aucune maille : il couvre alors celle de son centre, pour que rien ne
            // se pose sans marquer le maillage.
            if (_covered || _centerGiven || !_centerInGrid)
                return false;
            _centerGiven = true;
            _current = _centerCell;
            return true;
        }
    }
}
