using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que Construire propose (GDD, section 16) : ce qui se pose encore (la ville sait le bâtir, et une construction
    /// unique n'est pas déjà posée). Le campement d'abord (GDD, section 6) : tant que la Chimère n'a pas de logis,
    /// Construire ne propose que son logis, si le catalogue en a un, et la roue ne propose pas la Place (décision
    /// 221) ; c'est une consigne de l'interface, pas une règle des ordres (décision 95). Il se lit au moment où l'on
    /// demande : il ne retient rien.
    /// </summary>
    internal sealed class BuildOffer
    {
        private readonly Catalog _catalog;
        private readonly World _world;
        private readonly bool _catalogHasAHome;

        public BuildOffer(Catalog catalog, World world)
        {
            _catalog = catalog;
            _world = world;
            _catalogHasAHome = HasAHome(catalog);
        }

        /// <summary>Vrai si Construire propose cette construction, maintenant.</summary>
        public bool Offers(StructureDefinition structure)
        {
            if (!_world.PlacementRules.IsAvailable(structure))
                return false;
            return structure.IsChimeraHome || HomeIsPlaced;
        }

        /// <summary>Vrai si la ville propose la Place, maintenant : dès le campement posé.</summary>
        public bool OffersPlaza => _catalog.Plaza != null && HomeIsPlaced;

        /// <summary>
        /// Range dans cette liste, vidée d'abord, ce que cette famille propose maintenant, dans son ordre.
        /// </summary>
        public void Fill(StructureFamilyDefinition family, List<StructureDefinition> offered)
        {
            offered.Clear();
            foreach (StructureDefinition structure in family.Structures)
            {
                if (structure != null && Offers(structure))
                    offered.Add(structure);
            }
        }

        /// <summary>Vrai si cette famille propose quelque chose ; sinon, elle ne paraît pas.</summary>
        public bool Offers(StructureFamilyDefinition family)
        {
            foreach (StructureDefinition structure in family.Structures)
            {
                if (structure != null && Offers(structure))
                    return true;
            }
            return false;
        }

        /// <summary>La première construction proposée, dans l'ordre du catalogue ; null s'il n'y en a aucune.</summary>
        public StructureDefinition First()
        {
            foreach (StructureFamilyDefinition family in _catalog.Families)
            {
                if (family == null)
                    continue;
                foreach (StructureDefinition structure in family.Structures)
                {
                    if (structure != null && Offers(structure))
                        return structure;
                }
            }
            return null;
        }

        private bool HomeIsPlaced => !_catalogHasAHome || _world.Structures.ChimeraHome != null;

        private static bool HasAHome(Catalog catalog)
        {
            foreach (StructureFamilyDefinition family in catalog.Families)
            {
                if (family == null)
                    continue;
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
