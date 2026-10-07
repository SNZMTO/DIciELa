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
            Chosen = entry.LaysTheZone ? PaintInk.Zone : PaintInk.None;
        }

        /// <summary>Les sols que la ville sait faire, dans l'ordre du catalogue, tels qu'au dernier Refresh.</summary>
        public IReadOnlyList<FloorDefinition> Known => _known;

        /// <summary>Ce que l'outil pose maintenant.</summary>
        public PaintInk Chosen { get; private set; }

        /// <summary>Relit les sols que la ville sait faire ; l'outil Chemins qui ne pose rien prend le premier.</summary>
        internal void Refresh()
        {
            _known.Clear();
            for (int i = 0; i < _floors.Count; i++)
            {
                if (_floors[i] != null && _knowledge.Knows(_floors[i]))
                    _known.Add(_floors[i]);
            }
            if (Chosen.IsNone && _known.Count > 0)
                Chosen = PaintInk.Of(_known[0]);
        }

        /// <summary>
        /// Choisit cette encre : la zone, si l'entrée la pose, ou un sol connu ; vrai si ce que pose l'outil change, faux
        /// s'il ne peut pas la poser ou la posait déjà.
        /// </summary>
        internal bool Choose(PaintInk ink)
        {
            bool allowed = ink.IsZone ? _entry.LaysTheZone : ink.Floor != null && _known.Contains(ink.Floor);
            if (!allowed || ink.Equals(Chosen))
                return false;
            Chosen = ink;
            return true;
        }
    }
}
