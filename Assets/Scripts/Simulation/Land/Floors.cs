using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les sols (GDD, section 9 ; décisions 209 et 210) : chemins, routes et sols de la Place, un seul système. Une couche
    /// à côté du maillage, qui ne change pas où l'on marche : chaque maille a sa sorte de sol et son état (FloorCells),
    /// la vérité, comme les mailles de la Place, découpées sur ce qui arrêtait la forme. Un ordre « tracer » fait un sol
    /// en chantier (FloorPatch), en fantôme ; bâti, il sert. La gomme retire un fantôme tout de suite, et met un sol bâti
    /// dans un petit chantier, où il sert jusqu'à la fin. Seule fonction qui les change ; ses événements attendent la fin
    /// de sa couche.
    /// </summary>
    public sealed class Floors
    {
        private readonly NavGrid _grid;
        private readonly FloorCells _cells;
        private readonly FloorChecks _checks;
        // Le sol en chantier de chaque maille en fantôme ou qu'on efface.
        private readonly Dictionary<int, FloorPatch> _patchAt = new Dictionary<int, FloorPatch>();
        private readonly List<int> _change = new List<int>();
        private readonly HashSet<int> _removed = new HashSet<int>();
        private readonly List<FloorPatch> _shrunk = new List<FloorPatch>();
        private readonly List<FloorResult> _refusals = new List<FloorResult>();
        private int _nextId = 1;

        public Floors(NavGrid grid, PlaceZone place, Knowledge knowledge)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _cells = new FloorCells(grid);
            _checks = new FloorChecks(grid, place, knowledge, _cells);
        }

        /// <summary>Levé quand des sols changent, une fois par pas au plus, avec la boîte des mailles changées.</summary>
        public event Action<RectInt> FloorsChanged;

        /// <summary>Levé quand un ordre sur les sols est refusé, avec sa raison.</summary>
        public event Action<FloorResult> FloorRefused;

        public FloorState StateAt(Vector2Int cell)
        {
            return _cells.StateAt(_grid.IndexOf(cell));
        }

        /// <summary>La sorte du sol de cette maille, quel que soit son état ; null s'il n'y en a pas.</summary>
        public FloorDefinition FloorAt(Vector2Int cell)
        {
            return _cells.FloorAt(_grid.IndexOf(cell));
        }

        /// <summary>
        /// Si l'on peut tracer ce sol dans cette forme, à cette entrée : la réponse de l'ordre et de l'aperçu ; les mailles
        /// qu'elle ajouterait vont dans <paramref name="change"/>, si on en donne une.
        /// </summary>
        public FloorResult CheckTrace(GroundShape shape, FloorDefinition floor, FloorScope scope,
            List<Vector2Int> change = null)
        {
            return Report(_checks.Trace(shape, floor, scope, _change), change);
        }

        /// <summary>
        /// Si l'on peut gommer cette forme, à cette entrée : la réponse de l'ordre et de l'aperçu ; les mailles qu'elle
        /// effacerait vont dans <paramref name="change"/>, si on en donne une.
        /// </summary>
        public FloorResult CheckErase(GroundShape shape, FloorScope scope, List<Vector2Int> change = null)
        {
            return Report(_checks.Erase(shape, scope, _change), change);
        }

        /// <summary>Le sol de chaque maille, que lit le pas des charrettes.</summary>
        internal FloorCells Cells => _cells;

        /// <summary>Vrai si cette maille a un sol, bâti ou en fantôme : rien ne s'y bâtit.</summary>
        internal bool HasFloor(int index)
        {
            return _cells.StateAt(index) != FloorState.None;
        }

        /// <summary>Trace ce sol dans cette forme, en fantôme, et rend son chantier ; refusé, null, et le refus est levé.</summary>
        internal FloorPatch Trace(GroundShape shape, FloorDefinition floor, FloorScope scope)
        {
            if (!Accept(CheckTrace(shape, floor, scope)))
                return null;
            var patch = new FloorPatch(_nextId++, floor, _grid);
            for (int i = 0; i < _change.Count; i++)
            {
                _cells.Plan(_change[i], floor);
                _patchAt.Add(_change[i], patch);
                patch.Add(_change[i]);
            }
            return patch;
        }

        /// <summary>
        /// Gomme cette forme, si l'on peut : ses fantômes disparaissent tout de suite (les chantiers qu'elle vide vont
        /// dans <paramref name="emptied"/>) ; ses sols bâtis entrent dans un petit chantier, rendu dans
        /// <paramref name="erasing"/> (null s'il n'y en a pas). Refusée, rien ne change et le refus est levé.
        /// </summary>
        internal bool Erase(GroundShape shape, FloorScope scope, List<FloorPatch> emptied, out FloorPatch erasing)
        {
            emptied.Clear();
            erasing = null;
            if (!Accept(CheckErase(shape, scope)))
                return false;
            _removed.Clear();
            _shrunk.Clear();
            for (int i = 0; i < _change.Count; i++)
            {
                int index = _change[i];
                if (_cells.StateAt(index) == FloorState.Planned)
                {
                    FloorPatch planned = _patchAt[index];
                    _patchAt.Remove(index);
                    _cells.Clear(index);
                    _removed.Add(index);
                    if (!_shrunk.Contains(planned))
                        _shrunk.Add(planned);
                    continue;
                }
                if (erasing == null)
                    erasing = new FloorPatch(_nextId++, null, _grid);
                _cells.Set(index, FloorState.Erasing);
                _patchAt.Add(index, erasing);
                erasing.Add(index);
            }
            for (int i = 0; i < _shrunk.Count; i++)
            {
                _shrunk[i].Forget(_removed);
                if (_shrunk[i].CellCount > 0)
                    continue;
                _shrunk[i].End();
                emptied.Add(_shrunk[i]);
            }
            return true;
        }

        /// <summary>
        /// Le chantier de ce sol est fini : tracé, il est bâti, il sert ; effacé, il n'y a plus de sol. Seule l'économie
        /// l'appelle, quand le dernier travail est fait.
        /// </summary>
        internal void Finish(FloorPatch patch)
        {
            if (ReferenceEquals(patch, null))
                throw new ArgumentNullException(nameof(patch));
            if (patch.IsOver)
                throw new InvalidOperationException("Le sol en chantier n° " + patch.Id + " est déjà fini.");

            IReadOnlyList<int> cells = patch.Cells;
            for (int i = 0; i < cells.Count; i++)
            {
                _patchAt.Remove(cells[i]);
                if (patch.IsErase)
                    _cells.Clear(cells[i]);
                else
                    _cells.Set(cells[i], FloorState.Built);
            }
            patch.End();
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide les files. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                if (_cells.TakeChanged(out RectInt box))
                    FloorsChanged?.Invoke(box);
                for (int i = 0; i < _refusals.Count; i++)
                    FloorRefused?.Invoke(_refusals[i]);
            }
            finally
            {
                _refusals.Clear();
            }
        }

        // La réponse d'une vérification, et les mailles qu'elle changerait, copiées pour l'aperçu.
        private FloorResult Report(FloorResult result, List<Vector2Int> change)
        {
            if (change == null)
                return result;
            change.Clear();
            for (int i = 0; i < _change.Count; i++)
                change.Add(_grid.CellAt(_change[i]));
            return result;
        }

        private bool Accept(FloorResult result)
        {
            if (result == FloorResult.Ok)
                return true;
            _refusals.Add(result);
            return false;
        }
    }
}
