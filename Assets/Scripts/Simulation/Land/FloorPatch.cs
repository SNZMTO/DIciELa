using System;
using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Un sol en chantier (GDD, section 9 ; décisions 209 et 210) : les mailles d'une forme tracée, qui attendent leurs
    /// bâtisseurs en fantôme, ou celles d'un sol bâti qu'on efface, qui servent jusqu'à la fin de leur petit chantier.
    /// Ses mailles sont la vérité, découpées sur ce qui l'arrêtait ; les sols les tiennent à jour : la gomme retire tout
    /// de suite d'un tracé ses mailles en fantôme.
    /// </summary>
    public sealed class FloorPatch
    {
        private readonly NavGrid _grid;
        private readonly List<int> _cells = new List<int>();

        internal FloorPatch(int id, FloorDefinition floor, NavGrid grid)
        {
            Id = id;
            Floor = floor;
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        }

        /// <summary>Son identifiant dans ce monde ; jamais 0.</summary>
        public int Id { get; }

        /// <summary>Le sol qu'on trace ; null pour un effacement, qui peut prendre plusieurs sortes.</summary>
        public FloorDefinition Floor { get; }

        /// <summary>Vrai pour le petit chantier qui efface un sol bâti.</summary>
        public bool IsErase => Floor == null;

        public int CellCount => _cells.Count;

        public float AreaSquareMeters => _cells.Count * _grid.Metrics.CellSize * _grid.Metrics.CellSize;

        /// <summary>Change chaque fois que la gomme lui retire des mailles : ce qui s'en déduit se garde avec elle.</summary>
        public int Revision { get; private set; }

        /// <summary>Vrai une fois fini (bâti, ou effacé), ou gommé tout entier avant d'être bâti.</summary>
        public bool IsOver { get; private set; }

        /// <summary>Ses mailles, à leur place dans les tableaux du maillage.</summary>
        internal IReadOnlyList<int> Cells => _cells;

        /// <summary>Met dans cette liste ses mailles, dans l'ordre où elles ont été tracées ; la liste est vidée d'abord.</summary>
        public void CopyCells(List<Vector2Int> cells)
        {
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));
            cells.Clear();
            for (int i = 0; i < _cells.Count; i++)
                cells.Add(_grid.CellAt(_cells[i]));
        }

        internal void Add(int index)
        {
            _cells.Add(index);
        }

        /// <summary>La gomme lui a retiré ces mailles : il ne garde que les autres, dans leur ordre.</summary>
        internal void Forget(HashSet<int> removed)
        {
            int kept = 0;
            for (int i = 0; i < _cells.Count; i++)
            {
                if (!removed.Contains(_cells[i]))
                    _cells[kept++] = _cells[i];
            }
            _cells.RemoveRange(kept, _cells.Count - kept);
            Revision++;
        }

        internal void End()
        {
            IsOver = true;
        }
    }
}
