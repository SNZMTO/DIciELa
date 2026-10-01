using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>Un bouton de la fiche d'un lieu : ce qu'il dit, déjà traduit, et le pastel de son feutre.</summary>
    public readonly struct PlaceSheetAction
    {
        public readonly string Text;
        public readonly Pastel Pastel;

        public PlaceSheetAction(string text, Pastel pastel)
        {
            Text = text;
            Pastel = pastel;
        }
    }
}
