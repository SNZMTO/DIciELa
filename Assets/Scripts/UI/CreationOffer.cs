using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le catalogue propose à l'écran de création (GDD, section 6), pour ses choix (CreationChoices) : les pièces de
    /// chaque emplacement, dans l'ordre du catalogue ; le rang d'une couleur ; la pièce qu'une tête porte d'office à un
    /// emplacement.
    /// </summary>
    internal sealed class CreationOffer
    {
        private readonly Catalog _catalog;
        private readonly List<List<AttachmentDefinition>> _pieces = new List<List<AttachmentDefinition>>();

        public CreationOffer(Catalog catalog)
        {
            _catalog = catalog;
            foreach (AttachmentSlotDefinition slot in catalog.AttachmentSlots)
            {
                var pieces = new List<AttachmentDefinition>();
                foreach (AttachmentDefinition piece in catalog.Attachments)
                {
                    if (piece != null && ReferenceEquals(piece.Slot, slot))
                        pieces.Add(piece);
                }
                _pieces.Add(pieces);
            }
        }

        /// <summary>Le nombre d'emplacements.</summary>
        public int SlotCount => _pieces.Count;

        /// <summary>Les pièces de l'emplacement de ce rang, dans l'ordre du catalogue.</summary>
        public List<AttachmentDefinition> PiecesAt(int slot) => _pieces[slot];

        /// <summary>Le rang d'une couleur dans le catalogue ; la première, si le catalogue ne l'a pas.</summary>
        public int PaletteIndex(PaletteDefinition palette)
        {
            for (int i = 0; i < _catalog.Palettes.Count; i++)
            {
                if (ReferenceEquals(_catalog.Palettes[i], palette))
                    return i;
            }
            return 0;
        }

        /// <summary>La pièce que cette tête porte d'office à cet emplacement ; null si aucune.</summary>
        public static AttachmentDefinition WornIn(HeadDefinition head, AttachmentSlotDefinition slot)
        {
            foreach (AttachmentDefinition piece in head.Worn)
            {
                if (piece != null && ReferenceEquals(piece.Slot, slot))
                    return piece;
            }
            return null;
        }
    }
}
