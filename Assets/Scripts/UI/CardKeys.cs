using System;
using Diciela.Controls;

namespace Diciela.UI
{
    /// <summary>
    /// Les touches 1 à 9 (décisions 184, 206 et 210) : le panneau Construire ouvert, elles prennent ses cartes ; sinon,
    /// un outil de sol en main (la Place, ou Chemins), les premières prennent ses modes, dans l'ordre de sa barre. Un seul
    /// aiguillage : une touche ne fait jamais les deux.
    /// </summary>
    public sealed class CardKeys : ICardPicker
    {
        private readonly IBuildMenu _menu;
        private readonly ICardPicker _cards;
        private readonly PlaceTool[] _tools;

        public CardKeys(IBuildMenu menu, ICardPicker cards, params PlaceTool[] tools)
        {
            if (menu == null || cards == null || tools == null || Array.IndexOf(tools, null) >= 0)
                throw new ArgumentException("Les touches des cartes ont besoin du panneau Construire et des outils de sol.");
            _menu = menu;
            _cards = cards;
            _tools = tools;
        }

        public void TakeCard(int index)
        {
            if (_menu.IsOpen)
            {
                _cards.TakeCard(index);
                return;
            }
            foreach (PlaceTool tool in _tools)
            {
                if (tool.IsActive && index >= 0 && index < tool.Entry.Modes.Count)
                    tool.Choose(tool.Entry.Modes[index]);
            }
        }
    }
}
