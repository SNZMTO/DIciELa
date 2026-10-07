using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Tend un trajet comme une ficelle (SAVOIR, 2.1). La recherche rend un escalier de mailles ; on n'en garde que les
    /// points où la ficelle plie, reliés en ligne droite. On part des deux bouts de l'escalier et des mailles où il
    /// change de direction : entre deux de ces points, l'escalier va tout droit par des pas pareils, et la ligne droite
    /// qui les joint ne touche que ses mailles (en diagonale, aussi les coins que la recherche a vérifiés libres). On
    /// tire la ficelle depuis le départ, puis depuis l'arrivée : de chaque bout, on file de point en point tant que la
    /// ligne droite depuis le dernier pli reste libre ; au premier point caché, la ficelle plie sur le segment qui y
    /// mène, au plus loin qu'on le voit. Tirer depuis l'arrivée ramène un pli qui dépassait un coin. Chaque segment de
    /// la ligne tendue est libre, et elle n'est jamais plus longue que l'escalier. Le trajet d'une charrette se tend
    /// morceau par morceau, chacun fait des mailles qui lui coûtent le même prix, et sa ficelle n'y traverse que des
    /// mailles qui ne coûtent pas plus : elle reste sur sa route, et ne coupe pas l'herbe entre deux bouts de chemin.
    /// Les listes se gardent d'un trajet à l'autre.
    /// </summary>
    internal sealed class PathTightener
    {
        private readonly NavGrid _grid;
        private readonly List<Vector2Int> _points = new List<Vector2Int>();
        private readonly List<Vector2Int> _pulled = new List<Vector2Int>();
        // Le pas de la charrette dont on tend le trajet, et le prix du morceau tendu ; sans charrette, null.
        private CartPace _pace;
        private int _highestCost;

        public PathTightener(NavGrid grid)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
        }

        /// <summary>
        /// Tire la ligne tendue de l'escalier du trajet et la range dans ses coins ; pour une charrette (son pas donné),
        /// morceau par morceau de même prix. Un piéton n'a qu'un morceau.
        /// </summary>
        public void Tighten(Path path, CartPace pace)
        {
            IReadOnlyList<Vector2Int> cells = path.Cells;
            _pace = pace;
            // Chaque tour tend un morceau d'au moins une maille : au plus un tour par maille.
            int first = 0;
            while (first < cells.Count)
            {
                _highestCost = CostAt(cells[first]);
                int last = first;
                while (last + 1 < cells.Count && CostAt(cells[last + 1]) == _highestCost)
                    last++;
                TightenPiece(path, first, last);
                first = last + 1;
            }
        }

        // Tend le morceau de l'escalier de first à last et range ses coins au bout de ceux du trajet.
        private void TightenPiece(Path path, int first, int last)
        {
            TurnsOf(path.Cells, first, last, _points);
            Pull(_points, _pulled);
            _pulled.Reverse();
            // Tirée depuis l'arrivée, la ficelle revient dans _points, de l'arrivée au départ.
            Pull(_pulled, _points);
            for (int i = _points.Count - 1; i >= 0; i--)
                path.AddCorner(_grid.CellCenter(_points[i]));
        }

        // Le prix de cette maille pour la charrette ; pour un piéton, le même partout.
        private int CostAt(Vector2Int cell)
        {
            return _pace == null ? 0 : _pace.CostAt(_grid.IndexOf(cell));
        }

        /// <summary>Les deux bouts du morceau d'escalier, et entre eux les mailles où il change de direction.</summary>
        private static void TurnsOf(IReadOnlyList<Vector2Int> cells, int first, int last, List<Vector2Int> turns)
        {
            turns.Clear();
            turns.Add(cells[first]);
            for (int i = first + 1; i < last; i++)
            {
                if (cells[i] - cells[i - 1] != cells[i + 1] - cells[i])
                    turns.Add(cells[i]);
            }
            if (last > first)
                turns.Add(cells[last]);
        }

        /// <summary>
        /// Tire une ligne brisée dont chaque segment est libre, depuis son premier point : on file de point en point
        /// tant que la ligne droite depuis le dernier pli reste libre ; au premier point caché, la ficelle plie sur le
        /// segment qui y mène, au plus loin qu'on le voit depuis le dernier pli.
        /// </summary>
        private void Pull(List<Vector2Int> points, List<Vector2Int> pulled)
        {
            pulled.Clear();
            pulled.Add(points[0]);
            if (points.Count == 1)
                return;

            Vector2Int fold = points[0];
            // Le dernier point qu'on voit depuis le dernier pli ; au début, le bout du premier segment, libre.
            Vector2Int seen = points[1];
            for (int i = 2; i < points.Count; i++)
            {
                Vector2Int next = points[i];
                if (!StraightLine.IsClear(_grid, fold, next, _pace, _highestCost))
                {
                    fold = FurthestSeen(fold, seen, next);
                    pulled.Add(fold);
                }
                seen = next;
            }
            pulled.Add(seen);
        }

        /// <summary>
        /// Le point du segment de from à to le plus près de to (to exclu) qu'on voit depuis eye en ligne droite, parmi
        /// ceux où le segment passe par le milieu d'une maille ; from au moins, qu'on voit déjà. Pris sur le segment,
        /// le pli raccourcit la ficelle, et le reste du segment, jusqu'à to, reste libre.
        /// </summary>
        private Vector2Int FurthestSeen(Vector2Int eye, Vector2Int from, Vector2Int to)
        {
            Vector2Int delta = to - from;
            int count = GreatestCommonDivisor(Math.Abs(delta.x), Math.Abs(delta.y));
            var step = new Vector2Int(delta.x / count, delta.y / count);
            int seen = 0;
            int hidden = count;
            // L'écart entre le point vu et le point caché se coupe en deux à chaque tour.
            while (hidden - seen > 1)
            {
                int middle = (seen + hidden) / 2;
                if (StraightLine.IsClear(_grid, eye, from + step * middle, _pace, _highestCost))
                    seen = middle;
                else
                    hidden = middle;
            }
            return from + step * seen;
        }

        private static int GreatestCommonDivisor(int a, int b)
        {
            // Le reste diminue à chaque tour.
            while (b != 0)
            {
                int rest = a % b;
                a = b;
                b = rest;
            }
            return a;
        }
    }
}
