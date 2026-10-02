using System;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les îlots du maillage (SAVOIR, 2.1) : le sol libre découpé en morceaux d'un seul tenant. Deux mailles d'un même
    /// îlot se rejoignent à pied ; deux mailles d'îlots différents, jamais, et la recherche des trajets le sait sans
    /// rien fouiller. Comme une diagonale ne coupe jamais un coin, se rejoindre en huit directions revient à se
    /// rejoindre en quatre : les îlots se font donc rangée par rangée. Chaque tronçon de mailles libres d'une rangée
    /// (FreeRuns) se relie aux tronçons qu'il touche dans la rangée d'en dessous, par un union-find : chaque tronçon
    /// pointe vers un tronçon plus ancien de son îlot, et ainsi de suite jusqu'au plus ancien, qui représente l'îlot. Les
    /// îlots se déduisent du maillage, ne sont jamais sauvegardés, et se refont quand on les lit et que la version du
    /// maillage a changé : une seule fois pour tout ce qu'un pas a posé, sur les tronçons, sans relire les rangées qui
    /// n'ont pas changé. Chacun sait combien il a de mailles. Les tableaux se gardent d'une fois à l'autre et ne
    /// grandissent qu'au besoin.
    /// </summary>
    public sealed class Islands
    {
        private readonly NavGrid _grid;
        private readonly FreeRuns _runs;

        // Par tronçon : celui, plus ancien ou lui-même, vers qui il pointe, et le numéro de son îlot.
        private int[] _parent;
        private int[] _island;
        // Le nombre de mailles de chaque îlot, par son numéro (à partir de 1) : il y a au plus un îlot par tronçon.
        private int[] _size;
        private int _islandCount;

        public Islands(NavGrid grid)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
            _runs = new FreeRuns(grid);
            int capacity = 4 * grid.Depth;
            _parent = new int[capacity];
            _island = new int[capacity];
            _size = new int[capacity + 1];
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
            int first = _runs.FirstOf(cell.y);
            int last = _runs.FirstOf(cell.y + 1) - 1;
            while (first < last)
            {
                int middle = (first + last + 1) / 2;
                if (_runs.StartOf(middle) <= cell.x)
                    first = middle;
                else
                    last = middle - 1;
            }
            return _island[first];
        }

        /// <summary>
        /// Le nombre de mailles de cet îlot ; 0 pour l'îlot 0, celui des mailles prises. Un numéro sans îlot : une erreur.
        /// </summary>
        public int SizeOf(int island)
        {
            Refresh();
            if (island < 0 || island > _islandCount)
                throw new ArgumentOutOfRangeException(nameof(island), "L'îlot " + island + " n'existe pas.");
            return island == 0 ? 0 : _size[island];
        }

        private void Refresh()
        {
            if (_runs.Refresh())
                Label();
        }

        /// <summary>Relie chaque tronçon à ceux qu'il touche dans la rangée d'en dessous, puis numérote.</summary>
        private void Label()
        {
            int runs = _runs.Count;
            if (_parent.Length < runs)
                Grow(runs);
            for (int run = 0; run < runs; run++)
                _parent[run] = run;

            for (int y = 1; y < _grid.Depth; y++)
            {
                int below = _runs.FirstOf(y - 1);
                int belowEnd = _runs.FirstOf(y);
                int rowEnd = _runs.FirstOf(y + 1);
                for (int run = belowEnd; run < rowEnd; run++)
                {
                    int start = _runs.StartOf(run);
                    int end = _runs.EndOf(run);
                    // Un tronçon d'en dessous fini avant celui-ci ne touchera pas non plus les suivants de la rangée.
                    while (below < belowEnd && _runs.EndOf(below) <= start)
                        below++;
                    for (int other = below; other < belowEnd && _runs.StartOf(other) < end; other++)
                        Join(run, other);
                }
            }
            Number(runs);
        }

        private void Grow(int runs)
        {
            int capacity = Math.Max(runs, 2 * _parent.Length);
            Array.Resize(ref _parent, capacity);
            Array.Resize(ref _island, capacity);
            Array.Resize(ref _size, capacity + 1);
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
        /// Numérote les îlots à partir de 1, dans l'ordre de leur plus ancien tronçon, et compte leurs mailles. Un
        /// tronçon pointe vers un plus ancien du même îlot, déjà numéroté : il prend son numéro.
        /// </summary>
        private void Number(int runs)
        {
            int islands = 0;
            for (int run = 0; run < runs; run++)
            {
                int parent = _parent[run];
                if (parent == run)
                    _size[++islands] = 0;
                _island[run] = parent == run ? islands : _island[parent];
                _size[_island[run]] += _runs.EndOf(run) - _runs.StartOf(run);
            }
            _islandCount = islands;
        }
    }
}
