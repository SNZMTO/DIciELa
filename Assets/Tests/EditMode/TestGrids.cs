using System;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>Les petits gestes communs des tests des trajets et de la marche.</summary>
    internal static class TestGrids
    {
        /// <summary>Les mailles prises du maillage, pour juger un trajet autour d'autre chose que des blocs dessinés.</summary>
        public static bool[,] Occupied(NavGrid grid)
        {
            var occupied = new bool[grid.Width, grid.Depth];
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Depth; y++)
                    occupied[x, y] = grid.OccupantAt(new Vector2Int(x, y)) != 0;
            }
            return occupied;
        }

        /// <summary>Joue des pas jusqu'à ce que done réponde vrai, au plus maxSteps ; rend le nombre de pas joués.</summary>
        public static int StepUntil(World world, Func<bool> done, int maxSteps, string what)
        {
            for (int step = 0; step < maxSteps; step++)
            {
                if (done())
                    return step;
                world.Step();
            }
            Assert.Fail("Au bout de " + maxSteps + " pas, toujours pas : " + what + ".");
            return maxSteps;
        }
    }
}
