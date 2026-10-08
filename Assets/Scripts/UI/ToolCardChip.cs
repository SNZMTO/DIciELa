namespace Diciela.UI
{
    /// <summary>
    /// Une puce de la carte de l'outil en main (décision 221) : un nombre ou un réglage, écrit, avec son visage devant
    /// (une icône, la texture d'un sol, la photo d'une construction), ou rien.
    /// </summary>
    public readonly struct ToolCardChip
    {
        public ToolCardChip(HudFace face, string text)
        {
            Face = face;
            Text = text ?? "";
        }

        public HudFace Face { get; }

        public string Text { get; }
    }
}
