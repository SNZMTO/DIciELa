using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les arrivées d'une recherche de trajet (SAVOIR, 2.1) : lesquelles sont marquées, leur boîte, et le coût restant
    /// estimé pour rejoindre la plus proche. Un booléen par maille, fait une fois, marqué au début d'une recherche et
    /// effacé à la fin.
    /// </summary>
    internal sealed class PathGoals
    {
        private readonly NavGrid _grid;
        private readonly bool[] _isGoal;
        private Vector2Int _min;
        private Vector2Int _max;

        public PathGoals(NavGrid grid)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
            _isGoal = new bool[grid.Width * grid.Depth];
        }

        /// <summary>Marque les arrivées libres et prend leur boîte ; faux s'il n'y en a aucune.</summary>
        public bool Mark(IReadOnlyList<Vector2Int> goals)
        {
            bool any = false;
            for (int i = 0; i < goals.Count; i++)
            {
                Vector2Int goal = goals[i];
                if (!_grid.IsWalkable(goal))
                    continue;
                _isGoal[_grid.IndexOf(goal)] = true;
                _min = any ? Vector2Int.Min(_min, goal) : goal;
                _max = any ? Vector2Int.Max(_max, goal) : goal;
                any = true;
            }
            return any;
        }

        public void Unmark(IReadOnlyList<Vector2Int> goals)
        {
            for (int i = 0; i < goals.Count; i++)
                _isGoal[_grid.IndexOf(goals[i])] = false;
        }

        /// <summary>Vrai si la maille à cette place dans le maillage est une arrivée marquée.</summary>
        public bool Contains(int index)
        {
            return _isGoal[index];
        }

        /// <summary>
        /// Le coût restant estimé : la distance octile (en huit directions) de cette maille à la boîte des arrivées.
        /// Elle ne dépasse jamais le vrai coût, si bien que le premier trajet sorti de la file est le moins cher.
        /// </summary>
        public int Estimate(Vector2Int cell)
        {
            int dx = Math.Max(0, Math.Max(_min.x - cell.x, cell.x - _max.x));
            int dy = Math.Max(0, Math.Max(_min.y - cell.y, cell.y - _max.y));
            return PathFinder.StraightCost * Math.Max(dx, dy) +
                   (PathFinder.DiagonalCost - PathFinder.StraightCost) * Math.Min(dx, dy);
        }
    }
}
