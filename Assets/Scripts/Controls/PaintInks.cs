using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;

namespace Diciela.Controls
{
    /// <summary>
    /// Ce que pose un outil de sol (décision 210) : la zone de la Place, si son entrée la pose, ou l'un des sols que la
    /// ville sait faire, dans l'ordre du catalogue. Au départ, la zone pour l'outil de la Place, le premier sol connu pour
    /// l'outil Chemins ; un sol choisi le reste. Sans sol connu, l'outil Chemins ne pose rien (sa gomme efface encore).
    /// La liste se relit quand on le demande (à la prise en main) : une idée venue entre-temps y paraît. Seul son outil
    /// la change, qui le dit.
    /// </summary>
    public sealed class PaintInks
    {
        private readonly PaintEntry _entry;
        private readonly Knowledge _knowledge;
        private readonly IReadOnlyList<FloorDefinition> _floors;
        private readonly List<FloorDefinition> _known = new List<FloorDefinition>();

        internal PaintInks(PaintEntry entry, Knowledge knowledge, IReadOnlyList<FloorDefinition> floors)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
            _knowledge = knowledge ?? throw new ArgumentNullException(nameof(knowledge));
            _floors = floors ?? Array.Empty<FloorDefinition>();
        }

        /// <summary>Les sols que la ville sait faire, dans l'ordre du catalogue, tels qu'au dernier Refresh.</summary>
        public IReadOnlyList<FloorDefinition> Known => _known;

        /// <summary>Le sol choisi ; null : la zone (l'outil de la Place), ou rien (l'outil Chemins sans sol connu).</summary>
        public FloorDefinition Floor { get; private set; }

        /// <summary>Vrai si l'outil pose la zone de la Place.</summary>
        public bool LaysTheZone => Floor == null && _entry.LaysTheZone;

        /// <summary>Relit les sols que la ville sait faire ; l'outil Chemins sans sol choisi prend le premier.</summary>
        internal void Refresh()
        {
            _known.Clear();
            for (int i = 0; i < _floors.Count; i++)
            {
                if (_floors[i] != null && _knowledge.Knows(_floors[i]))
                    _known.Add(_floors[i]);
            }
            if (Floor == null && !_entry.LaysTheZone && _known.Count > 0)
                Floor = _known[0];
        }

        /// <summary>
        /// Choisit ce sol (null : la zone, si l'entrée la pose) ; vrai si ce que pose l'outil change, faux s'il ne peut pas
        /// le poser ou le posait déjà.
        /// </summary>
        internal bool Choose(FloorDefinition floor)
        {
            if (floor == null ? !_entry.LaysTheZone : !_known.Contains(floor))
                return false;
            if (ReferenceEquals(floor, Floor))
                return false;
            Floor = floor;
            return true;
        }
    }
}
