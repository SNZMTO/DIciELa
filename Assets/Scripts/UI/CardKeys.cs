using System;
using Diciela.Controls;

namespace Diciela.UI
{
    /// <summary>
    /// Les touches 1 à 9 (décisions 184 et 206) : le panneau Construire ouvert, elles prennent ses cartes ; sinon,
    /// l'outil de la Place en main, les cinq premières prennent ses modes, dans l'ordre de sa barre. Un seul aiguillage :
    /// une touche ne fait jamais les deux.
    /// </summary>
    public sealed class CardKeys : ICardPicker
    {
        private readonly IBuildMenu _menu;
        private readonly ICardPicker _cards;
        private readonly PlaceTool _place;

        public CardKeys(IBuildMenu menu, ICardPicker cards, PlaceTool place)
        {
            if (menu == null || cards == null || ReferenceEquals(place, null))
                throw new ArgumentException("Les touches des cartes ont besoin du panneau Construire et de l'outil de la Place.");
            _menu = menu;
            _cards = cards;
            _place = place;
        }

        public void TakeCard(int index)
        {
            if (_menu.IsOpen)
                _cards.TakeCard(index);
            else if (_place.IsActive && Enum.IsDefined(typeof(PlaceToolMode), index))
                _place.Choose((PlaceToolMode)index);
        }
    }
}
