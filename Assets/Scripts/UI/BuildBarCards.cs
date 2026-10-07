using System.Collections.Generic;
using System.Globalization;
using Diciela.Controls;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Les cartes de la famille ouverte du panneau Construire (GDD, section 16) : chaque construction qui se pose
    /// encore, avec ce qu'elle offre (ses lits), ce qu'elle coûte et sa touche. Il retient ce que chaque carte met en
    /// main.
    /// </summary>
    internal sealed class BuildBarCards
    {
        private readonly TextTable _texts;
        private readonly BuildBarOffer _offer;
        private readonly IToolKeys _keys;
        private readonly List<BuildBarCard> _cards = new List<BuildBarCard>();
        private readonly List<StructureDefinition> _structures = new List<StructureDefinition>();

        public BuildBarCards(TextTable texts, BuildBarOffer offer, IToolKeys keys)
        {
            _texts = texts;
            _offer = offer;
            _keys = keys;
        }

        public IReadOnlyList<BuildBarCard> Cards => _cards;

        /// <summary>Refait les cartes de cette famille ; aucune pour une famille nulle.</summary>
        public void Fill(StructureFamilyDefinition family)
        {
            _cards.Clear();
            _structures.Clear();
            if (family == null)
                return;
            foreach (StructureDefinition structure in family.Structures)
            {
                if (structure == null || !_offer.Offers(structure))
                    continue;
                _structures.Add(structure);
                _cards.Add(new BuildBarCard(_texts.Get(structure.NameKey), structure.Picture, family.Pastel,
                    BedsOf(structure), structure.Cost, _keys.KeyOfCard(_cards.Count)));
            }
        }

        /// <summary>La construction de la carte de ce rang ; null hors des cartes.</summary>
        public StructureDefinition StructureAt(int index)
        {
            return index >= 0 && index < _structures.Count ? _structures[index] : null;
        }

        /// <summary>Le rang de la carte de cette construction ; -1 si elle n'en a pas.</summary>
        public int IndexOf(StructureDefinition structure)
        {
            return ReferenceEquals(structure, null) ? -1 : _structures.IndexOf(structure);
        }

        /// <summary>Les lits qu'offre une construction, écrits pour sa carte (« 2 lits ») ; vide si elle n'en a pas.</summary>
        private string BedsOf(StructureDefinition structure)
        {
            if (structure.Beds <= 0)
                return "";
            return _texts.Get(TextKeys.CardBeds)
                .Replace(TextKeys.BedsParameter, structure.Beds.ToString(CultureInfo.InvariantCulture));
        }
    }
}
