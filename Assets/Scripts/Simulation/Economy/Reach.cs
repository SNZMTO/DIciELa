using System;
using System.Collections.Generic;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Ce qu'un habitant peut rejoindre à pied (ARCHITECTURE, section 7) : une construction, un arbre ou un rocher dont
    /// l'un des abords est sur son îlot. On n'offre que cela. Rien n'est alloué : la liste des abords se garde d'un appel
    /// à l'autre.
    /// </summary>
    internal sealed class Reach
    {
        private readonly Islands _islands;
        private readonly Surroundings _surroundings;
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();

        public Reach(Islands islands, Surroundings surroundings)
        {
            if (ReferenceEquals(islands, null))
                throw new ArgumentNullException(nameof(islands));
            if (ReferenceEquals(surroundings, null))
                throw new ArgumentNullException(nameof(surroundings));

            _islands = islands;
            _surroundings = surroundings;
        }

        /// <summary>Vrai si l'un des abords de cette construction est sur cet îlot.</summary>
        public bool IsReachable(Structure structure, int island)
        {
            return IsReachable(PlacementRules.Footprint(structure.Definition, structure.Placement), structure.Id, island);
        }

        /// <summary>Vrai si l'un des abords de ce qu'occupe cet occupant, sous cette empreinte, est sur cet îlot.</summary>
        public bool IsReachable(RotatedRect footprint, int occupant, int island)
        {
            if (island == 0)
                return false;
            _surroundings.Collect(footprint, occupant, _cells);
            for (int i = 0; i < _cells.Count; i++)
            {
                if (_islands.IslandOf(_cells[i]) == island)
                    return true;
            }
            return false;
        }
    }
}
