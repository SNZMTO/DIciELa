using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Les onglets du panneau Construire (GDD, section 16 ; décision 190) : les familles du catalogue qui proposent
    /// quelque chose maintenant, dans leur ordre, et celle qui est ouverte. Une famille qui ne propose rien ne paraît pas
    /// (les métiers, tant que personne n'en a apporté l'idée) ; si la famille ouverte se vide, la première qui reste
    /// s'ouvre.
    /// </summary>
    internal sealed class BuildBarTabs
    {
        private readonly Catalog _catalog;
        private readonly BuildBarOffer _offer;
        private readonly List<StructureFamilyDefinition> _shown = new List<StructureFamilyDefinition>();

        public BuildBarTabs(Catalog catalog, BuildBarOffer offer)
        {
            _catalog = catalog;
            _offer = offer;
        }

        /// <summary>Les familles dont l'onglet paraît, dans l'ordre du catalogue.</summary>
        public IReadOnlyList<StructureFamilyDefinition> Shown => _shown;

        /// <summary>La famille ouverte ; null quand aucune ne paraît.</summary>
        public StructureFamilyDefinition Open { get; private set; }

        /// <summary>Le rang de l'onglet ouvert parmi ceux qui paraissent ; -1 s'il n'y en a pas.</summary>
        public int OpenIndex => Open == null ? -1 : _shown.IndexOf(Open);

        /// <summary>Recompte les onglets ; la famille ouverte le reste tant qu'elle paraît.</summary>
        public void Refresh()
        {
            _shown.Clear();
            foreach (StructureFamilyDefinition family in _catalog.Families)
            {
                if (family != null && _offer.Offers(family))
                    _shown.Add(family);
            }
            if (Open == null || !_shown.Contains(Open))
                Open = _shown.Count > 0 ? _shown[0] : null;
        }

        /// <summary>Ouvre l'onglet de ce rang parmi ceux qui paraissent ; faux s'il n'y en a pas.</summary>
        public bool Choose(int index)
        {
            if (index < 0 || index >= _shown.Count)
                return false;
            Open = _shown[index];
            return true;
        }
    }
}
