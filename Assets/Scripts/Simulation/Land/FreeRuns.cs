using System;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les tronçons du maillage, d'où se font les îlots (SAVOIR, 2.1) : dans chaque rangée, d'ouest en est, les suites de
    /// mailles libres. Ils se relèvent quand on les lit et que la version du maillage a changé, mais seulement dans les
    /// rangées qui ont changé depuis le dernier relevé (le maillage le dit, rangée par rangée) ; les autres se reprennent
    /// telles quelles. Retirer un arbre de la carte de 500 m relève ainsi quelques rangées, pas quatre millions de
    /// mailles. Les tableaux se gardent d'un relevé à l'autre et ne grandissent qu'au besoin.
    /// </summary>
    internal sealed class FreeRuns
    {
        private readonly NavGrid _grid;

        // Les tronçons de la rangée y vont de _first[y] (compris) à _first[y + 1] (exclu) ; un tronçon couvre les
        // colonnes de _start (comprise) à _end (exclue). Les tableaux « Before » tiennent le relevé précédent.
        private int[] _first;
        private int[] _start;
        private int[] _end;
        private int[] _firstBefore;
        private int[] _startBefore;
        private int[] _endBefore;
        private bool _read;
        private int _readVersion;

        public FreeRuns(NavGrid grid)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
            _first = new int[grid.Depth + 1];
            _firstBefore = new int[grid.Depth + 1];
            // Une carte vide a un tronçon par rangée ; chaque construction en ajoute un sur chaque rangée qu'elle coupe.
            int capacity = 4 * grid.Depth;
            _start = new int[capacity];
            _end = new int[capacity];
            _startBefore = new int[capacity];
            _endBefore = new int[capacity];
        }

        /// <summary>Le nombre de tronçons du dernier relevé.</summary>
        public int Count { get; private set; }

        /// <summary>Le premier tronçon de cette rangée ; celui de la rangée suivante borne ses tronçons.</summary>
        public int FirstOf(int row)
        {
            return _first[row];
        }

        /// <summary>La première colonne de ce tronçon.</summary>
        public int StartOf(int run)
        {
            return _start[run];
        }

        /// <summary>La colonne qui suit la dernière de ce tronçon.</summary>
        public int EndOf(int run)
        {
            return _end[run];
        }

        /// <summary>Relève les tronçons si le maillage a changé depuis le dernier relevé ; vrai s'il l'a fait.</summary>
        public bool Refresh()
        {
            if (_read && _readVersion == _grid.Version)
                return false;
            Swap();
            Count = 0;
            for (int row = 0; row < _grid.Depth; row++)
            {
                _first[row] = Count;
                if (_read && _grid.RowVersion(row) <= _readVersion)
                    CopyRow(row);
                else
                    ScanRow(row);
            }
            _first[_grid.Depth] = Count;
            _read = true;
            _readVersion = _grid.Version;
            return true;
        }

        // Le relevé courant devient le précédent ; ses tableaux serviront au suivant.
        private void Swap()
        {
            (_first, _firstBefore) = (_firstBefore, _first);
            (_start, _startBefore) = (_startBefore, _start);
            (_end, _endBefore) = (_endBefore, _end);
        }

        private void CopyRow(int row)
        {
            for (int run = _firstBefore[row]; run < _firstBefore[row + 1]; run++)
                Add(_startBefore[run], _endBefore[run]);
        }

        private void ScanRow(int row)
        {
            int width = _grid.Width;
            int index = row * width;
            int x = 0;
            while (x < width)
            {
                if (!_grid.IsWalkable(index + x))
                {
                    x++;
                    continue;
                }
                int start = x;
                while (x < width && _grid.IsWalkable(index + x))
                    x++;
                Add(start, x);
            }
        }

        private void Add(int start, int end)
        {
            if (Count == _start.Length)
            {
                Array.Resize(ref _start, 2 * Count);
                Array.Resize(ref _end, 2 * Count);
            }
            _start[Count] = start;
            _end[Count] = end;
            Count++;
        }
    }
}
