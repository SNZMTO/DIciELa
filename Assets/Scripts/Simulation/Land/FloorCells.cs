using System;
using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Le sol de chaque maille (décision 209) : son état et sa sorte, la vérité des sols, à côté du maillage, sans rien
    /// changer à où l'on marche. Deux octets par maille ; une sorte prend sa place la première fois qu'elle est tracée.
    /// Il compte, par sorte et par bloc de 1 m (FloorBlocks), les mailles où elle sert (bâtie, ou qu'on efface), pour le
    /// pas des charrettes et son estimation, et garde la boîte des mailles changées depuis la dernière fois qu'on l'a
    /// prise, pour la vue (SAVOIR, 2.5). Seuls les sols l'écrivent.
    /// </summary>
    internal sealed class FloorCells
    {
        private readonly NavGrid _grid;
        private readonly byte[] _state;
        // Par maille, la place de sa sorte dans _kinds ; 0 : pas de sol.
        private readonly byte[] _kind;
        private readonly List<FloorDefinition> _kinds = new List<FloorDefinition> { null };
        // Par sorte, le nombre de mailles où elle sert.
        private readonly int[] _serving = new int[byte.MaxValue + 1];
        private readonly FloorBlocks _blocks;
        private Vector2Int _changedMin;
        private Vector2Int _changedMax;
        private bool _changed;

        public FloorCells(NavGrid grid)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _state = new byte[grid.Width * grid.Depth];
            _kind = new byte[grid.Width * grid.Depth];
            _blocks = new FloorBlocks(grid);
        }

        /// <summary>Où servent les sols, à gros grain : par blocs de 1 m.</summary>
        public FloorBlocks Blocks => _blocks;

        public FloorState StateAt(int index)
        {
            return (FloorState)_state[index];
        }

        /// <summary>La sorte du sol de cette maille, quel que soit son état ; null s'il n'y en a pas.</summary>
        public FloorDefinition FloorAt(int index)
        {
            return _kinds[_kind[index]];
        }

        /// <summary>Le nombre de sortes déjà tracées, l'herbe comprise (la sorte 0).</summary>
        public int KindCount => _kinds.Count;

        public FloorDefinition Kind(int kind)
        {
            return _kinds[kind];
        }

        /// <summary>La place de la sorte du sol de cette maille s'il sert (bâti, ou qu'on efface) ; sinon 0, l'herbe.</summary>
        public int ServingKindAt(int index)
        {
            return IsServing(_state[index]) ? _kind[index] : 0;
        }

        /// <summary>Vrai si cette sorte sert sur au moins une maille.</summary>
        public bool Serves(int kind)
        {
            return _serving[kind] > 0;
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
            Count(index, -1);
            _state[index] = (byte)state;
            Count(index, 1);
            Touch(index);
        }

        /// <summary>Cette maille n'a plus de sol.</summary>
        public void Clear(int index)
        {
            Count(index, -1);
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

        // Compte ou décompte cette maille chez sa sorte, si elle y sert.
        private void Count(int index, int sign)
        {
            if (!IsServing(_state[index]))
                return;
            _serving[_kind[index]] += sign;
            _blocks.Count(index, sign);
        }

        private static bool IsServing(byte state)
        {
            return state == (byte)FloorState.Built || state == (byte)FloorState.Erasing;
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
