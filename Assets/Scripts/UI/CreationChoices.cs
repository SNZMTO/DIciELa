using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Society;

namespace Diciela.UI
{
    /// <summary>
    /// Les choix de l'écran de création (GDD, section 6), sans l'écran : une ligne pour la tête, une pour le corps, puis
    /// une par emplacement, chacune avec sa pièce et sa couleur. Les flèches font le tour des pièces d'une ligne (« aucune »
    /// en tête pour un emplacement ; la tête et le corps en ont toujours une) et des couleurs du catalogue ; changer de
    /// pièce lui rend sa couleur d'origine. On part de l'apparence de départ du catalogue.
    /// </summary>
    public sealed class CreationChoices
    {
        private const int HeadRow = 0;
        private const int BodyRow = 1;
        private const int FirstSlotRow = 2;
        private const int None = -1;

        private readonly Catalog _catalog;
        private readonly List<List<AttachmentDefinition>> _offered = new List<List<AttachmentDefinition>>();
        private readonly int[] _pieces;
        private readonly int[] _palettes;
        private readonly List<WornAttachment> _worn = new List<WornAttachment>();

        public CreationChoices(Catalog catalog)
        {
            Appearance starting = Appearance.StartingIn(catalog);
            _catalog = catalog;
            foreach (AttachmentSlotDefinition slot in catalog.AttachmentSlots)
            {
                var pieces = new List<AttachmentDefinition>();
                foreach (AttachmentDefinition piece in catalog.Attachments)
                {
                    if (piece != null && ReferenceEquals(piece.Slot, slot))
                        pieces.Add(piece);
                }
                _offered.Add(pieces);
            }

            _pieces = new int[FirstSlotRow + _offered.Count];
            _palettes = new int[_pieces.Length];
            for (int row = FirstSlotRow; row < _pieces.Length; row++)
                _pieces[row] = _palettes[row] = None;
            _palettes[HeadRow] = PaletteIndex(starting.HeadPalette);
            _palettes[BodyRow] = PaletteIndex(starting.BodyPalette);
            foreach (WornAttachment worn in starting.Attachments)
            {
                for (int slot = 0; slot < _offered.Count; slot++)
                {
                    int piece = _offered[slot].IndexOf(worn.Attachment);
                    if (piece < 0)
                        continue;
                    _pieces[FirstSlotRow + slot] = piece;
                    _palettes[FirstSlotRow + slot] = PaletteIndex(worn.Palette);
                }
            }
        }

        public int Rows => _pieces.Length;

        /// <summary>La clé du nom de cette ligne.</summary>
        public string LabelKey(int row)
        {
            if (row == HeadRow)
                return TextKeys.CreationHead;
            return row == BodyRow ? TextKeys.CreationBody : _catalog.AttachmentSlots[row - FirstSlotRow].NameKey;
        }

        /// <summary>La clé du nom de la pièce choisie sur cette ligne ; vide pour « aucune ».</summary>
        public string PieceKey(int row)
        {
            if (row == HeadRow)
                return _catalog.Heads[_pieces[row]].NameKey;
            if (row == BodyRow)
                return _catalog.Bodies[_pieces[row]].NameKey;
            return _pieces[row] == None ? "" : _offered[row - FirstSlotRow][_pieces[row]].NameKey;
        }

        /// <summary>La clé du nom de la couleur de cette ligne ; vide sans pièce.</summary>
        public string PaletteKey(int row)
        {
            return _palettes[row] == None ? "" : _catalog.Palettes[_palettes[row]].NameKey;
        }

        /// <summary>Passe à la pièce d'à côté sur cette ligne (-1 à gauche, +1 à droite), dans sa couleur d'origine.</summary>
        public void Step(int row, int direction)
        {
            if (row == HeadRow)
            {
                _pieces[row] = Wrap(_pieces[row] + direction, _catalog.Heads.Count);
                _palettes[row] = PaletteIndex(_catalog.Heads[_pieces[row]].Palette);
            }
            else if (row == BodyRow)
            {
                _pieces[row] = Wrap(_pieces[row] + direction, _catalog.Bodies.Count);
                _palettes[row] = PaletteIndex(_catalog.Bodies[_pieces[row]].Palette);
            }
            else if (row > BodyRow && row < _pieces.Length)
            {
                List<AttachmentDefinition> offered = _offered[row - FirstSlotRow];
                // « Aucune » est un choix de la ligne, avant ses pièces.
                _pieces[row] = Wrap(_pieces[row] + 1 + direction, offered.Count + 1) - 1;
                _palettes[row] = _pieces[row] == None ? None : PaletteIndex(offered[_pieces[row]].Palette);
            }
        }

        /// <summary>Passe à la couleur d'à côté sur cette ligne, si elle porte une pièce.</summary>
        public void Recolor(int row, int direction)
        {
            if (row >= 0 && row < _pieces.Length && _palettes[row] != None)
                _palettes[row] = Wrap(_palettes[row] + direction, _catalog.Palettes.Count);
        }

        /// <summary>L'apparence choisie en ce moment.</summary>
        public Appearance Current()
        {
            _worn.Clear();
            for (int row = FirstSlotRow; row < _pieces.Length; row++)
            {
                if (_pieces[row] != None)
                    _worn.Add(new WornAttachment(_offered[row - FirstSlotRow][_pieces[row]], Palette(row)));
            }
            return new Appearance(_catalog.Heads[_pieces[HeadRow]], Palette(HeadRow), _catalog.Bodies[_pieces[BodyRow]],
                Palette(BodyRow), _worn);
        }

        private PaletteDefinition Palette(int row)
        {
            return _catalog.Palettes[_palettes[row]];
        }

        /// <summary>Le rang d'une couleur dans le catalogue ; la première, si le catalogue ne l'a pas.</summary>
        private int PaletteIndex(PaletteDefinition palette)
        {
            for (int i = 0; i < _catalog.Palettes.Count; i++)
            {
                if (ReferenceEquals(_catalog.Palettes[i], palette))
                    return i;
            }
            return 0;
        }

        private static int Wrap(int index, int count)
        {
            return (index % count + count) % count;
        }
    }
}
