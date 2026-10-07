using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Un polygone qu'on peint au sol, coin par coin (décision 216) : ses coins, en mètres, dans l'ordre ; il couvre les
    /// mailles dont le milieu est dedans, la règle pair-impair tranchant un polygone qui se croise. Ses mailles se
    /// cherchent rangée par rangée : les bords croisés au milieu de la rangée bornent ce qui est dedans.
    /// </summary>
    public sealed class GroundPolygon : GroundShape
    {
        private readonly Vector2[] _corners;
        private readonly List<float> _crossings = new List<float>();

        public GroundPolygon(IReadOnlyList<Vector2> corners)
        {
            if (corners == null || corners.Count < 3)
                throw new ArgumentException("Un polygone a au moins trois coins.", nameof(corners));
            _corners = new Vector2[corners.Count];
            for (int i = 0; i < corners.Count; i++)
                _corners[i] = corners[i];
        }

        public IReadOnlyList<Vector2> Corners => _corners;

        public override GroundShape Copied(GroundCopy copy)
        {
            var corners = new Vector2[_corners.Length];
            for (int i = 0; i < corners.Length; i++)
                corners[i] = copy.Apply(_corners[i]);
            return new GroundPolygon(corners);
        }

        internal override void CoverCells(NavGrid grid, List<Vector2Int> cells)
        {
            Vector2 min = _corners[0], max = min;
            for (int i = 1; i < _corners.Length; i++)
            {
                min = Vector2.Min(min, _corners[i]);
                max = Vector2.Max(max, _corners[i]);
            }
            Bounds(grid, min, max, out Vector2Int from, out Vector2Int to);
            float size = grid.Metrics.CellSize;
            for (int y = from.y; y <= to.y; y++)
            {
                Vector2 first = grid.CellCenter(new Vector2Int(0, y));
                Cross(first.y);
                for (int pair = 0; pair + 1 < _crossings.Count; pair += 2)
                {
                    // Les milieux de la rangée dans [entrée, sortie[ : la colonne x a son milieu en first.x + x × size.
                    int start = Mathf.Max(from.x, Mathf.CeilToInt((_crossings[pair] - first.x) / size));
                    int end = Mathf.Min(to.x, Mathf.CeilToInt((_crossings[pair + 1] - first.x) / size) - 1);
                    for (int x = start; x <= end; x++)
                        cells.Add(new Vector2Int(x, y));
                }
            }
        }

        // Où les bords croisent la ligne de cette hauteur, dans l'ordre ; un bord compte s'il a un bout au-dessus et
        // l'autre pas, pour qu'un coin posé sur la ligne ne compte qu'une fois.
        private void Cross(float height)
        {
            _crossings.Clear();
            for (int i = 0; i < _corners.Length; i++)
            {
                Vector2 a = _corners[i];
                Vector2 b = _corners[(i + 1) % _corners.Length];
                if ((a.y > height) == (b.y > height))
                    continue;
                _crossings.Add(a.x + (height - a.y) * (b.x - a.x) / (b.y - a.y));
            }
            _crossings.Sort();
        }
    }
}
