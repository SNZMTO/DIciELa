using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Ce qu'un habitant peut rejoindre à pied (ARCHITECTURE, section 7) : une construction, un arbre ou un rocher dont
    /// l'un des abords est sur son îlot ; un sol en chantier dont l'une des mailles y est (on s'y tient pour le bâtir,
    /// puisqu'on y marche). On n'offre que cela. Les îlots que touchent les abords d'un occupant, ou les mailles d'un
    /// sol, se gardent avec la version du maillage d'où ils viennent (un sol, aussi avec sa révision) : un habitant qui
    /// cherche une tâche à chaque pas ne les refait pas, tant que rien ne s'est posé ni retiré. Une seule instance par
    /// monde, pour toutes les tâches ; rien n'est alloué une fois les listes à leur taille.
    /// </summary>
    internal sealed class Reach
    {
        private readonly NavGrid _grid;
        private readonly Islands _islands;
        private readonly Surroundings _surroundings;
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();

        // Par occupant déjà demandé : où commencent, et combien sont, les îlots de ses abords dans _around.
        private readonly Dictionary<int, (int Start, int Count)> _spans = new Dictionary<int, (int, int)>();
        // Par sol en chantier déjà demandé : sa révision, et où sont les îlots de ses mailles dans _around.
        private readonly Dictionary<int, (int Revision, int Start, int Count)> _floorSpans =
            new Dictionary<int, (int, int, int)>();
        private readonly List<int> _around = new List<int>();
        private int _version = -1;

        public Reach(NavGrid grid, Islands islands, Surroundings surroundings)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (ReferenceEquals(islands, null))
                throw new ArgumentNullException(nameof(islands));
            if (ReferenceEquals(surroundings, null))
                throw new ArgumentNullException(nameof(surroundings));

            _grid = grid;
            _islands = islands;
            _surroundings = surroundings;
        }

        /// <summary>Vrai si l'on peut servir ce chantier depuis cet îlot : ce qu'il bâtit se rejoint.</summary>
        public bool IsReachable(ConstructionSite site, int island)
        {
            return site.Structure != null ? IsReachable(site.Structure, island) : IsReachable(site.Floor, island);
        }

        /// <summary>Vrai si l'une des mailles de ce sol en chantier est sur cet îlot.</summary>
        public bool IsReachable(FloorPatch patch, int island)
        {
            if (island == 0)
                return false;
            Refresh();
            if (!_floorSpans.TryGetValue(patch.Id, out (int Revision, int Start, int Count) span) ||
                span.Revision != patch.Revision)
            {
                (int start, int count) = Gather(patch);
                span = (patch.Revision, start, count);
                _floorSpans[patch.Id] = span;
            }
            return Holds(span.Start, span.Count, island);
        }

        /// <summary>Vrai si l'un des abords de cette construction est sur cet îlot.</summary>
        public bool IsReachable(Structure structure, int island)
        {
            return IsReachable(PlacementRules.Footprint(structure.Definition, structure.Placement), structure.Id, island);
        }

        /// <summary>Vrai si l'on peut se tenir sur cette maille depuis cet îlot : elle est sur lui (une place de charrette).</summary>
        public bool IsReachable(Vector2Int cell, int island)
        {
            return island != 0 && _grid.Contains(cell) && _islands.IslandOf(cell) == island;
        }

        /// <summary>Vrai si l'un des abords de ce qu'occupe cet occupant, sous cette empreinte, est sur cet îlot.</summary>
        public bool IsReachable(RotatedRect footprint, int occupant, int island)
        {
            if (island == 0)
                return false;
            Refresh();
            if (!_spans.TryGetValue(occupant, out (int Start, int Count) span))
            {
                span = Gather(footprint, occupant);
                _spans.Add(occupant, span);
            }
            return Holds(span.Start, span.Count, island);
        }

        /// <summary>
        /// L'entrepôt le plus proche de ce point, à vol d'oiseau, joignable depuis cet îlot, qui a cette marchandise à
        /// donner ; null s'il n'y en a pas.
        /// </summary>
        public Structure NearestWithGood(Warehouses warehouses, GoodDefinition good, int island, Vector2 from)
        {
            return Nearest(warehouses, good, 1, false, island, from);
        }

        /// <summary>
        /// L'entrepôt le plus proche de ce point, à vol d'oiseau, joignable depuis cet îlot, qui a la place de tant de
        /// cette marchandise, selon son encombrement ; null s'il n'y en a pas.
        /// </summary>
        public Structure NearestWithRoom(Warehouses warehouses, GoodDefinition good, int amount, int island, Vector2 from)
        {
            return Nearest(warehouses, good, amount, true, island, from);
        }

        // Le seul parcours des entrepôts par la distance : avec tant de cette marchandise à donner, ou la place de tant.
        // Un entrepôt qu'on déplace ne prend ni ne donne rien de neuf (décision 199).
        private Structure Nearest(Warehouses warehouses, GoodDefinition good, int amount, bool room, int island,
            Vector2 from)
        {
            Structure nearest = null;
            float best = float.MaxValue;
            for (int i = 0; i < warehouses.Count; i++)
            {
                Inventory stock = warehouses.StockAt(i);
                if ((room ? stock.RoomFor(good) : stock.AvailableOf(good)) < amount)
                    continue;
                Structure warehouse = warehouses.StructureAt(i);
                if (warehouse.State != StructureState.Built)
                    continue;
                float distance = Vector2.Distance(warehouse.Placement.Center, from);
                if (distance < best && IsReachable(warehouse, island))
                {
                    best = distance;
                    nearest = warehouse;
                }
            }
            return nearest;
        }

        // Ce qu'on a gardé vient d'une version du maillage passée : on l'oublie.
        private void Refresh()
        {
            if (_version == _grid.Version)
                return;
            _spans.Clear();
            _floorSpans.Clear();
            _around.Clear();
            _version = _grid.Version;
        }

        private bool Holds(int start, int count, int island)
        {
            for (int i = start; i < start + count; i++)
            {
                if (_around[i] == island)
                    return true;
            }
            return false;
        }

        // Les îlots, sans doublon, que touchent les abords de cet occupant, rangés au bout de _around.
        private (int Start, int Count) Gather(RotatedRect footprint, int occupant)
        {
            _surroundings.Collect(footprint, occupant, _cells);
            return GatherIslands(_cells);
        }

        // Les îlots, sans doublon, des mailles de ce sol, rangés au bout de _around.
        private (int Start, int Count) Gather(FloorPatch patch)
        {
            patch.CopyCells(_cells);
            return GatherIslands(_cells);
        }

        private (int Start, int Count) GatherIslands(List<Vector2Int> cells)
        {
            int start = _around.Count;
            for (int i = 0; i < cells.Count; i++)
            {
                int island = _islands.IslandOf(cells[i]);
                if (island != 0 && _around.IndexOf(island, start) < 0)
                    _around.Add(island);
            }
            return (start, _around.Count - start);
        }
    }
}
