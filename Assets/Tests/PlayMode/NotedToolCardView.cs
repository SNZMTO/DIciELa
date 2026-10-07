using System.Collections.Generic;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Une vue de la carte de l'outil en main qui note ce que le présentateur lui montre.</summary>
    internal sealed class NotedToolCardView : IToolCardView
    {
        public bool IsShown { get; private set; }
        public string Icon { get; private set; } = "";
        public string Pastel { get; private set; } = "";
        public string Name { get; private set; } = "";
        public string ModeIcon { get; private set; } = "";
        public string Mode { get; private set; } = "";
        public List<ToolCardChip> Chips { get; } = new List<ToolCardChip>();
        public string Reason { get; private set; } = "";
        public bool IsRefusal { get; private set; }

        /// <summary>Les textes de ses puces, dans leur ordre.</summary>
        public string[] ChipTexts => Chips.ConvertAll(chip => chip.Text).ToArray();

        public void ShowNothing()
        {
            IsShown = false;
        }

        public void ShowTool(string icon, string pastel, string name, string modeIcon, string mode)
        {
            IsShown = true;
            Icon = icon;
            Pastel = pastel;
            Name = name;
            ModeIcon = modeIcon;
            Mode = mode;
        }

        public void ShowChips(IReadOnlyList<ToolCardChip> chips)
        {
            Chips.Clear();
            foreach (ToolCardChip chip in chips)
                Chips.Add(chip);
        }

        public void ShowReason(string text, bool refusal)
        {
            Reason = text;
            IsRefusal = refusal;
        }
    }
}
