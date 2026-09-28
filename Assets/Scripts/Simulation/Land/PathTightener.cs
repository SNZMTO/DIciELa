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
    /// la ligne tendue est libre, et elle n'est jamais plus longue que l'escalier. Les listes se gardent d'un trajet à
    /// l'autre.
    /// </summary>
    internal sealed class PathTightener
    {
        private readonly NavGrid _grid;
        private readonly List<Vector2Int> _points = new List<Vector2Int>();
        private readonly List<Vector2Int> _pulled = new List<Vector2Int>();

        public PathTightener(NavGrid grid)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
        }

        /// <summary>Tire la ligne tendue de l'escalier du trajet et la range dans ses coins.</summary>
        public void Tighten(Path path)
        {
            TurnsOf(path.Cells, _points);
            Pull(_points, _pulled);
            _pulled.Reverse();
            // Tirée depuis l'arrivée, la ficelle revient dans _points, de l'arrivée au départ.
            Pull(_pulled, _points);
            for (int i = _points.Count - 1; i >= 0; i--)
                path.AddCorner(_grid.CellCenter(_points[i]));
        }

        /// <summary>Les deux bouts de l'escalier, et entre eux les mailles où il change de direction.</summary>
        private static void TurnsOf(IReadOnlyList<Vector2Int> cells, List<Vector2Int> turns)
        {
            turns.Clear();
            turns.Add(cells[0]);
            for (int i = 1; i < cells.Count - 1; i++)
            {
                if (cells[i] - cells[i - 1] != cells[i + 1] - cells[i])
                    turns.Add(cells[i]);
            }
            if (cells.Count > 1)
                turns.Add(cells[cells.Count - 1]);
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
                if (!StraightLine.IsClear(_grid, fold, next))
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
                if (StraightLine.IsClear(_grid, eye, from + step * middle))
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
