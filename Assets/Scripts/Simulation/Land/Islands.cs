using System;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les îlots du maillage (SAVOIR, 2.1) : le sol libre découpé en morceaux d'un seul tenant. Deux mailles d'un même
    /// îlot se rejoignent à pied ; deux mailles d'îlots différents, jamais, et la recherche des trajets le sait sans
    /// rien fouiller. Comme une diagonale ne coupe jamais un coin, se rejoindre en huit directions revient à se
    /// rejoindre en quatre : les îlots se font donc rangée par rangée. Chaque tronçon de mailles libres d'une rangée se
    /// relie aux tronçons qu'il touche dans la rangée d'en dessous, par un union-find : chaque tronçon pointe vers un
    /// tronçon plus ancien de son îlot, et ainsi de suite jusqu'au plus ancien, qui représente l'îlot. Les îlots se
    /// déduisent du maillage, ne sont jamais sauvegardés, et se refont quand on les lit et que la version du maillage a
    /// changé : une seule fois pour tout ce qu'un pas a posé. Les tableaux se gardent d'une fois à l'autre et ne
    /// grandissent qu'au besoin.
    /// </summary>
    public sealed class Islands
    {
        private readonly NavGrid _grid;

        // Les tronçons de la rangée y vont de _firstRun[y] (compris) à _firstRun[y + 1] (exclu), d'ouest en est ; un
        // tronçon couvre les colonnes de _start (comprise) à _end (exclue).
        private readonly int[] _firstRun;
        private int[] _start;
        private int[] _end;
        private int[] _parent;
        private int[] _island;
        private int _runs;
        private bool _labeled;
        private int _labeledVersion;

        public Islands(NavGrid grid)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
            _firstRun = new int[grid.Depth + 1];
            // Une carte vide a un tronçon par rangée ; chaque construction en ajoute un sur chaque rangée qu'elle coupe.
            int capacity = 4 * grid.Depth;
            _start = new int[capacity];
            _end = new int[capacity];
            _parent = new int[capacity];
            _island = new int[capacity];
        }

        /// <summary>
        /// L'îlot de cette maille : un numéro à partir de 1, le même pour toutes les mailles qu'on peut rejoindre à pied
        /// depuis elle ; 0 si elle est prise. Hors du maillage, une erreur.
        /// </summary>
        public int IslandOf(Vector2Int cell)
        {
            int index = _grid.IndexOf(cell);
            if (!_grid.IsWalkable(index))
                return 0;
            Refresh();

            // La maille est libre : le dernier tronçon de sa rangée qui commence avant elle, ou sur elle, la contient.
            // L'intervalle cherché rétrécit de moitié à chaque tour.
            int first = _firstRun[cell.y];
            int last = _firstRun[cell.y + 1] - 1;
            while (first < last)
            {
                int middle = (first + last + 1) / 2;
                if (_start[middle] <= cell.x)
                    first = middle;
                else
                    last = middle - 1;
            }
            return _island[first];
        }

        private void Refresh()
        {
            if (_labeled && _labeledVersion == _grid.Version)
                return;
            Label();
            _labeled = true;
            _labeledVersion = _grid.Version;
        }

        /// <summary>Relève les tronçons rangée par rangée, relie chacun à ceux qu'il touche en dessous, puis numérote.</summary>
        private void Label()
        {
            _runs = 0;
            int width = _grid.Width;
            for (int y = 0; y < _grid.Depth; y++)
            {
                int below = y > 0 ? _firstRun[y - 1] : 0;
                int belowEnd = _runs;
                _firstRun[y] = _runs;
                int row = y * width;
                int x = 0;
                while (x < width)
                {
                    if (!_grid.IsWalkable(row + x))
                    {
                        x++;
                        continue;
                    }
                    int start = x;
                    while (x < width && _grid.IsWalkable(row + x))
                        x++;
                    int run = AddRun(start, x);

                    // Un tronçon d'en dessous fini avant celui-ci ne touchera pas non plus les suivants de la rangée.
                    while (below < belowEnd && _end[below] <= start)
                        below++;
                    for (int other = below; other < belowEnd && _start[other] < x; other++)
                        Join(run, other);
                }
            }
            _firstRun[_grid.Depth] = _runs;
            Number();
        }

        private int AddRun(int start, int end)
        {
            if (_runs == _start.Length)
                Grow();
            _start[_runs] = start;
            _end[_runs] = end;
            _parent[_runs] = _runs;
            return _runs++;
        }

        private void Grow()
        {
            int capacity = 2 * _start.Length;
            Array.Resize(ref _start, capacity);
            Array.Resize(ref _end, capacity);
            Array.Resize(ref _parent, capacity);
            Array.Resize(ref _island, capacity);
        }

        /// <summary>Met deux tronçons dans le même îlot : le représentant le plus récent pointe vers le plus ancien.</summary>
        private void Join(int a, int b)
        {
            int rootA = Root(a);
            int rootB = Root(b);
            if (rootA < rootB)
                _parent[rootB] = rootA;
            else if (rootB < rootA)
                _parent[rootA] = rootB;
        }

        /// <summary>
        /// Le représentant de l'îlot d'un tronçon. Chaque tronçon pointe vers un plus ancien que lui, ou vers lui-même
        /// s'il représente son îlot : le numéro baisse à chaque saut, et la remontée finit. Au passage, chaque tronçon
        /// saute un cran, pour que les remontées suivantes soient plus courtes.
        /// </summary>
        private int Root(int run)
        {
            while (_parent[run] != run)
            {
                _parent[run] = _parent[_parent[run]];
                run = _parent[run];
            }
            return run;
        }

        /// <summary>
        /// Numérote les îlots à partir de 1, dans l'ordre de leur plus ancien tronçon. Un tronçon pointe vers un plus
        /// ancien du même îlot, déjà numéroté : il prend son numéro.
        /// </summary>
        private void Number()
        {
            int islands = 0;
            for (int run = 0; run < _runs; run++)
            {
                int parent = _parent[run];
                _island[run] = parent == run ? ++islands : _island[parent];
            }
        }
    }
}
