using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.Simulation.Core
{
    /// <summary>
    /// Le savoir de la ville (GDD, sections 6 et 13 ; décision 185) : les constructions qu'elle sait bâtir. Au départ,
    /// exactement ce que la Chimère sait en arrivant, que dit le déroulé du départ ; les idées des habitants s'y
    /// ajouteront. Sans déroulé du départ (un banc d'essai), rien ne se passe de soi-même : la ville sait tout bâtir. Au
    /// socle : les règles de pose le lisent comme le panneau Construire, et plus tard les recettes et les décos.
    /// </summary>
    public sealed class Knowledge
    {
        private readonly OpeningDefinition _opening;

        public Knowledge(OpeningDefinition opening)
        {
            _opening = opening;
        }

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
            return false;
        }
    }
}
