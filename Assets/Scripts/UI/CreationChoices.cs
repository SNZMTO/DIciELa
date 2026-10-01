using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Society;

namespace Diciela.UI
{
    /// <summary>
    /// Les choix de l'écran de création (GDD, section 6), sans l'écran : une ligne pour la tête, une pour le corps, puis
    /// une par emplacement, chacune avec sa pièce et sa couleur. Les flèches font le tour des pièces d'une ligne (« aucune »
    /// en tête si l'on peut se passer de l'emplacement) et des couleurs du catalogue ; changer de pièce lui rend sa couleur
    /// d'origine. On part de l'apparence de départ du catalogue. Les pièces qu'une tête porte d'office (ses oreilles, ses
    /// touffes) la suivent : sur chaque ligne qui montrait une pièce de l'ancienne tête (ou rien, faute d'en avoir), celle
    /// de la nouvelle ; une pièce choisie par le joueur reste. Une ligne que la tête ne porte pas (les oreilles de la
    /// grenouille) reste vide avec elle. Ce que le catalogue propose se lit dans son offre (CreationOffer).
    /// </summary>
    public sealed class CreationChoices
    {
        private const int HeadRow = 0;
        private const int BodyRow = 1;
        private const int FirstSlotRow = 2;
        private const int None = -1;
        // Les identifiants des deux premières lignes ; les suivantes prennent celui de leur emplacement.
        private const string HeadRowId = "tete";
        private const string BodyRowId = "corps";

        private readonly Catalog _catalog;
        private readonly CreationOffer _offer;
        private readonly int[] _pieces;
        private readonly int[] _palettes;
        private readonly List<WornAttachment> _worn = new List<WornAttachment>();

        public CreationChoices(Catalog catalog)
        {
            Appearance starting = Appearance.StartingIn(catalog);
            _catalog = catalog;
            _offer = new CreationOffer(catalog);
            _pieces = new int[FirstSlotRow + _offer.SlotCount];
            _palettes = new int[_pieces.Length];
            for (int row = FirstSlotRow; row < _pieces.Length; row++)
                _pieces[row] = _palettes[row] = None;
            _palettes[HeadRow] = _offer.PaletteIndex(starting.HeadPalette);
            _palettes[BodyRow] = _offer.PaletteIndex(starting.BodyPalette);
            foreach (WornAttachment worn in starting.Attachments)
            {
                for (int slot = 0; slot < _offer.SlotCount; slot++)
                {
                    int piece = _offer.PiecesAt(slot).IndexOf(worn.Attachment);
                    if (piece < 0)
                        continue;
                    _pieces[FirstSlotRow + slot] = piece;
                    _palettes[FirstSlotRow + slot] = _offer.PaletteIndex(worn.Palette);
                }
            }
        }

        public int Rows => _pieces.Length;

        /// <summary>L'identifiant de cette ligne : « tete », « corps », puis celui de l'emplacement ; il choisit son icône.</summary>
        public string RowId(int row)
        {
            if (row == HeadRow)
                return HeadRowId;
            return row == BodyRow ? BodyRowId : _catalog.AttachmentSlots[row - FirstSlotRow].Id;
        }

        /// <summary>La clé du nom de cette ligne.</summary>
        public string LabelKey(int row)
        {
            if (row == HeadRow)
                return TextKeys.CreationHead;
            return row == BodyRow ? TextKeys.CreationBody : _catalog.AttachmentSlots[row - FirstSlotRow].NameKey;
        }

        /// <summary>La clé du nom de la pièce choisie sur cette ligne ; vide pour « aucune », ou que la tête ne porte pas.</summary>
        public string PieceKey(int row)
        {
            if (Refused(row))
                return "";
            if (row == HeadRow)
                return _catalog.Heads[_pieces[row]].NameKey;
            if (row == BodyRow)
                return _catalog.Bodies[_pieces[row]].NameKey;
            return _pieces[row] == None ? "" : _offer.PiecesAt(row - FirstSlotRow)[_pieces[row]].NameKey;
        }

        /// <summary>La clé du nom de la couleur de cette ligne ; vide sans pièce.</summary>
        public string PaletteKey(int row)
        {
            if (Refused(row))
                return "";
            return _palettes[row] == None ? "" : _catalog.Palettes[_palettes[row]].NameKey;
        }

        /// <summary>Passe à la pièce d'à côté sur cette ligne (-1 à gauche, +1 à droite), dans sa couleur d'origine.</summary>
        public void Step(int row, int direction)
        {
            if (row == HeadRow)
            {
                HeadDefinition before = _catalog.Heads[_pieces[row]];
                _pieces[row] = Wrap(_pieces[row] + direction, _catalog.Heads.Count);
                _palettes[row] = _offer.PaletteIndex(_catalog.Heads[_pieces[row]].Palette);
                Follow(before, _catalog.Heads[_pieces[row]]);
            }
            else if (row == BodyRow)
            {
                _pieces[row] = Wrap(_pieces[row] + direction, _catalog.Bodies.Count);
                _palettes[row] = _offer.PaletteIndex(_catalog.Bodies[_pieces[row]].Palette);
            }
            else if (row > BodyRow && row < _pieces.Length && !Refused(row))
            {
                List<AttachmentDefinition> offered = _offer.PiecesAt(row - FirstSlotRow);
                // « Aucune » est un choix de la ligne, avant ses pièces, si l'on peut se passer de son emplacement.
                if (_catalog.AttachmentSlots[row - FirstSlotRow].Required && offered.Count > 0)
                    _pieces[row] = Wrap(_pieces[row] + direction, offered.Count);
                else
                    _pieces[row] = Wrap(_pieces[row] + 1 + direction, offered.Count + 1) - 1;
                _palettes[row] = _pieces[row] == None ? None : _offer.PaletteIndex(offered[_pieces[row]].Palette);
            }
        }

        /// <summary>Passe à la couleur d'à côté sur cette ligne, si elle porte une pièce.</summary>
        public void Recolor(int row, int direction)
        {
            if (row >= 0 && row < _pieces.Length && _palettes[row] != None && !Refused(row))
                _palettes[row] = Wrap(_palettes[row] + direction, _catalog.Palettes.Count);
        }

        /// <summary>L'apparence choisie en ce moment.</summary>
        public Appearance Current()
        {
            _worn.Clear();
            for (int row = FirstSlotRow; row < _pieces.Length; row++)
            {
                if (_pieces[row] != None && !Refused(row))
                    _worn.Add(new WornAttachment(_offer.PiecesAt(row - FirstSlotRow)[_pieces[row]], Palette(row)));
            }
            return new Appearance(_catalog.Heads[_pieces[HeadRow]], Palette(HeadRow), _catalog.Bodies[_pieces[BodyRow]],
                Palette(BodyRow), _worn);
        }

        /// <summary>
        /// Sur chaque ligne d'emplacement qui montrait la pièce d'office de l'ancienne tête (ou rien, faute d'en avoir),
        /// met celle de la nouvelle (ou rien), dans sa couleur d'origine.
        /// </summary>
        private void Follow(HeadDefinition before, HeadDefinition after)
        {
            for (int row = FirstSlotRow; row < _pieces.Length; row++)
            {
                List<AttachmentDefinition> offered = _offer.PiecesAt(row - FirstSlotRow);
                AttachmentSlotDefinition slot = _catalog.AttachmentSlots[row - FirstSlotRow];
                AttachmentDefinition shown = _pieces[row] == None ? null : offered[_pieces[row]];
                if (!ReferenceEquals(shown, CreationOffer.WornIn(before, slot)))
                    continue;
                AttachmentDefinition own = CreationOffer.WornIn(after, slot);
                // Une ligne dont on ne peut pas se passer garde sa pièce si la nouvelle tête n'en porte pas, ou prend la
                // première qu'elle propose si elle n'en avait pas (en revenant d'une tête qui ne la porte pas).
                if (own == null && slot.Required)
                {
                    if (shown == null && offered.Count > 0)
                        (_pieces[row], _palettes[row]) = (0, _offer.PaletteIndex(offered[0].Palette));
                    continue;
                }
                _pieces[row] = own == null ? None : offered.IndexOf(own);
                _palettes[row] = _pieces[row] == None ? None : _offer.PaletteIndex(own.Palette);
            }
        }

        // Vrai si la tête choisie ne porte pas ce qui va à l'emplacement de cette ligne : sa pièce ne se montre ni ne change.
        private bool Refused(int row)
        {
            return row >= FirstSlotRow && row < _pieces.Length &&
                   _catalog.Heads[_pieces[HeadRow]].Refuses(_catalog.AttachmentSlots[row - FirstSlotRow]);
        }

        private PaletteDefinition Palette(int row) => _catalog.Palettes[_palettes[row]];

        private static int Wrap(int index, int count) => (index % count + count) % count;
    }
}
