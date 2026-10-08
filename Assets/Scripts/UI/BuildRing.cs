using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// La roue de Construire (GDD, section 16 ; décision 226), le seul endroit qui sait quelle case fait quoi : ses
    /// familles autour du plateau, chacune de son pastel avec son icône, à la place que lui donne son rang dans le
    /// catalogue (en haut, à droite, en bas, à gauche, puis les diagonales) ; une famille qui ne propose rien ne paraît
    /// pas, mais garde sa place. Poussée plus loin, une famille déplie ses constructions en éventail, chacune sa
    /// photo ; la bulle dit son nom, ses lits et son coût. La case Outils, en bas à gauche, ramène aux outils. Prendre
    /// une famille garde sa construction déjà en main, sinon met en main sa première ; au chiffre, déjà en main, elle
    /// passe à la suivante. Prendre Construire reprend la dernière construction prise, si elle se pose encore, sinon la
    /// première proposée. Ce qui se propose, BuildOffer le dit, au moment où l'on demande.
    /// </summary>
    internal sealed class BuildRing : IToolRing
    {
        // La place de chaque famille, au rang de chacune dans le catalogue ; la case Outils garde la sienne.
        private static readonly int[] FamilyPlaces = { 0, 2, 4, 6, 1, 3, 7 };
        private const int Back = 5;
        private const string BackIcon = "roue-retour";

        private readonly Catalog _catalog;
        private readonly BuildOffer _offer;
        private readonly PlacementTool _placement;
        private readonly TextTable _texts;
        private readonly IToolKeys _keys;
        private readonly string _pastel;
        private readonly List<StructureDefinition> _offered = new List<StructureDefinition>();

        public BuildRing(Catalog catalog, BuildOffer offer, PlacementTool placement, TextTable texts, IToolKeys keys,
            string pastel)
        {
            _catalog = catalog;
            _offer = offer;
            _placement = placement;
            _texts = texts;
            _keys = keys;
            _pastel = pastel;
        }

        /// <summary>
        /// Met en main la dernière construction prise, si elle se pose encore, sinon la première proposée.
        /// </summary>
        public void Take()
        {
            StructureDefinition last = _placement.Last;
            StructureDefinition structure = last != null && _offer.Offers(last) ? last : _offer.First();
            if (structure != null)
                _placement.Begin(structure);
        }

        public List<WheelSlot> Slots()
        {
            var slots = new List<WheelSlot>();
            IReadOnlyList<StructureFamilyDefinition> families = _catalog.Families;
            for (int rank = 0; rank < families.Count && rank < FamilyPlaces.Length; rank++)
            {
                StructureFamilyDefinition family = families[rank];
                if (family == null || !_offer.Offers(family))
                    continue;
                int place = FamilyPlaces[rank];
                slots.Add(new WheelSlot(place, HudFace.Of(family.Icon), Pastels.ClassOf(family.Pastel), KeyOf(place),
                    _texts.Get(family.NameKey), _texts.Get(TextKeys.WheelFamilyDetail), Holds(family),
                    Options(family)));
            }
            slots.Add(new WheelSlot(Back, HudFace.Styled(BackIcon), _pastel, KeyOf(Back),
                _texts.Get(TextKeys.WheelBack), _texts.Get(TextKeys.WheelBackDetail), false));
            return slots;
        }

        public bool Pick(int place, int option)
        {
            if (place == Back)
                return false;
            StructureFamilyDefinition family = FamilyAt(place);
            if (family == null)
                return true;
            _offer.Fill(family, _offered);
            if (option >= 0 && option < _offered.Count)
                _placement.Begin(_offered[option]);
            else if (!Holds(family) && _offered.Count > 0)
                _placement.Begin(_offered[0]);
            return true;
        }

        public bool Key(int place)
        {
            if (place == Back)
                return false;
            StructureFamilyDefinition family = FamilyAt(place);
            if (family == null)
                return true;
            _offer.Fill(family, _offered);
            if (_offered.Count == 0)
                return true;
            int held = _placement.IsActive ? _offered.IndexOf(_placement.Current) : -1;
            _placement.Begin(_offered[(held + 1) % _offered.Count]);
            return true;
        }

        public bool Scroll(int place, int option, int notches)
        {
            return false;
        }

        // Les constructions qu'une famille propose, en jetons : leur photo (l'icône de la famille, faute de photo),
        // leur nom, leurs lits et leur coût, une ligne chacun.
        private List<WheelOption> Options(StructureFamilyDefinition family)
        {
            _offer.Fill(family, _offered);
            var options = new List<WheelOption>();
            foreach (StructureDefinition structure in _offered)
            {
                HudFace face = structure.Picture != null ? HudFace.Photo(structure.Picture) : HudFace.Of(family.Icon);
                string beds = StructureTexts.Beds(structure, _texts);
                string cost = StructureTexts.Cost(structure, _texts);
                string detail = beds.Length > 0 && cost.Length > 0 ? beds + "\n" + cost : beds + cost;
                options.Add(new WheelOption(face, _texts.Get(structure.NameKey), detail,
                    _placement.IsActive && ReferenceEquals(_placement.Current, structure)));
            }
            return options;
        }

        // La famille qui a cette place, qu'elle propose quelque chose ou non ; null si aucune ne l'a.
        private StructureFamilyDefinition FamilyAt(int place)
        {
            int rank = System.Array.IndexOf(FamilyPlaces, place);
            IReadOnlyList<StructureFamilyDefinition> families = _catalog.Families;
            return rank >= 0 && rank < families.Count ? families[rank] : null;
        }

        // Vrai si la construction en main est de cette famille.
        private bool Holds(StructureFamilyDefinition family)
        {
            if (!_placement.IsActive)
                return false;
            foreach (StructureDefinition structure in family.Structures)
            {
                if (ReferenceEquals(structure, _placement.Current))
                    return true;
            }
            return false;
        }

        // La touche écrite sur cette case : son chiffre.
        private string KeyOf(int place)
        {
            return _keys.KeyOfSlot(place);
        }
    }
}
