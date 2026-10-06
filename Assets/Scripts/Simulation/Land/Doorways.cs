using System;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les devants de porte (GDD, section 9) : pour chaque maille du maillage, combien de portes demandent qu'elle reste
    /// dégagée. Ils sont déduits des constructions posées, jamais sauvegardés : les constructions les tiennent à jour, les
    /// règles de pose les lisent, et n'y posent rien ; la Place peut s'y étendre (décision 204). On y marche comme
    /// ailleurs : seul le maillage dit où l'on passe.
    /// </summary>
    public sealed class Doorways
    {
        private readonly NavGrid _grid;
        private readonly byte[] _count;

        public Doorways(NavGrid grid)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
            _count = new byte[grid.Width * grid.Depth];
        }

        /// <summary>Vrai si cette maille doit rester dégagée devant une porte.</summary>
        public bool Covers(Vector2Int cell)
        {
            return _count[_grid.IndexOf(cell)] > 0;
        }

        /// <summary>Combien de portes demandent que cette maille reste dégagée.</summary>
        public int CountAt(Vector2Int cell)
        {
            return _count[_grid.IndexOf(cell)];
        }

        /// <summary>Compte une porte de plus devant chaque maille du rectangle. Seules les constructions l'appellent.</summary>
        internal void Add(RotatedRect rect)
        {
            foreach (Vector2Int cell in _grid.CellsCoveredBy(rect))
            {
                int index = _grid.IndexOf(cell);
                if (_count[index] < byte.MaxValue)
                    _count[index]++;
            }
        }

        /// <summary>
        /// Compte une porte de moins devant chaque maille du rectangle : sa construction a changé de place. Seules les
        /// constructions l'appellent.
        /// </summary>
        internal void Remove(RotatedRect rect)
        {
            foreach (Vector2Int cell in _grid.CellsCoveredBy(rect))
            {
                int index = _grid.IndexOf(cell);
                if (_count[index] > 0)
                    _count[index]--;
            }
        }
    }
}
