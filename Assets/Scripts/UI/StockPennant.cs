using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Un fanion du stock, tel que la vue le montre : l'icône de la marchandise, combien la ville en tient, et ce que dit
    /// son infobulle. À zéro, il reste, en douce.
    /// </summary>
    public readonly struct StockPennant
    {
        public StockPennant(VectorImage icon, int count, string tooltip)
        {
            Icon = icon;
            Count = count;
            Tooltip = tooltip;
        }

        public VectorImage Icon { get; }
        public int Count { get; }
        public string Tooltip { get; }
    }
}
