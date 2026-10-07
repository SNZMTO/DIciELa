using System;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Où servent les sols, à gros grain (SAVOIR, 2.1) : la carte coupée en blocs de 1 m, et par bloc le nombre de mailles
    /// où un sol sert (bâti, ou qu'on efface). L'estimation de la charrette en tire la distance de chaque bloc au sol le
    /// plus proche ; sa version ne change que lorsqu'un bloc gagne son premier sol ou perd son dernier. Seul le sol de
    /// chaque maille (FloorCells) l'écrit.
    /// </summary>
    internal sealed class FloorBlocks
    {
        // Le côté d'un bloc : assez petit pour une estimation serrée, assez grand pour que la distance se refasse vite.
        private const float BlockMeters = 1f;

        private readonly NavGrid _grid;
        private readonly int[] _serving;

        public FloorBlocks(NavGrid grid)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Size = Math.Max(1, (int)Math.Round(BlockMeters / grid.Metrics.CellSize));
            Columns = (grid.Width + Size - 1) / Size;
            Rows = (grid.Depth + Size - 1) / Size;
            _serving = new int[Columns * Rows];
        }

        /// <summary>Le côté d'un bloc, en mailles.</summary>
        public int Size { get; }

        public int Columns { get; }
        public int Rows { get; }

        /// <summary>Change chaque fois qu'un bloc gagne son premier sol qui sert, ou perd son dernier.</summary>
        public int Version { get; private set; }

        /// <summary>Le bloc de la maille à cette place du maillage.</summary>
        public int BlockOf(int index)
        {
            return index / _grid.Width / Size * Columns + index % _grid.Width / Size;
        }

        /// <summary>Vrai si un sol sert sur au moins une maille de ce bloc.</summary>
        public bool Serves(int block)
        {
            return _serving[block] > 0;
        }

        /// <summary>Un sol se met à servir sur cette maille (+1), ou cesse d'y servir (-1).</summary>
        public void Count(int index, int sign)
        {
            int block = BlockOf(index);
            bool before = _serving[block] > 0;
            _serving[block] += sign;
            if (before != _serving[block] > 0)
                Version++;
        }
    }
}
