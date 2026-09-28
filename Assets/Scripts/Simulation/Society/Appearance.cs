using System;
using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// L'apparence d'un habitant (GDD, section 6 ; ARCHITECTURE, section 4) : son corps, sa couleur, et au plus une pièce
    /// par emplacement. Elle est entière dès sa naissance : une apparence incohérente ne se construit pas. La simulation
    /// la garde avec l'habitant sans jamais la lire ; la vue l'habille.
    /// </summary>
    public sealed class Appearance
    {
        private readonly AttachmentDefinition[] _attachments;

        public Appearance(BodyDefinition body, PaletteDefinition palette, IReadOnlyList<AttachmentDefinition> attachments)
        {
            if (ReferenceEquals(body, null))
                throw new ArgumentNullException(nameof(body));
            if (ReferenceEquals(palette, null))
                throw new ArgumentNullException(nameof(palette));
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));

            _attachments = new AttachmentDefinition[attachments.Count];
            for (int i = 0; i < attachments.Count; i++)
            {
                AttachmentDefinition attachment = attachments[i];
                if (ReferenceEquals(attachment, null) || ReferenceEquals(attachment.Slot, null))
                    throw new ArgumentException("Une pièce de l'apparence est vide, ou sans emplacement.",
                        nameof(attachments));
                for (int j = 0; j < i; j++)
                {
                    if (ReferenceEquals(_attachments[j].Slot, attachment.Slot))
                        throw new ArgumentException("Deux pièces de l'apparence sont au même emplacement (« " +
                                                    attachment.Slot.Id + " »).", nameof(attachments));
                }
                _attachments[i] = attachment;
            }
            Body = body;
            Palette = palette;
        }

        public BodyDefinition Body { get; }
        public PaletteDefinition Palette { get; }

        /// <summary>Ses pièces, une au plus par emplacement.</summary>
        public IReadOnlyList<AttachmentDefinition> Attachments => _attachments;

        /// <summary>
        /// L'apparence de départ que propose ce catalogue : son premier corps, dans sa couleur d'origine, sans pièce.
        /// </summary>
        public static Appearance StartingIn(Catalog catalog)
        {
            if (ReferenceEquals(catalog, null) || catalog.Bodies.Count == 0 || ReferenceEquals(catalog.Bodies[0], null))
                throw new ArgumentException("Le catalogue ne propose aucun corps.", nameof(catalog));

            BodyDefinition body = catalog.Bodies[0];
            return new Appearance(body, body.Palette, Array.Empty<AttachmentDefinition>());
        }
    }
}
