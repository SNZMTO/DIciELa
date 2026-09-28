using System;
using System.Collections.Generic;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// La marche d'un habitant vers une maille (SAVOIR, 2.1) : il suit la ligne tendue de son trajet, et l'avancement se
    /// compte en distance. Le trajet se garde avec la version du maillage où il a été vérifié : quand elle change, on
    /// revérifie le reste de la ligne, et l'on ne cherche un autre trajet que si une construction la coupe, but compris.
    /// La ligne part du milieu de la maille où il se trouve, qu'il a pu dépasser : il la rejoint sans reculer. Issue
    /// d'échec : le but n'est plus joignable, et la marche est perdue. Le trajet et ses listes se gardent d'une marche à
    /// l'autre : marcher n'alloue rien.
    /// </summary>
    internal sealed class Walk
    {
        private readonly NavGrid _grid;
        private readonly PathFinder _finder;
        private readonly Path _path = new Path();
        private readonly Vector2Int[] _start = new Vector2Int[1];
        private readonly Vector2Int[] _goal = new Vector2Int[1];
        private int _next;
        private Vector2 _join;
        private int _checkedVersion;

        public Walk(NavGrid grid, PathFinder finder)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (ReferenceEquals(finder, null))
                throw new ArgumentNullException(nameof(finder));

            _grid = grid;
            _finder = finder;
        }

        /// <summary>Vrai tant qu'il marche vers son but.</summary>
        public bool IsWalking { get; private set; }

        /// <summary>La maille visée par la dernière marche.</summary>
        public Vector2Int Goal => _goal[0];

        /// <summary>Le trajet suivi, dont la ligne tendue ; il ne vaut que pendant la marche.</summary>
        public Path Path => _path;

        /// <summary>Part de ce point vers le milieu de cette maille ; faux si elle n'est pas joignable d'ici.</summary>
        public bool TryStart(Vector2 from, Vector2Int goal)
        {
            _goal[0] = goal;
            return TryResume(from);
        }

        /// <summary>Repart de ce point vers le même but, par un nouveau trajet ; faux s'il n'est plus joignable.</summary>
        public bool TryResume(Vector2 from)
        {
            _start[0] = _grid.CellOf(from);
            _checkedVersion = _grid.Version;
            _next = 0;
            IsWalking = _finder.TryFindPath(_start, _goal, _path);
            if (IsWalking)
                _join = JoinPoint(from);
            return IsWalking;
        }

        /// <summary>
        /// Où il rejoint sa ligne, qui part du milieu de sa maille : au point de son premier segment le plus proche de
        /// lui, sans sortir de sa maille. Il ne recule donc pas vers ce milieu s'il l'a dépassé, et ne quitte jamais le
        /// sol libre : sa maille, puis la ligne vérifiée.
        /// </summary>
        private Vector2 JoinPoint(Vector2 from)
        {
            IReadOnlyList<Vector2> corners = _path.Corners;
            Vector2 start = corners[0];
            if (corners.Count < 2)
                return start;
            Vector2 segment = corners[1] - start;
            float length = segment.magnitude;
            Vector2 along = segment / length;
            // Le segment quitte la maille à une demi-maille de son milieu, sur l'axe où il avance le plus.
            float inCell = 0.5f * _grid.Metrics.CellSize / Mathf.Max(Mathf.Abs(along.x), Mathf.Abs(along.y));
            float reach = Mathf.Clamp(Vector2.Dot(from - start, along), 0f, Mathf.Min(inCell, length));
            return start + along * reach;
        }

        /// <summary>Cesse de marcher, là où il est.</summary>
        public void Stop()
        {
            IsWalking = false;
        }

        /// <summary>
        /// Avance le marcheur de cette distance le long de sa ligne, d'un coin à l'autre. Si le maillage a changé et
        /// qu'une construction coupe le reste de la ligne, il cherche d'abord un autre trajet, depuis là où il est.
        /// </summary>
        public WalkOutcome Advance(ref Vector2 position, float meters)
        {
            if (!IsWalking)
                throw new InvalidOperationException("Une marche arrêtée n'avance plus.");
            if (_grid.Version != _checkedVersion)
            {
                _checkedVersion = _grid.Version;
                if (!RestIsClear() && !TryResume(position))
                    return WalkOutcome.Lost;
            }

            IReadOnlyList<Vector2> corners = _path.Corners;
            float left = meters;
            // Chaque tour atteint un coin, ou finit le pas au milieu d'un segment : au plus un tour par coin. Le premier
            // coin visé est le point où il rejoint sa ligne.
            while (_next < corners.Count)
            {
                Vector2 corner = _next == 0 ? _join : corners[_next];
                float distance = Vector2.Distance(position, corner);
                if (distance > left)
                {
                    position += (corner - position) * (left / distance);
                    return WalkOutcome.Walking;
                }
                position = corner;
                left -= distance;
                _next++;
            }
            IsWalking = false;
            return WalkOutcome.Arrived;
        }

        /// <summary>
        /// Vrai si le reste de la ligne ne touche que des mailles libres : le segment où il marche, les suivants, et la
        /// maille du but. Le segment en cours est vérifié en entier, la part déjà faite comprise : au pire, un trajet
        /// cherché pour rien.
        /// </summary>
        private bool RestIsClear()
        {
            IReadOnlyList<Vector2> corners = _path.Corners;
            for (int i = Math.Max(_next - 1, 0); i < corners.Count - 1; i++)
            {
                if (!StraightLine.IsClear(_grid, _grid.CellOf(corners[i]), _grid.CellOf(corners[i + 1])))
                    return false;
            }
            return _grid.IsWalkable(_goal[0]);
        }
    }
}
