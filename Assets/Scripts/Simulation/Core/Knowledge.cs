using System;
using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.Simulation.Core
{
    /// <summary>
    /// Le savoir de la ville (GDD, sections 6 et 13 ; décisions 185 et 190) : les constructions qu'elle sait bâtir. Au
    /// départ, exactement ce que la Chimère sait en arrivant, que dit le déroulé du départ ; puis les idées, qui servent
    /// aussitôt : celle qu'apporte chaque premier habitant, et plus tard celles des déclencheurs. Sans déroulé du départ
    /// (un banc d'essai), rien ne se passe de soi-même : la ville sait tout bâtir. Au socle : les règles de pose le lisent
    /// comme le panneau Construire, et plus tard les recettes et les décos. Ce qu'elle apprend pendant un pas s'annonce
    /// à la fin de la couche (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class Knowledge
    {
        private readonly OpeningDefinition _opening;
        private readonly List<StructureDefinition> _learned = new List<StructureDefinition>();
        private readonly List<StructureDefinition> _happened = new List<StructureDefinition>();

        public Knowledge(OpeningDefinition opening)
        {
            _opening = opening;
        }

        /// <summary>Levé quand la ville apprend à bâtir une construction : une idée.</summary>
        public event Action<StructureDefinition> Learned;

        /// <summary>Vrai si la ville sait bâtir cette construction.</summary>
        public bool Knows(StructureDefinition structure)
        {
            if (ReferenceEquals(_opening, null))
                return true;
            IReadOnlyList<StructureDefinition> known = _opening.KnownStructures;
            for (int i = 0; i < known.Count; i++)
            {
                if (ReferenceEquals(known[i], structure))
                    return true;
            }
            return _learned.Contains(structure);
        }

        /// <summary>
        /// La ville sait désormais bâtir cette construction (une idée) ; si elle la savait déjà, rien ne change. Seul le
        /// monde l'appelle (le déroulé du départ).
        /// </summary>
        internal void Learn(StructureDefinition structure)
        {
            if (ReferenceEquals(structure, null))
                throw new ArgumentNullException(nameof(structure));
            if (Knows(structure))
                return;
            _learned.Add(structure);
            _happened.Add(structure);
        }

        /// <summary>Annonce ce que la ville a appris pendant le pas, puis vide la file. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _happened.Count; i++)
                    Learned?.Invoke(_happened[i]);
            }
            finally
            {
                _happened.Clear();
            }
        }
    }
}
