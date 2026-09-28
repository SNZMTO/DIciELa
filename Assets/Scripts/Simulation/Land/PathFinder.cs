using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// La recherche des trajets d'un piéton sur le maillage (SAVOIR, 2.1) : un A*, qui explore d'abord les mailles dont
    /// le coût payé plus le coût restant estimé est le plus petit. Huit voisines : un pas droit coûte 10, un pas en
    /// diagonale 14, et une diagonale ne coupe jamais un coin. Plusieurs départs, plusieurs arrivées ; on n'est arrivé
    /// qu'en sortant une arrivée de la file, jamais en la découvrant. Une instance par maillage : ses tableaux, faits
    /// une fois, servent à chaque recherche sans être vidés (un numéro de recherche dit ce qui est à jour), si bien
    /// qu'une recherche n'alloue rien. Les mêmes entrées rendent toujours le même trajet.
    /// </summary>
    public sealed class PathFinder
    {
        internal const int StraightCost = 10;
        internal const int DiagonalCost = 14;
        private const byte FromStart = byte.MaxValue;

        /// <summary>Les huit pas : les quatre droits, puis les quatre en diagonale.</summary>
        private static readonly Vector2Int[] Steps =
        {
            new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1)
        };

        private readonly NavGrid _grid;
        private readonly PathGoals _goals;
        private readonly PathQueue _open = new PathQueue();
        private readonly int[] _cost;
        private readonly int[] _searchOf;
        private readonly byte[] _from;
        private int _search;

        public PathFinder(NavGrid grid)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
            _goals = new PathGoals(grid);
            int cells = grid.Width * grid.Depth;
            _cost = new int[cells];
            _searchOf = new int[cells];
            _from = new byte[cells];
        }

        /// <summary>
        /// Cherche le trajet le moins cher d'un des départs à une des arrivées et le range dans path, maille par maille,
        /// du départ à l'arrivée comprises ; faux s'il n'y en a pas, et path reste vide. Un départ ou une arrivée sur une
        /// maille prise est ignoré ; une maille hors du maillage est une erreur.
        /// </summary>
        public bool TryFindPath(IReadOnlyList<Vector2Int> starts, IReadOnlyList<Vector2Int> goals,
            List<Vector2Int> path)
        {
            if (ReferenceEquals(starts, null))
                throw new ArgumentNullException(nameof(starts));
            if (ReferenceEquals(goals, null))
                throw new ArgumentNullException(nameof(goals));
            if (ReferenceEquals(path, null))
                throw new ArgumentNullException(nameof(path));
            CheckInsideGrid(starts, nameof(starts));
            CheckInsideGrid(goals, nameof(goals));

            path.Clear();
            BeginSearch();
            if (!_goals.Mark(goals))
                return false;
            PushStarts(starts);
            bool found = Search(out int arrival);
            _goals.Unmark(goals);
            if (found)
                BuildPath(arrival, path);
            return found;
        }

        private void CheckInsideGrid(IReadOnlyList<Vector2Int> cells, string name)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (!_grid.Contains(cells[i]))
                    throw new ArgumentOutOfRangeException(name, "La maille " + cells[i] + " est hors du maillage.");
            }
        }

        /// <summary>Un nouveau numéro de recherche : ce qu'ont écrit les recherches d'avant n'est plus à jour.</summary>
        private void BeginSearch()
        {
            _open.Clear();
            if (_search == int.MaxValue)
            {
                Array.Clear(_searchOf, 0, _searchOf.Length);
                _search = 0;
            }
            _search++;
        }

        private void PushStarts(IReadOnlyList<Vector2Int> starts)
        {
            for (int i = 0; i < starts.Count; i++)
            {
                Vector2Int start = starts[i];
                if (!_grid.IsWalkable(start))
                    continue;
                int index = _grid.IndexOf(start);
                if (_searchOf[index] == _search)
                    continue;
                _searchOf[index] = _search;
                _cost[index] = 0;
                _from[index] = FromStart;
                _open.Push(index, 0, _goals.Estimate(start));
            }
        }

        /// <summary>Sort la maille la plus prometteuse jusqu'à tomber sur une arrivée, ou jusqu'à ce que la file soit vide.</summary>
        private bool Search(out int arrival)
        {
            while (_open.Count > 0)
            {
                _open.Pop(out int index, out int cost);
                // Une entrée périmée : la maille a été rejointe moins cher depuis qu'elle est entrée dans la file.
                if (cost > _cost[index])
                    continue;
                if (_goals.Contains(index))
                {
                    arrival = index;
                    return true;
                }

                Vector2Int cell = _grid.CellAt(index);
                for (int direction = 0; direction < Steps.Length; direction++)
                    TryStep(cell, cost, direction);
            }
            arrival = -1;
            return false;
        }

        /// <summary>Fait un pas depuis cette maille, s'il est permis et s'il rejoint sa voisine moins cher qu'avant.</summary>
        private void TryStep(Vector2Int cell, int cost, int direction)
        {
            Vector2Int step = Steps[direction];
            Vector2Int next = cell + step;
            if (!_grid.Contains(next) || !_grid.IsWalkable(next))
                return;

            bool diagonal = step.x != 0 && step.y != 0;
            // Une diagonale ne coupe jamais un coin : les deux mailles droites qu'elle longe se traversent.
            if (diagonal && (!_grid.IsWalkable(new Vector2Int(next.x, cell.y)) ||
                             !_grid.IsWalkable(new Vector2Int(cell.x, next.y))))
                return;

            int nextCost = cost + (diagonal ? DiagonalCost : StraightCost);
            int index = _grid.IndexOf(next);
            if (_searchOf[index] == _search && _cost[index] <= nextCost)
                return;
            _searchOf[index] = _search;
            _cost[index] = nextCost;
            _from[index] = (byte)direction;
            _open.Push(index, nextCost, nextCost + _goals.Estimate(next));
        }

        /// <summary>Remonte les pas depuis l'arrivée jusqu'à un départ, puis remet le trajet dans le sens de la marche.</summary>
        private void BuildPath(int arrival, List<Vector2Int> path)
        {
            // Un trajet passe au plus une fois par chaque maille : c'est la borne de la remontée.
            int index = arrival;
            for (int i = 0; i < _from.Length; i++)
            {
                Vector2Int cell = _grid.CellAt(index);
                path.Add(cell);
                byte from = _from[index];
                if (from == FromStart)
                {
                    path.Reverse();
                    return;
                }
                index = _grid.IndexOf(cell - Steps[from]);
            }
            throw new InvalidOperationException("Le trajet ne remonte à aucun départ.");
        }
    }
}
