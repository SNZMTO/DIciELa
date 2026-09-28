using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// L'oracle des trajets (SAVOIR, 4) : un Dijkstra lent et sûr, sans estimation ni tas (à chaque tour, la maille
    /// ouverte la moins chère, cherchée dans tout le terrain), un juge qui vérifie un escalier pas à pas, et les îlots
    /// remplis pas à pas. Les règles de la marche y sont écrites une seconde fois, à part du code testé : huit voisines,
    /// un pas droit coûte 10, un pas en diagonale 14, une diagonale ne coupe jamais un coin ; un départ ou une arrivée
    /// bloqué ne compte pas.
    /// </summary>
    internal static class SlowPaths
    {
        public const int NoPath = -1;

        /// <summary>Le coût du trajet le moins cher d'un départ à une arrivée, ou <see cref="NoPath"/>.</summary>
        public static int CheapestCost(bool[,] blocked, IReadOnlyList<Vector2Int> starts,
            IReadOnlyList<Vector2Int> goals)
        {
            int width = blocked.GetLength(0);
            int depth = blocked.GetLength(1);
            var cost = new int[width, depth];
            var done = new bool[width, depth];
            var isGoal = new bool[width, depth];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < depth; y++)
                    cost[x, y] = int.MaxValue;
            }
            foreach (Vector2Int goal in goals)
                isGoal[goal.x, goal.y] = true;
            foreach (Vector2Int start in starts)
            {
                if (IsFree(blocked, start))
                    cost[start.x, start.y] = 0;
            }

            // Chaque tour ferme une maille : il y a au plus autant de tours que de mailles.
            for (int round = 0; round < width * depth; round++)
            {
                var best = new Vector2Int(-1, -1);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < depth; y++)
                    {
                        if (!done[x, y] && cost[x, y] != int.MaxValue &&
                            (best.x < 0 || cost[x, y] < cost[best.x, best.y]))
                            best = new Vector2Int(x, y);
                    }
                }
                if (best.x < 0)
                    return NoPath;
                if (isGoal[best.x, best.y])
                    return cost[best.x, best.y];
                done[best.x, best.y] = true;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var next = new Vector2Int(best.x + dx, best.y + dy);
                        int step = StepCost(blocked, best, next);
                        if (step != NoPath && cost[best.x, best.y] + step < cost[next.x, next.y])
                            cost[next.x, next.y] = cost[best.x, best.y] + step;
                    }
                }
            }
            return NoPath;
        }

        /// <summary>
        /// Les îlots selon l'oracle : chaque maille libre reçoit le numéro de l'îlot qu'on remplit pas à pas depuis la
        /// première maille libre pas encore atteinte (huit voisines, sans couper de coin) ; 0 pour une maille bloquée.
        /// </summary>
        public static int[,] Islands(bool[,] blocked)
        {
            int width = blocked.GetLength(0);
            int depth = blocked.GetLength(1);
            var island = new int[width, depth];
            var reached = new Stack<Vector2Int>();
            int count = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < depth; y++)
                {
                    if (blocked[x, y] || island[x, y] != 0)
                        continue;
                    count++;
                    island[x, y] = count;
                    reached.Push(new Vector2Int(x, y));
                    // Chaque maille entre au plus une fois dans la pile.
                    while (reached.Count > 0)
                    {
                        Vector2Int cell = reached.Pop();
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                var next = new Vector2Int(cell.x + dx, cell.y + dy);
                                if (StepCost(blocked, cell, next) == NoPath || island[next.x, next.y] != 0)
                                    continue;
                                island[next.x, next.y] = count;
                                reached.Push(next);
                            }
                        }
                    }
                }
            }
            return island;
        }

        /// <summary>Le coût d'un pas de from à to s'il est permis, sinon <see cref="NoPath"/>.</summary>
        public static int StepCost(bool[,] blocked, Vector2Int from, Vector2Int to)
        {
            int dx = to.x - from.x;
            int dy = to.y - from.y;
            if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1 || (dx == 0 && dy == 0) || !IsFree(blocked, to))
                return NoPath;
            if (dx == 0 || dy == 0)
                return 10;
            bool cutsACorner = !IsFree(blocked, new Vector2Int(to.x, from.y)) ||
                               !IsFree(blocked, new Vector2Int(from.x, to.y));
            return cutsACorner ? NoPath : 14;
        }

        /// <summary>
        /// Ce qui ne va pas dans ce trajet, ou null s'il est juste : il part d'un départ libre, finit sur une arrivée
        /// libre, et chaque pas est permis. Rend aussi ce qu'il coûte.
        /// </summary>
        public static string Judge(bool[,] blocked, IReadOnlyList<Vector2Int> starts, IReadOnlyList<Vector2Int> goals,
            IReadOnlyList<Vector2Int> path, out int cost)
        {
            cost = 0;
            if (path.Count == 0)
                return "le trajet est vide";
            if (!IsFree(blocked, path[0]) || !Contains(starts, path[0]))
                return "le trajet ne part pas d'un départ libre : " + path[0];
            if (!IsFree(blocked, path[path.Count - 1]) || !Contains(goals, path[path.Count - 1]))
                return "le trajet ne finit pas sur une arrivée libre : " + path[path.Count - 1];
            for (int i = 1; i < path.Count; i++)
            {
                int step = StepCost(blocked, path[i - 1], path[i]);
                if (step == NoPath)
                    return "le pas de " + path[i - 1] + " à " + path[i] + " n'est pas permis";
                cost += step;
            }
            return null;
        }

        public static bool IsFree(bool[,] blocked, Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < blocked.GetLength(0) && cell.y < blocked.GetLength(1) &&
                   !blocked[cell.x, cell.y];
        }

        private static bool Contains(IReadOnlyList<Vector2Int> cells, Vector2Int cell)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] == cell)
                    return true;
            }
            return false;
        }
    }
}
