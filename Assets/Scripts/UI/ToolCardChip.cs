using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Une puce de la carte de l'outil en main (décision 221) : un nombre ou un réglage, écrit, avec l'icône que nomme
    /// sa classe de style, ou une vignette (la texture d'un sol, la photo d'une construction), ou rien devant.
    /// </summary>
    public readonly struct ToolCardChip
    {
        public ToolCardChip(string icon, Texture2D picture, string text)
        {
            Icon = icon ?? "";
            Picture = picture;
            Text = text ?? "";
        }

        /// <summary>La classe de son icône ; vide si elle n'en a pas.</summary>
        public string Icon { get; }

        /// <summary>Sa vignette ; null si elle n'en a pas.</summary>
        public Texture2D Picture { get; }

        public string Text { get; }
    }
}
