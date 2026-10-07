using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// L'estimation du reste d'un trajet de charrette (SAVOIR, 2.1) : jamais plus que le vrai coût, et au plus près, parce
    /// qu'elle sait où sont les sols. Un trajet qui ne passe sur aucun sol plus rapide que l'herbe paie au moins l'herbe
    /// sur toute la distance qui reste. Un trajet qui en prend un paie au moins le prix le plus bas sur cette distance,
    /// plus la différence avec l'herbe sur ce qu'il doit faire, au moins, pour rejoindre un sol (le pas qui y entre mis à
    /// part) et pour en revenir à l'arrivée la plus proche d'un sol. L'estimation est la plus petite des deux. La
    /// distance au sol le plus proche se lit par blocs de 1 m (FloorBlocks) : refaite en deux passes quand un bloc gagne
    /// ou perd son premier sol, et comptée d'une diagonale de bloc trop courte, pour rester sous la vraie.
    /// </summary>
    internal sealed class CartEstimate
    {
        // Une distance plus grande que toute la carte : aucun sol ne sert.
        private const int Far = int.MaxValue / 4;

        private readonly CartPace _pace;
        private readonly FloorBlocks _blocks;
        // Par bloc, la distance au bloc le plus proche où un sol sert, à 10 par bloc en ligne droite, 14 en diagonale.
        private readonly int[] _distance;
        private int _version = -1;
        private int _grass;
        private int _lowest;
        private long _goalReach;

        public CartEstimate(CartPace pace)
        {
            _pace = pace ?? throw new ArgumentNullException(nameof(pace));
            _blocks = pace.Cells.Blocks;
            _distance = new int[_blocks.Columns * _blocks.Rows];
        }

        /// <summary>
        /// Une recherche commence vers ces arrivées : les prix d'aujourd'hui, et la distance d'elles aux sols.
        /// </summary>
        public void Begin(NavGrid grid, IReadOnlyList<Vector2Int> goals)
        {
            _grass = _pace.GrassCost;
            _lowest = _pace.LowestCost;
            if (_lowest >= _grass)
                return;
            Refresh();
            _goalReach = Far;
            for (int i = 0; i < goals.Count; i++)
                _goalReach = Math.Min(_goalReach, ReachOf(grid.IndexOf(goals[i])));
        }

        /// <summary>
        /// Le coût restant estimé depuis la maille à cette place du maillage, qui est à toGoals de la boîte des arrivées
        /// (à 10 par maille en ligne droite, 14 en diagonale).
        /// </summary>
        public int Estimate(int index, int toGoals)
        {
            long onGrass = (long)_grass * toGoals;
            if (_lowest >= _grass)
                return (int)onGrass;
            long toFloor = Math.Max(0L, ReachOf(index) - PathFinder.DiagonalCost);
            long byFloors = (long)_lowest * toGoals + (long)(_grass - _lowest) * (toFloor + _goalReach);
            return (int)Math.Min(onGrass, byFloors);
        }

        // Ce qu'il faut au moins parcourir de cette maille au sol qui sert le plus proche : la distance des blocs, moins la
        // diagonale d'un bloc (la maille peut être au bord du sien, le sol au bord du sien), en mailles.
        private long ReachOf(int index)
        {
            int distance = _distance[_blocks.BlockOf(index)];
            if (distance >= Far)
                return Far;
            return (long)_blocks.Size * Math.Max(0, distance - PathFinder.DiagonalCost);
        }

        // La distance de chaque bloc au plus proche où un sol sert, en deux passes : depuis le sud-ouest, puis depuis le
        // nord-est. Exacte pour des pas de 10 et 14 sans obstacle, et refaite seulement si un bloc a changé.
        private void Refresh()
        {
            if (_version == _blocks.Version)
                return;
            int columns = _blocks.Columns, rows = _blocks.Rows;
            for (int block = 0; block < _distance.Length; block++)
                _distance[block] = _blocks.Serves(block) ? 0 : Far;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int block = y * columns + x;
                    int best = _distance[block];
                    if (x > 0)
                        best = Math.Min(best, _distance[block - 1] + PathFinder.StraightCost);
                    if (y > 0)
                    {
                        best = Math.Min(best, _distance[block - columns] + PathFinder.StraightCost);
                        if (x > 0)
                            best = Math.Min(best, _distance[block - columns - 1] + PathFinder.DiagonalCost);
                        if (x < columns - 1)
                            best = Math.Min(best, _distance[block - columns + 1] + PathFinder.DiagonalCost);
                    }
                    _distance[block] = best;
                }
            }
            for (int y = rows - 1; y >= 0; y--)
            {
                for (int x = columns - 1; x >= 0; x--)
                {
                    int block = y * columns + x;
                    int best = _distance[block];
                    if (x < columns - 1)
                        best = Math.Min(best, _distance[block + 1] + PathFinder.StraightCost);
                    if (y < rows - 1)
                    {
                        best = Math.Min(best, _distance[block + columns] + PathFinder.StraightCost);
                        if (x < columns - 1)
                            best = Math.Min(best, _distance[block + columns + 1] + PathFinder.DiagonalCost);
                        if (x > 0)
                            best = Math.Min(best, _distance[block + columns - 1] + PathFinder.DiagonalCost);
                    }
                    _distance[block] = best;
                }
            }
            _version = _blocks.Version;
        }
    }
}
