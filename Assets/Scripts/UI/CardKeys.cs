using System;
using Diciela.Controls;

namespace Diciela.UI
{
    /// <summary>
    /// Les chiffres (décisions 184 et 221) : le panneau Construire ouvert, ils prennent ses cartes ; sinon, les cases de
    /// la roue de l'outil en main (IRingKeys). Un seul aiguillage : une touche ne fait jamais les deux.
    /// </summary>
    public sealed class CardKeys : ICardPicker
    {
        private readonly IBuildMenu _menu;
        private readonly ICardPicker _cards;
        private readonly IRingKeys _ring;

        public CardKeys(IBuildMenu menu, ICardPicker cards, IRingKeys ring)
        {
            if (menu == null || cards == null || ring == null)
                throw new ArgumentException("Les chiffres ont besoin du panneau Construire et de la roue.");
            _menu = menu;
            _cards = cards;
            _ring = ring;
        }

        public void TakeCard(int index)
        {
            if (_menu.IsOpen)
                _cards.TakeCard(index);
            else
                _ring.TakeSlot(index);
        }
    }
}
