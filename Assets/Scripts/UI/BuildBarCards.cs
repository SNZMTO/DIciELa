using System.Collections.Generic;
using System.Globalization;
using Diciela.Controls;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Les cartes de la famille ouverte du panneau Construire (GDD, section 16) : la carte de la Place d'abord, si la
    /// famille la porte (décision 206), puis chaque construction qui se pose encore, avec ce qu'elle offre (ses lits), ce
    /// qu'elle coûte et sa touche. Il retient ce que chaque carte met en main.
    /// </summary>
    internal sealed class BuildBarCards
    {
        private readonly TextTable _texts;
        private readonly PlazaDefinition _plaza;
        private readonly BuildBarOffer _offer;
        private readonly IToolKeys _keys;
        private readonly List<BuildBarCard> _cards = new List<BuildBarCard>();
        private readonly List<StructureDefinition> _structures = new List<StructureDefinition>();

        public BuildBarCards(TextTable texts, PlazaDefinition plaza, BuildBarOffer offer, IToolKeys keys)
        {
            _texts = texts;
            _plaza = plaza;
            _offer = offer;
            _keys = keys;
        }

        public IReadOnlyList<BuildBarCard> Cards => _cards;

        /// <summary>Vrai si la première carte est celle de la Place.</summary>
        public bool HasPlaza { get; private set; }

        /// <summary>Refait les cartes de cette famille ; aucune pour une famille nulle.</summary>
        public void Fill(StructureFamilyDefinition family)
        {
            _cards.Clear();
            _structures.Clear();
            HasPlaza = _offer.OffersPlazaIn(family);
            if (family == null)
                return;
            // La Place ne coûte rien et n'offre pas de lit : son nom et son icône (décision 204).
            if (HasPlaza)
                _cards.Add(new BuildBarCard(_texts.Get(_plaza.NameKey), null, _plaza.Icon, family.Pastel, "", null,
                    _keys.KeyOfCard(0)));
            foreach (StructureDefinition structure in family.Structures)
            {
                if (structure == null || !_offer.Offers(structure))
                    continue;
                _structures.Add(structure);
                _cards.Add(new BuildBarCard(_texts.Get(structure.NameKey), structure.Picture, null, family.Pastel,
                    BedsOf(structure), structure.Cost, _keys.KeyOfCard(_cards.Count)));
            }
        }

        /// <summary>Vrai si la carte de ce rang est celle de la Place.</summary>
        public bool IsPlaza(int index)
        {
            return HasPlaza && index == 0;
        }

        /// <summary>La construction de la carte de ce rang ; null pour la carte de la Place, ou hors des cartes.</summary>
        public StructureDefinition StructureAt(int index)
        {
            int at = HasPlaza ? index - 1 : index;
            return at >= 0 && at < _structures.Count ? _structures[at] : null;
        }

        /// <summary>Le rang de la carte de cette construction ; -1 si elle n'en a pas.</summary>
        public int IndexOf(StructureDefinition structure)
        {
            int at = ReferenceEquals(structure, null) ? -1 : _structures.IndexOf(structure);
            return at < 0 ? -1 : at + (HasPlaza ? 1 : 0);
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
