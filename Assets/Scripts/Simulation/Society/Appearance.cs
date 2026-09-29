using System;
using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// L'apparence d'un habitant (GDD, section 6 ; ARCHITECTURE, section 4) : sa tête et son corps, chacun de sa couleur,
    /// et au plus une pièce par emplacement, chacune de la sienne. Elle est entière dès sa naissance : une apparence
    /// incohérente ne se construit pas. La simulation la garde avec l'habitant sans jamais la lire ; la vue l'habille.
    /// </summary>
    public sealed class Appearance
    {
        private readonly WornAttachment[] _attachments;

        public Appearance(HeadDefinition head, PaletteDefinition headPalette, BodyDefinition body,
            PaletteDefinition bodyPalette, IReadOnlyList<WornAttachment> attachments)
        {
            if (ReferenceEquals(head, null))
                throw new ArgumentNullException(nameof(head));
            if (ReferenceEquals(headPalette, null))
                throw new ArgumentNullException(nameof(headPalette));
            if (ReferenceEquals(body, null))
                throw new ArgumentNullException(nameof(body));
            if (ReferenceEquals(bodyPalette, null))
                throw new ArgumentNullException(nameof(bodyPalette));
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));

            _attachments = new WornAttachment[attachments.Count];
            for (int i = 0; i < attachments.Count; i++)
            {
                WornAttachment worn = attachments[i];
                if (ReferenceEquals(worn.Attachment, null) || ReferenceEquals(worn.Attachment.Slot, null) ||
                    ReferenceEquals(worn.Palette, null))
                    throw new ArgumentException("Une pièce de l'apparence est vide, sans emplacement ou sans couleur.",
                        nameof(attachments));
                for (int j = 0; j < i; j++)
                {
                    if (ReferenceEquals(_attachments[j].Attachment.Slot, worn.Attachment.Slot))
                        throw new ArgumentException("Deux pièces de l'apparence sont au même emplacement (« " +
                                                    worn.Attachment.Slot.Id + " »).", nameof(attachments));
                }
                _attachments[i] = worn;
            }
            Head = head;
            HeadPalette = headPalette;
            Body = body;
            BodyPalette = bodyPalette;
        }

        public HeadDefinition Head { get; }
        public PaletteDefinition HeadPalette { get; }
        public BodyDefinition Body { get; }
        public PaletteDefinition BodyPalette { get; }

        /// <summary>Ses pièces, une au plus par emplacement, chacune de sa couleur.</summary>
        public IReadOnlyList<WornAttachment> Attachments => _attachments;

        /// <summary>
        /// L'apparence de départ que propose ce catalogue : sa première tête et son premier corps, dans leurs couleurs
        /// d'origine, avec les pièces que chacun porte d'office, de sa couleur (les oreilles du chat, sa queue).
        /// </summary>
        public static Appearance StartingIn(Catalog catalog)
        {
            if (ReferenceEquals(catalog, null) || catalog.Heads.Count == 0 || ReferenceEquals(catalog.Heads[0], null) ||
                catalog.Bodies.Count == 0 || ReferenceEquals(catalog.Bodies[0], null))
                throw new ArgumentException("Le catalogue ne propose aucune tête ou aucun corps.", nameof(catalog));

            HeadDefinition head = catalog.Heads[0];
            BodyDefinition body = catalog.Bodies[0];
            var worn = new List<WornAttachment>(head.Worn.Count + body.Worn.Count);
            foreach (AttachmentDefinition piece in head.Worn)
                worn.Add(new WornAttachment(piece, head.Palette));
            foreach (AttachmentDefinition piece in body.Worn)
                worn.Add(new WornAttachment(piece, body.Palette));
            return new Appearance(head, head.Palette, body, body.Palette, worn);
        }
    }
}
