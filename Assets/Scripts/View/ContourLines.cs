using System.Collections.Generic;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Le bord d'un champ lissé (CellField ; décision 227), en lignes brisées fermées drapées sur le relief, par la
    /// méthode des « carrés qui marchent » (ContourSquares). Dans chaque carré, un trait entre par le côté qui va d'un
    /// coin dehors à un coin dedans et sort par celui qui va d'un coin dedans à un coin dehors : le dedans à sa gauche.
    /// Le côté par où un trait sort est celui par où le suivant entre, dans le carré voisin : on les chaîne jusqu'à
    /// revenir au départ, et chaque chaîne devient une ligne aux angles joints. Une selle a deux traits : reliée par
    /// son milieu, ils tournent autour de ses coins dehors, sinon autour de ses coins dedans. Ses listes servent d'un
    /// bord à l'autre.
    /// </summary>
    internal sealed class ContourLines
    {
        private readonly List<long> _starts = new List<long>();
        private readonly List<long> _ends = new List<long>();
        private readonly List<bool> _done = new List<bool>();
        private readonly Dictionary<long, int> _startingAt = new Dictionary<long, int>();
        private readonly Dictionary<long, Vector2> _points = new Dictionary<long, Vector2>();
        private readonly List<Vector2> _polyline = new List<Vector2>();
        private readonly bool[] _inside = new bool[4];

        /// <summary>Le bord de ce champ, en lignes brisées fermées de cette largeur.</summary>
        public void AddOutline(DrapedMesh mesh, CellField field, float width)
        {
            _starts.Clear();
            _ends.Clear();
            _startingAt.Clear();
            _points.Clear();
            for (int j = 0; j < field.CornersY - 1; j++)
            {
                for (int i = 0; i < field.CornersX - 1; i++)
                    OutlineSquare(field, i, j);
            }
            _done.Clear();
            for (int k = 0; k < _starts.Count; k++)
                _done.Add(false);
            for (int first = 0; first < _starts.Count; first++)
            {
                if (_done[first])
                    continue;
                _polyline.Clear();
                int segment = first;
                while (segment >= 0 && !_done[segment])
                {
                    _done[segment] = true;
                    _polyline.Add(_points[_starts[segment]]);
                    segment = _startingAt.TryGetValue(_ends[segment], out int next) ? next : -1;
                }
                mesh.AddPolyline(_polyline, true, width);
            }
        }

        private void OutlineSquare(CellField field, int i, int j)
        {
            int count = ContourSquares.Classify(field, i, j, _inside);
            if (count == 0 || count == 4)
                return;
            bool saddle = ContourSquares.IsSaddle(count, _inside);
            bool joined = saddle && ContourSquares.CenterInside(field, i, j);
            for (int exit = 0; exit < 4; exit++)
            {
                if (!_inside[exit] || _inside[(exit + 1) % 4])
                    continue;
                int entry;
                if (saddle)
                    entry = joined ? (exit + 1) % 4 : (exit + 3) % 4;
                else
                {
                    entry = (exit + 3) % 4;
                    while (_inside[entry] || !_inside[(entry + 1) % 4])
                        entry = (entry + 3) % 4;
                }
                AddSegment(field, i, j, entry, exit);
            }
        }

        private void AddSegment(CellField field, int i, int j, int entry, int exit)
        {
            long from = ContourSquares.SideKey(field, i, j, entry), to = ContourSquares.SideKey(field, i, j, exit);
            _points[from] = ContourSquares.SidePoint(field, i, j, entry);
            _points[to] = ContourSquares.SidePoint(field, i, j, exit);
            _startingAt[from] = _starts.Count;
            _starts.Add(from);
            _ends.Add(to);
        }
    }
}
