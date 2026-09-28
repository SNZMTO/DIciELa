using System;
using System.Collections.Generic;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// L'oracle de la ligne tendue (SAVOIR, 4) : un juge lent et sûr, écrit à part du code testé. Une ligne droite entre
    /// deux milieux de mailles touche une maille quand elle rencontre son carré, bords et coins compris ; le juge essaie
    /// chaque maille autour du segment, par des produits en croix exacts, en entiers : les coordonnées sont doublées,
    /// si bien que le milieu d'une maille tombe sur des nombres impairs et ses bords sur des pairs.
    /// </summary>
    internal static class SlowLines
    {
        private const double Tolerance = 1e-3;

        /// <summary>
        /// Ce qui ne va pas dans la ligne tendue de ce trajet, ou null si elle est juste : elle va du milieu de la
        /// première maille de l'escalier au milieu de la dernière ; chaque coin est au milieu d'une maille, différente
        /// de celle du coin d'avant ; aucun segment ne touche une maille bloquée ; sa longueur est la somme de ses
        /// segments, pas plus que celle de l'escalier, pas moins que la ligne droite d'un bout à l'autre.
        /// </summary>
        public static string Judge(bool[,] blocked, int cellsPerMeter, Path path)
        {
            IReadOnlyList<Vector2Int> cells = path.Cells;
            if (path.Corners.Count == 0)
                return "la ligne tendue est vide";

            var points = new List<Vector2Int>();
            foreach (Vector2 corner in path.Corners)
            {
                if (!TryCellOf(corner, cellsPerMeter, out Vector2Int cell))
                    return "le coin " + corner + " n'est pas au milieu d'une maille";
                points.Add(cell);
            }
            if (points[0] != cells[0])
                return "la ligne tendue ne part pas de la première maille de l'escalier";
            if (points[points.Count - 1] != cells[cells.Count - 1])
                return "la ligne tendue ne finit pas sur la dernière maille de l'escalier";

            double length = 0.0;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i] == points[i - 1])
                    return "deux coins qui se suivent sont sur la même maille " + points[i];
                Vector2Int? touched = FirstBlockedTouch(blocked, points[i - 1], points[i]);
                if (touched.HasValue)
                    return "le segment de " + points[i - 1] + " à " + points[i] + " touche la maille bloquée " +
                           touched.Value;
                length += Distance(points[i - 1], points[i]);
            }

            double cellSize = 1.0 / cellsPerMeter;
            length *= cellSize;
            double stairs = StairsLength(cells) * cellSize;
            double straight = Distance(cells[0], cells[cells.Count - 1]) * cellSize;
            if (Math.Abs(path.Length - length) > Tolerance)
                return "la longueur dite, " + path.Length + " m, n'est pas celle des segments, " + length + " m";
            if (length > stairs + Tolerance)
                return "la ligne tendue (" + length + " m) est plus longue que l'escalier (" + stairs + " m)";
            if (length < straight - Tolerance)
                return "la ligne tendue (" + length + " m) est plus courte que la ligne droite (" + straight + " m)";
            return null;
        }

        /// <summary>Vrai si le segment entre les milieux de from et de to rencontre le carré de cell, bords compris.</summary>
        public static bool Touches(Vector2Int from, Vector2Int to, Vector2Int cell)
        {
            long ax = 2L * from.x + 1;
            long ay = 2L * from.y + 1;
            long bx = 2L * to.x + 1;
            long by = 2L * to.y + 1;
            long left = 2L * cell.x;
            long right = left + 2;
            long bottom = 2L * cell.y;
            long top = bottom + 2;
            if (Math.Max(ax, bx) < left || Math.Min(ax, bx) > right ||
                Math.Max(ay, by) < bottom || Math.Min(ay, by) > top)
                return false;

            // Les quatre coins du carré strictement du même côté de la ligne : elle passe à côté.
            int above = 0;
            int below = 0;
            foreach ((long x, long y) in new[] { (left, bottom), (right, bottom), (left, top), (right, top) })
            {
                long cross = (bx - ax) * (y - ay) - (by - ay) * (x - ax);
                if (cross > 0)
                    above++;
                else if (cross < 0)
                    below++;
            }
            return above < 4 && below < 4;
        }

        /// <summary>La première maille bloquée que touche le segment entre les milieux de from et de to, s'il y en a une.</summary>
        private static Vector2Int? FirstBlockedTouch(bool[,] blocked, Vector2Int from, Vector2Int to)
        {
            // Un segment entre deux milieux ne sort pas de la boîte de leurs deux mailles.
            for (int x = Math.Min(from.x, to.x); x <= Math.Max(from.x, to.x); x++)
            {
                for (int y = Math.Min(from.y, to.y); y <= Math.Max(from.y, to.y); y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (!SlowPaths.IsFree(blocked, cell) && Touches(from, to, cell))
                        return cell;
                }
            }
            return null;
        }

        /// <summary>La maille dont ce point est le milieu, s'il est bien au milieu d'une maille.</summary>
        private static bool TryCellOf(Vector2 meters, int cellsPerMeter, out Vector2Int cell)
        {
            double x = meters.x * cellsPerMeter - 0.5;
            double y = meters.y * cellsPerMeter - 0.5;
            cell = new Vector2Int((int)Math.Round(x), (int)Math.Round(y));
            return Math.Abs(x - cell.x) < Tolerance && Math.Abs(y - cell.y) < Tolerance;
        }

        /// <summary>La longueur de l'escalier, en mailles : 1 par pas droit, racine de 2 par pas en diagonale.</summary>
        private static double StairsLength(IReadOnlyList<Vector2Int> cells)
        {
            double length = 0.0;
            for (int i = 1; i < cells.Count; i++)
                length += Distance(cells[i - 1], cells[i]);
            return length;
        }

        private static double Distance(Vector2Int a, Vector2Int b)
        {
            double dx = b.x - a.x;
            double dy = b.y - a.y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
