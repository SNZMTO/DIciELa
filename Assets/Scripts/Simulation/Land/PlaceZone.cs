using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// La Place (GDD, section 11 ; décision 204) : une zone que le joueur trace sur le sol, de la forme qu'il veut, sans
    /// rien coûter et tout de suite, et les places de charrette qu'il y pose (des marques). Ses mailles sont la vérité : ce
    /// qui a été peint, découpé sur ce qui l'arrêtait (PlaceChecks) ; elle les tient au maillage comme un sol qu'on
    /// traverse, où rien ne se bâtit, mais qui peut s'étendre devant une porte. Au départ, une seule, d'un seul tenant, au
    /// plus de la superficie de son niveau. Seule fonction qui la change ; ses événements attendent la fin de sa couche.
    /// </summary>
    public sealed class PlaceZone
    {
        /// <summary>L'occupant de ses mailles au maillage : aucune construction ne porte ce numéro.</summary>
        public const int Occupant = int.MaxValue;

        private readonly NavGrid _grid;
        private readonly PlaceDefinition _definition;
        private readonly CartDefinition _cart;
        private readonly PlaceChecks _checks;
        private readonly HashSet<int> _cells = new HashSet<int>();
        private readonly List<int> _change = new List<int>();
        private readonly List<PlaceMark> _marks = new List<PlaceMark>();
        private readonly List<PlaceMark> _placed = new List<PlaceMark>();
        private readonly List<PlaceMark> _removed = new List<PlaceMark>();
        private readonly List<PlaceResult> _refusals = new List<PlaceResult>();
        private bool _changed;
        private int _nextMarkId = 1;

        /// <summary>La Place de cette fiche, ses places à la taille de cette charrette ; sans elles, ni zone ni place.</summary>
        public PlaceZone(NavGrid grid, PlaceDefinition definition, CartDefinition cart)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _definition = definition;
            _cart = cart;
            _checks = new PlaceChecks(grid);
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
        public IReadOnlyList<PlaceMark> Marks => _marks;

        private float CellArea => _grid.Metrics.CellSize * _grid.Metrics.CellSize;
        private int MaxCells => Mathf.FloorToInt(MaxAreaSquareMeters / CellArea + 1e-3f);

        public bool Contains(Vector2Int cell)
        {
            return _grid.Contains(cell) && _cells.Contains(_grid.IndexOf(cell));
        }

        /// <summary>La place de charrette de ce numéro ; null s'il n'y en a pas.</summary>
        public PlaceMark FindMark(int id)
        {
            for (int i = 0; i < _marks.Count; i++)
            {
                if (_marks[i].Id == id)
                    return _marks[i];
            }
            return null;
        }

        /// <summary>La place que prendrait une place de charrette posée ici, l'avant de la charrette ainsi tourné.</summary>
        public RotatedRect MarkFootprint(Vector2 position, float headingDegrees)
        {
            Vector2 size = _cart == null ? Vector2.zero : new Vector2(_cart.SpotWidthMeters, _cart.SpotLengthMeters);
            return new RotatedRect(position, size, headingDegrees);
        }

        /// <summary>Si l'on peut peindre cette forme : la réponse de l'ordre et de l'aperçu.</summary>
        public PlaceResult CheckPaint(GroundShape shape)
        {
            return _checks.Paint(shape, _cells, MaxCells, _change);
        }

        /// <summary>Si l'on peut gommer cette forme : la réponse de l'ordre et de l'aperçu.</summary>
        public PlaceResult CheckErase(GroundShape shape)
        {
            return _checks.Erase(shape, _cells, _marks, _change);
        }

        /// <summary>Si l'on peut poser une place de charrette à ce milieu, à ce cap : la réponse de l'ordre et de l'aperçu.</summary>
        public PlaceResult CheckMark(Vector2 center, float headingDegrees)
        {
            if (MaxMarks == 0)
                return PlaceResult.TooManyMarks;
            // Celui qui la gare se tient au bout du timon : au bord avant de la marque (CartSpot.StandAt).
            RotatedRect footprint = MarkFootprint(center, headingDegrees);
            Vector2 stand = center + Heading.Direction(headingDegrees) * (footprint.Size.y * 0.5f);
            return _checks.Mark(footprint, stand, _cells, _marks, MaxMarks);
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
            if (!Accept(CheckMark(center, headingDegrees)))
                return null;
            RotatedRect footprint = MarkFootprint(center, headingDegrees);
            float ahead = footprint.Size.y * 0.5f - _cart.PullMeters;
            var mark = new PlaceMark(_nextMarkId++, footprint, center + Heading.Direction(headingDegrees) * ahead);
            _marks.Add(mark);
            _placed.Add(mark);
            return mark;
        }

        /// <summary>Retire la place de charrette de ce numéro ; faux s'il n'y en a pas.</summary>
        internal bool RemoveMark(int id)
        {
            PlaceMark mark = FindMark(id);
            if (!Accept(mark == null ? PlaceResult.UnknownMark : PlaceResult.Ok))
                return false;
            _marks.Remove(mark);
            _removed.Add(mark);
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
                for (int i = 0; i < _placed.Count; i++)
                    MarkPlaced?.Invoke(_placed[i]);
                for (int i = 0; i < _removed.Count; i++)
                    MarkRemoved?.Invoke(_removed[i]);
                for (int i = 0; i < _refusals.Count; i++)
                    PlaceRefused?.Invoke(_refusals[i]);
            }
            finally
            {
                _changed = false;
                _placed.Clear();
                _removed.Clear();
                _refusals.Clear();
            }
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
