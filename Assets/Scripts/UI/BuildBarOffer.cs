using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le panneau Construire propose : ce qui se pose encore (la ville sait le bâtir, et une construction unique
    /// n'est pas déjà posée), et la carte de la Place, en tête de la famille que sa fiche nomme (GDD, section 16 ;
    /// décision 206). Le campement d'abord (GDD, section 6) : tant que la Chimère n'a pas de logis, il ne propose que son
    /// logis, si le catalogue en a un ; c'est une consigne du panneau, pas une règle des ordres (décision 95).
    /// </summary>
    internal sealed class BuildBarOffer
    {
        private readonly World _world;
        private readonly PlazaDefinition _plaza;
        private readonly bool _catalogHasAHome;

        public BuildBarOffer(Catalog catalog, World world)
        {
            _world = world;
            _plaza = catalog.Plaza;
            _catalogHasAHome = HasAHome(catalog);
        }

        /// <summary>Vrai si le panneau propose cette construction, maintenant.</summary>
        public bool Offers(StructureDefinition structure)
        {
            if (!_world.PlacementRules.IsAvailable(structure))
                return false;
            return structure.IsChimeraHome || HomeIsPlaced;
        }

        /// <summary>
        /// Vrai si le panneau propose la carte de la Place dans cette famille, maintenant : dès le campement posé, en tête
        /// de la famille que nomme sa fiche.
        /// </summary>
        public bool OffersPlazaIn(StructureFamilyDefinition family)
        {
            return _plaza != null && family != null && ReferenceEquals(family, _plaza.Family) && HomeIsPlaced;
        }

        /// <summary>Vrai si le panneau propose une carte de cette famille ; sinon, son onglet ne paraît pas.</summary>
        public bool Offers(StructureFamilyDefinition family)
        {
            if (OffersPlazaIn(family))
                return true;
            IReadOnlyList<StructureDefinition> structures = family.Structures;
            for (int i = 0; i < structures.Count; i++)
            {
                if (structures[i] != null && Offers(structures[i]))
                    return true;
            }
            return false;
        }

        private bool HomeIsPlaced => !_catalogHasAHome || _world.Structures.ChimeraHome != null;

        private static bool HasAHome(Catalog catalog)
        {
            foreach (StructureFamilyDefinition family in catalog.Families)
            {
                foreach (StructureDefinition structure in family.Structures)
                {
                    if (structure != null && structure.IsChimeraHome)
                        return true;
                }
            }
            return false;
        }
    }
}
