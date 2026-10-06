using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// La Place (GDD, section 11 ; décision 204) : une zone que le joueur trace sur le sol, de la forme qu'il veut, sans
    /// rien coûter et tout de suite, et les places de charrette qu'il y pose (des marques, PlaceMarks). Ses mailles sont
    /// la vérité : ce qui a été peint, découpé sur ce qui l'arrêtait (PlaceChecks) ; elle les tient au maillage comme un
    /// sol qu'on traverse, où rien ne se bâtit, mais qui peut s'étendre devant une porte. Au départ, une seule, d'un seul
    /// tenant, au plus de la superficie de son niveau. Seule fonction qui la change ; ses événements attendent la fin de
    /// sa couche.
    /// </summary>
    public sealed class PlaceZone
    {
        /// <summary>L'occupant de ses mailles au maillage : aucune construction ne porte ce numéro.</summary>
        public const int Occupant = int.MaxValue;

        private readonly NavGrid _grid;
        private readonly PlaceDefinition _definition;
        private readonly CartDefinition _cart;
        private readonly PlaceChecks _checks;
        private readonly PlaceMarks _marks;
        private readonly HashSet<int> _cells = new HashSet<int>();
        private readonly List<int> _change = new List<int>();
        private readonly List<PlaceResult> _refusals = new List<PlaceResult>();
        private bool _changed;

        /// <summary>La Place de cette fiche, ses places à la taille de cette charrette ; sans elles, ni zone ni place.</summary>
        public PlaceZone(NavGrid grid, PlaceDefinition definition, CartDefinition cart)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _definition = definition;
            _cart = cart;
            _checks = new PlaceChecks(grid);
            _marks = new PlaceMarks(cart);
        }

        /// <summary>Levé quand ses mailles changent : peintes ou gommées ; une fois par pas au plus.</summary>
        public event Action PlaceChanged;

        /// <summary>Levé quand une place de charrette est posée.</summary>
        public event Action<PlaceMark> MarkPlaced;

        /// <summary>Levé quand une place de charrette est retirée.</summary>
        public event Action<PlaceMark> MarkRemoved;

        /// <summary>Levé quand un ordre sur la Place est refusé, avec sa raison.</summary>
        public event Action<PlaceResult> PlaceRefused;

        /// <summary>Son niveau : 0 au départ ; les niveaux viendront, chacun par un petit chantier.</summary>
        public int Level => 0;

        public int CellCount => _cells.Count;
        public float AreaSquareMeters => _cells.Count * CellArea;
        public float MaxAreaSquareMeters => _definition == null ? 0f : _definition.MaxAreaAt(Level);
        public int MaxMarks => _definition == null || _cart == null ? 0 : _definition.CartSpotsAt(Level);

        /// <summary>Ses places de charrette, dans l'ordre où elles ont été posées : celui où elles appellent.</summary>
        public IReadOnlyList<PlaceMark> Marks => _marks.All;

        private float CellArea => _grid.Metrics.CellSize * _grid.Metrics.CellSize;
        private int MaxCells => Mathf.FloorToInt(MaxAreaSquareMeters / CellArea + 1e-3f);

        public bool Contains(Vector2Int cell)
        {
            return _grid.Contains(cell) && _cells.Contains(_grid.IndexOf(cell));
        }

        /// <summary>Met dans cette liste ses mailles, dans aucun ordre promis ; la liste est vidée d'abord.</summary>
        public void CopyCells(List<Vector2Int> cells)
        {
            cells.Clear();
            foreach (int index in _cells)
                cells.Add(_grid.CellAt(index));
        }

        /// <summary>La place que prendrait une place de charrette posée ici, l'avant de la charrette ainsi tourné.</summary>
        public RotatedRect MarkFootprint(Vector2 position, float headingDegrees)
        {
            return _marks.Footprint(position, headingDegrees);
        }

        /// <summary>
        /// Si l'on peut peindre cette forme : la réponse de l'ordre et de l'aperçu ; les mailles qu'elle ajouterait vont
        /// dans <paramref name="change"/>, si on en donne une.
        /// </summary>
        public PlaceResult CheckPaint(GroundShape shape, List<Vector2Int> change = null)
        {
            return Report(_checks.Paint(shape, _cells, MaxCells, _change), change);
        }

        /// <summary>
        /// Si l'on peut gommer cette forme : la réponse de l'ordre et de l'aperçu ; les mailles qu'elle retirerait vont
        /// dans <paramref name="change"/>, si on en donne une.
        /// </summary>
        public PlaceResult CheckErase(GroundShape shape, List<Vector2Int> change = null)
        {
            return Report(_checks.Erase(shape, _cells, _marks.All, _change), change);
        }

        /// <summary>Si l'on peut poser une place de charrette à ce milieu, à ce cap : la réponse de l'ordre et de l'aperçu.</summary>
        public PlaceResult CheckMark(Vector2 center, float headingDegrees)
        {
            if (MaxMarks == 0)
                return PlaceResult.TooManyMarks;
            return _checks.Mark(MarkFootprint(center, headingDegrees), _marks.StandOf(center, headingDegrees), _cells,
                _marks.All, MaxMarks);
        }

        /// <summary>Peint cette forme, si l'on peut ; sinon, rien ne change et le refus est levé.</summary>
        internal bool Paint(GroundShape shape)
        {
            if (!Accept(CheckPaint(shape)))
                return false;
            for (int i = 0; i < _change.Count; i++)
                _cells.Add(_change[i]);
            _grid.SetCells(_change, Occupant, true);
            _changed = true;
            return true;
        }

        /// <summary>Gomme cette forme, si l'on peut ; sinon, rien ne change et le refus est levé.</summary>
        internal bool Erase(GroundShape shape)
        {
            if (!Accept(CheckErase(shape)))
                return false;
            for (int i = 0; i < _change.Count; i++)
                _cells.Remove(_change[i]);
            _grid.SetCells(_change, 0, false);
            _changed = true;
            return true;
        }

        /// <summary>Pose une place de charrette à ce milieu, à ce cap, si l'on peut ; null sinon.</summary>
        internal PlaceMark AddMark(Vector2 center, float headingDegrees)
        {
            return Accept(CheckMark(center, headingDegrees)) ? _marks.Add(center, headingDegrees) : null;
        }

        /// <summary>Retire la place de charrette de ce numéro ; faux s'il n'y en a pas.</summary>
        internal bool RemoveMark(int id)
        {
            PlaceMark mark = _marks.Find(id);
            if (!Accept(mark == null ? PlaceResult.UnknownMark : PlaceResult.Ok))
                return false;
            _marks.Remove(mark);
            return true;
        }

        /// <summary>Un ordre sur la Place est refusé ailleurs (sa charrette arrive) : le refus part avec les autres.</summary>
        internal void Refuse(PlaceResult result)
        {
            _refusals.Add(result);
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide les files. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                if (_changed)
                    PlaceChanged?.Invoke();
                _marks.Deliver(MarkPlaced, MarkRemoved);
                for (int i = 0; i < _refusals.Count; i++)
                    PlaceRefused?.Invoke(_refusals[i]);
            }
            finally
            {
                _changed = false;
                _refusals.Clear();
            }
        }

        // La réponse d'une vérification, et les mailles qu'elle changerait, copiées pour l'aperçu.
        private PlaceResult Report(PlaceResult result, List<Vector2Int> change)
        {
            if (change == null)
                return result;
            change.Clear();
            for (int i = 0; i < _change.Count; i++)
                change.Add(_grid.CellAt(_change[i]));
            return result;
        }

        private bool Accept(PlaceResult result)
        {
            if (result == PlaceResult.Ok)
                return true;
            _refusals.Add(result);
            return false;
        }
    }
}
