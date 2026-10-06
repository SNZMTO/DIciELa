using System.Collections.Generic;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les règles de la Place (décision 204), une seule fonction par ordre, que lisent l'ordre et l'aperçu. Peindre
    /// ajoute les mailles libres que couvre la forme : une construction, un arbre ou un rocher l'arrêtent, ils font des
    /// trous ; dans la superficie que permet le niveau, la Place restant d'un seul tenant. Gommer retire les mailles de la
    /// Place que couvre la forme, jamais sous une place de charrette, la Place restant d'un seul tenant. Une place de
    /// charrette tient tout entière dans la Place, sans en chevaucher une autre, avec de quoi se tenir devant elle pour
    /// la garer. D'un seul tenant : d'une maille à sa voisine par un côté. Rien n'est alloué une fois les listes à leur
    /// taille.
    /// </summary>
    internal sealed class PlaceChecks
    {
        private readonly NavGrid _grid;
        private readonly List<Vector2Int> _covered = new List<Vector2Int>();
        private readonly HashSet<int> _added = new HashSet<int>();
        private readonly HashSet<int> _removed = new HashSet<int>();
        private readonly HashSet<int> _visited = new HashSet<int>();
        private readonly Queue<int> _queue = new Queue<int>();

        public PlaceChecks(NavGrid grid)
        {
            _grid = grid;
        }

        /// <summary>Ce que peindre cette forme ajouterait à la Place, mis dans cette liste ; et si on le peut.</summary>
        public PlaceResult Paint(GroundShape shape, HashSet<int> zone, int maxCells, List<int> added)
        {
            added.Clear();
            Cover(shape);
            for (int i = 0; i < _covered.Count; i++)
            {
                // Une maille de la Place n'est pas libre : son occupant est la Place.
                if (_grid.OccupantAt(_covered[i]) == 0)
                    added.Add(_grid.IndexOf(_covered[i]));
            }
            if (added.Count == 0)
                return PlaceResult.Nothing;
            if (zone.Count + added.Count > maxCells)
                return PlaceResult.TooLarge;
            return IsOnePiece(zone, added, null) ? PlaceResult.Ok : PlaceResult.NotOnePiece;
        }

        /// <summary>Ce que gommer cette forme retirerait de la Place, mis dans cette liste ; et si on le peut.</summary>
        public PlaceResult Erase(GroundShape shape, HashSet<int> zone, IReadOnlyList<PlaceMark> marks, List<int> removed)
        {
            removed.Clear();
            Cover(shape);
            for (int i = 0; i < _covered.Count; i++)
            {
                int index = _grid.IndexOf(_covered[i]);
                if (zone.Contains(index))
                    removed.Add(index);
            }
            if (removed.Count == 0)
                return PlaceResult.Nothing;
            Fill(_removed, removed);
            for (int i = 0; i < marks.Count; i++)
            {
                foreach (Vector2Int cell in _grid.CellsCoveredBy(marks[i].Footprint))
                {
                    if (_removed.Contains(_grid.IndexOf(cell)))
                        return PlaceResult.MarkInTheWay;
                }
            }
            return IsOnePiece(zone, null, removed) ? PlaceResult.Ok : PlaceResult.NotOnePiece;
        }

        /// <summary>Si une place de charrette se pose à cette place, celui qui la gare se tenant à ce point.</summary>
        public PlaceResult Mark(RotatedRect footprint, Vector2 stand, HashSet<int> zone, IReadOnlyList<PlaceMark> marks,
            int maxMarks)
        {
            if (marks.Count >= maxMarks)
                return PlaceResult.TooManyMarks;
            for (int corner = 0; corner < 4; corner++)
            {
                if (!_grid.Contains(_grid.CellOf(footprint.Corner(corner))))
                    return PlaceResult.OutsideThePlace;
            }
            _added.Clear();
            foreach (Vector2Int cell in _grid.CellsCoveredBy(footprint))
            {
                int index = _grid.IndexOf(cell);
                if (!zone.Contains(index))
                    return PlaceResult.OutsideThePlace;
                _added.Add(index);
            }
            for (int i = 0; i < marks.Count; i++)
            {
                foreach (Vector2Int cell in _grid.CellsCoveredBy(marks[i].Footprint))
                {
                    if (_added.Contains(_grid.IndexOf(cell)))
                        return PlaceResult.MarksOverlap;
                }
            }
            Vector2Int standCell = _grid.CellOf(stand);
            return _grid.Contains(standCell) && _grid.IsWalkable(standCell) ? PlaceResult.Ok : PlaceResult.NoRoomToPark;
        }

        private void Cover(GroundShape shape)
        {
            _covered.Clear();
            shape.CoverCells(_grid, _covered);
        }

        // Vrai si la Place, ces mailles ajoutées et ces mailles retirées, se tient d'un seul morceau ; vide, elle l'est.
        private bool IsOnePiece(HashSet<int> zone, List<int> added, List<int> removed)
        {
            Fill(_added, added);
            Fill(_removed, removed);
            int total = zone.Count + _added.Count - _removed.Count;
            if (total == 0)
                return true;
            int start = added != null && added.Count > 0 ? added[0] : -1;
            if (start < 0)
            {
                foreach (int index in zone)
                {
                    if (_removed.Contains(index))
                        continue;
                    start = index;
                    break;
                }
            }
            _visited.Clear();
            _queue.Clear();
            _visited.Add(start);
            _queue.Enqueue(start);
            int width = _grid.Width, count = _grid.Width * _grid.Depth;
            while (_queue.Count > 0)
            {
                int index = _queue.Dequeue();
                int x = index % width;
                Visit(index - 1, x > 0, zone);
                Visit(index + 1, x < width - 1, zone);
                Visit(index - width, index >= width, zone);
                Visit(index + width, index + width < count, zone);
            }
            return _visited.Count == total;
        }

        private void Visit(int index, bool inside, HashSet<int> zone)
        {
            if (!inside || _visited.Contains(index) || _removed.Contains(index))
                return;
            if (!zone.Contains(index) && !_added.Contains(index))
                return;
            _visited.Add(index);
            _queue.Enqueue(index);
        }

        private static void Fill(HashSet<int> set, List<int> from)
        {
            set.Clear();
            if (from == null)
                return;
            for (int i = 0; i < from.Count; i++)
                set.Add(from[i]);
        }
    }
}
