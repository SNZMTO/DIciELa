using Diciela.Data;

namespace Diciela.Simulation.Society
{
    /// <summary>Une pièce portée (GDD, section 6) : la pièce, et la couleur qu'on lui a choisie.</summary>
    public readonly struct WornAttachment
    {
        public WornAttachment(AttachmentDefinition attachment, PaletteDefinition palette)
        {
            Attachment = attachment;
            Palette = palette;
        }

        public AttachmentDefinition Attachment { get; }
        public PaletteDefinition Palette { get; }
    }
}
