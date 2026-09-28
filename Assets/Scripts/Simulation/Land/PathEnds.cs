using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les départs et les arrivées retenus pour une recherche de trajet (SAVOIR, 2.1) : dans le maillage (sinon, une
    /// erreur), libres, et sur un îlot où il y a à la fois un départ et une arrivée. Un départ seul sur son îlot ferait
    /// fouiller tout l'îlot en vain ; une arrivée seule élargirait pour rien la boîte des arrivées. S'il ne reste aucun
    /// départ, le trajet est introuvable, et on le sait sans rien fouiller. Les listes se gardent d'une recherche à
    /// l'autre.
    /// </summary>
    internal sealed class PathEnds
    {
        private readonly NavGrid _grid;
        private readonly Islands _islands;
        private readonly List<Vector2Int> _starts = new List<Vector2Int>();
        private readonly List<Vector2Int> _goals = new List<Vector2Int>();
        private readonly List<int> _startIslands = new List<int>();
        private readonly List<int> _goalIslands = new List<int>();

        public PathEnds(NavGrid grid, Islands islands)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (ReferenceEquals(islands, null))
                throw new ArgumentNullException(nameof(islands));

            _grid = grid;
            _islands = islands;
        }

        public IReadOnlyList<Vector2Int> Starts => _starts;
        public IReadOnlyList<Vector2Int> Goals => _goals;

        /// <summary>Retient les départs et les arrivées ; faux s'il n'en reste aucun.</summary>
        public bool Keep(IReadOnlyList<Vector2Int> starts, IReadOnlyList<Vector2Int> goals)
        {
            CheckInsideGrid(starts, nameof(starts));
            CheckInsideGrid(goals, nameof(goals));
            ReadIslands(starts, _startIslands);
            ReadIslands(goals, _goalIslands);
            KeepSharing(starts, _startIslands, _goalIslands, _starts);
            KeepSharing(goals, _goalIslands, _startIslands, _goals);
            return _starts.Count > 0;
        }

        private void CheckInsideGrid(IReadOnlyList<Vector2Int> cells, string name)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (!_grid.Contains(cells[i]))
                    throw new ArgumentOutOfRangeException(name, "La maille " + cells[i] + " est hors du maillage.");
            }
        }

        /// <summary>L'îlot de chaque maille : 0 pour une maille prise.</summary>
        private void ReadIslands(IReadOnlyList<Vector2Int> cells, List<int> islands)
        {
            islands.Clear();
            for (int i = 0; i < cells.Count; i++)
                islands.Add(_islands.IslandOf(cells[i]));
        }

        /// <summary>Garde les mailles libres dont l'îlot a aussi une maille de l'autre bout.</summary>
        private static void KeepSharing(IReadOnlyList<Vector2Int> cells, List<int> islands, List<int> otherIslands,
            List<Vector2Int> kept)
        {
            kept.Clear();
            for (int i = 0; i < cells.Count; i++)
            {
                if (islands[i] != 0 && otherIslands.Contains(islands[i]))
                    kept.Add(cells[i]);
            }
        }
    }
}
