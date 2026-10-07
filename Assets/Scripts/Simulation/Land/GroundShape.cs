using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Une forme qu'on peint au sol, ou qu'on y gomme (décisions 204 et 216 ; SAVOIR, 2.6) : un trait (à main levée, ou
    /// un tracé), un disque, un rectangle, un polygone. Elle ne dit que les mailles qu'elle couvre, celles du maillage
    /// dont le milieu est en elle ; ce qui l'arrête (une construction, un arbre, un rocher) se décide chez qui la peint.
    /// </summary>
    public abstract class GroundShape
    {
        /// <summary>Ajoute à cette liste les mailles du maillage dont le milieu est dans la forme, chacune une fois.</summary>
        internal abstract void CoverCells(NavGrid grid, List<Vector2Int> cells);

        /// <summary>Les mailles du maillage entre ces deux coins, bornées au maillage : la boîte où chercher.</summary>
        internal static void Bounds(NavGrid grid, Vector2 min, Vector2 max, out Vector2Int from, out Vector2Int to)
        {
            Vector2Int low = grid.CellOf(min);
            Vector2Int high = grid.CellOf(max);
            from = new Vector2Int(Mathf.Max(0, low.x), Mathf.Max(0, low.y));
            to = new Vector2Int(Mathf.Min(grid.Width - 1, high.x), Mathf.Min(grid.Depth - 1, high.y));
        }
    }
}
