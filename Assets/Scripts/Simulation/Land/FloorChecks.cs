using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les règles des sols (décisions 209 et 210), une seule fonction par ordre, que lisent l'ordre et l'aperçu. Tracer
    /// ajoute les mailles où l'on marche (le sol libre, la Place) que couvre la forme et qui n'ont pas encore de sol : ce
    /// qui s'y tient (une construction, son fantôme, un arbre, un rocher) et tout sol déjà posé, bâti ou en fantôme, y
    /// font un trou ; la ville doit savoir tracer ce sol. Gommer retire les sols que couvre la forme, en fantôme ou bâtis ;
    /// ceux qu'on efface déjà n'y comptent pas. À l'outil de la Place, l'un et l'autre s'arrêtent au bord de la zone. Rien
    /// n'est alloué une fois les listes à leur taille.
    /// </summary>
    internal sealed class FloorChecks
    {
        private readonly NavGrid _grid;
        private readonly PlazaZone _plaza;
        private readonly Knowledge _knowledge;
        // Le sol de chaque maille, tenu par les sols : ces règles le lisent sans l'écrire.
        private readonly FloorCells _cells;
        private readonly List<Vector2Int> _covered = new List<Vector2Int>();

        public FloorChecks(NavGrid grid, PlazaZone plaza, Knowledge knowledge, FloorCells cells)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _plaza = plaza ?? throw new ArgumentNullException(nameof(plaza));
            _knowledge = knowledge ?? throw new ArgumentNullException(nameof(knowledge));
            _cells = cells ?? throw new ArgumentNullException(nameof(cells));
        }

        /// <summary>Ce que tracer ce sol dans cette forme ajouterait, mis dans cette liste ; et si on le peut.</summary>
        public FloorResult Trace(GroundShape shape, FloorDefinition floor, FloorScope scope, List<int> added)
        {
            if (ReferenceEquals(floor, null))
                throw new ArgumentNullException(nameof(floor));

            added.Clear();
            if (!_knowledge.Knows(floor))
                return FloorResult.Unknown;
            Cover(shape);
            for (int i = 0; i < _covered.Count; i++)
            {
                int index = _grid.IndexOf(_covered[i]);
                if (_cells.StateAt(index) == FloorState.None && _grid.IsWalkable(index) && IsInScope(_covered[i], scope))
                    added.Add(index);
            }
            return added.Count == 0 ? FloorResult.Nothing : FloorResult.Ok;
        }

        /// <summary>Ce que gommer cette forme effacerait, mis dans cette liste ; et si on le peut.</summary>
        public FloorResult Erase(GroundShape shape, FloorScope scope, List<int> removed)
        {
            removed.Clear();
            Cover(shape);
            for (int i = 0; i < _covered.Count; i++)
            {
                int index = _grid.IndexOf(_covered[i]);
                FloorState state = _cells.StateAt(index);
                if ((state == FloorState.Planned || state == FloorState.Built) && IsInScope(_covered[i], scope))
                    removed.Add(index);
            }
            return removed.Count == 0 ? FloorResult.Nothing : FloorResult.Ok;
        }

        private bool IsInScope(Vector2Int cell, FloorScope scope)
        {
            return scope == FloorScope.Anywhere || _plaza.Contains(cell);
        }

        private void Cover(GroundShape shape)
        {
            if (ReferenceEquals(shape, null))
                throw new ArgumentNullException(nameof(shape));
            _covered.Clear();
            shape.CoverCells(_grid, _covered);
        }
    }
}
