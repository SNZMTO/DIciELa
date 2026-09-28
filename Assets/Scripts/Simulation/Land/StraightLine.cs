using System;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// La ligne droite entre les milieux de deux mailles (SAVOIR, 2.1). Elle est libre si toutes les mailles qu'elle
    /// touche le sont, coins compris : passer par le coin commun de quatre mailles les touche toutes les quatre, la
    /// règle même qui interdit à un pas en diagonale de couper un coin. Le calcul est exact, en entiers : on avance de
    /// maille en maille, en comparant où la ligne croise la prochaine frontière entre deux colonnes et la prochaine
    /// entre deux rangées. Une lecture du maillage : la ligne tendue d'un trajet en est faite, et un marcheur s'en sert
    /// pour savoir si le reste de sa ligne est encore libre.
    /// </summary>
    public static class StraightLine
    {
        /// <summary>Vrai si la ligne droite du milieu de from à celui de to ne touche que des mailles libres.</summary>
        public static bool IsClear(NavGrid grid, Vector2Int from, Vector2Int to)
        {
            int columns = Math.Abs(to.x - from.x);
            int rows = Math.Abs(to.y - from.y);
            var step = new Vector2Int(Math.Sign(to.x - from.x), Math.Sign(to.y - from.y));
            Vector2Int cell = from;
            if (!grid.IsWalkable(cell))
                return false;

            // Chaque tour franchit au moins une frontière : au plus columns + rows tours.
            int crossedColumns = 0;
            int crossedRows = 0;
            while (crossedColumns < columns || crossedRows < rows)
            {
                // La ligne croise sa prochaine frontière entre colonnes à (½ + crossedColumns) / columns de sa longueur,
                // sa prochaine entre rangées à (½ + crossedRows) / rows : on les compare sans diviser.
                int order = (1 + 2 * crossedColumns) * rows - (1 + 2 * crossedRows) * columns;
                if (order == 0)
                {
                    // Par le coin commun de quatre mailles : les deux mailles de côté sont touchées aussi.
                    if (!grid.IsCornerOpen(cell, step))
                        return false;
                    cell += step;
                    crossedColumns++;
                    crossedRows++;
                }
                else if (order < 0)
                {
                    cell.x += step.x;
                    crossedColumns++;
                }
                else
                {
                    cell.y += step.y;
                    crossedRows++;
                }
                if (!grid.IsWalkable(cell))
                    return false;
            }
            return true;
        }
    }
}
