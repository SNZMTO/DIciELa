using System;
using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Le sol de chaque maille (décision 209) : son état et sa sorte, la vérité des sols, à côté du maillage, sans rien
    /// changer à où l'on marche. Deux octets par maille ; une sorte prend sa place la première fois qu'elle est tracée.
    /// Il garde aussi la boîte des mailles changées depuis la dernière fois qu'on l'a prise, pour la vue (SAVOIR, 2.5).
    /// Seuls les sols l'écrivent.
    /// </summary>
    internal sealed class FloorCells
    {
        private readonly NavGrid _grid;
        private readonly byte[] _state;
        // Par maille, la place de sa sorte dans _kinds ; 0 : pas de sol.
        private readonly byte[] _kind;
        private readonly List<FloorDefinition> _kinds = new List<FloorDefinition> { null };
        private Vector2Int _changedMin;
        private Vector2Int _changedMax;
        private bool _changed;

        public FloorCells(NavGrid grid)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _state = new byte[grid.Width * grid.Depth];
            _kind = new byte[grid.Width * grid.Depth];
        }

        public FloorState StateAt(int index)
        {
            return (FloorState)_state[index];
        }

        /// <summary>La sorte du sol de cette maille, quel que soit son état ; null s'il n'y en a pas.</summary>
        public FloorDefinition FloorAt(int index)
        {
            return _kinds[_kind[index]];
        }

        /// <summary>Ce sol est tracé sur cette maille, en fantôme.</summary>
        public void Plan(int index, FloorDefinition floor)
        {
            _state[index] = (byte)FloorState.Planned;
            _kind[index] = KindOf(floor);
            Touch(index);
        }

        /// <summary>Le sol de cette maille change d'état, sans changer de sorte : bâti, ou qu'on efface.</summary>
        public void Set(int index, FloorState state)
        {
            _state[index] = (byte)state;
            Touch(index);
        }

        /// <summary>Cette maille n'a plus de sol.</summary>
        public void Clear(int index)
        {
            _state[index] = (byte)FloorState.None;
            _kind[index] = 0;
            Touch(index);
        }

        /// <summary>La boîte des mailles changées depuis la dernière fois, et l'oublie ; faux si rien n'a changé.</summary>
        public bool TakeChanged(out RectInt box)
        {
            box = _changed ? new RectInt(_changedMin, _changedMax - _changedMin + Vector2Int.one) : default;
            bool changed = _changed;
            _changed = false;
            return changed;
        }

        // La place de cette sorte dans _kinds, prise la première fois qu'elle est tracée.
        private byte KindOf(FloorDefinition floor)
        {
            int kind = _kinds.IndexOf(floor);
            if (kind > 0)
                return (byte)kind;
            if (_kinds.Count > byte.MaxValue)
                throw new InvalidOperationException("Plus de " + byte.MaxValue + " sortes de sol.");
            _kinds.Add(floor);
            return (byte)(_kinds.Count - 1);
        }

        private void Touch(int index)
        {
            Vector2Int cell = _grid.CellAt(index);
            _changedMin = _changed ? Vector2Int.Min(_changedMin, cell) : cell;
            _changedMax = _changed ? Vector2Int.Max(_changedMax, cell) : cell;
            _changed = true;
        }
    }
}
