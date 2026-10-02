using Diciela.Data;
using Diciela.Simulation;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le panneau Construire propose : ce qui se pose encore (la ville sait le bâtir, et une construction unique
    /// n'est pas déjà posée). Le campement d'abord (GDD, section 6) : tant que la Chimère n'a pas de logis, il ne propose
    /// que son logis, si le catalogue en a un ; c'est une consigne du panneau, pas une règle des ordres (décision 95).
    /// </summary>
    internal sealed class BuildBarOffer
    {
        private readonly World _world;
        private readonly bool _catalogHasAHome;

        public BuildBarOffer(Catalog catalog, World world)
        {
            _world = world;
            _catalogHasAHome = HasAHome(catalog);
        }

        /// <summary>Vrai si le panneau propose cette construction, maintenant.</summary>
        public bool Offers(StructureDefinition structure)
        {
            if (!_world.PlacementRules.IsAvailable(structure))
                return false;
            return structure.IsChimeraHome || !_catalogHasAHome || _world.Structures.ChimeraHome != null;
        }

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
