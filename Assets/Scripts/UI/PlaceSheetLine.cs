using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>Une ligne du contenu d'un lieu, dans sa fiche : l'icône de la marchandise, son nom, combien il y en a.</summary>
    public readonly struct PlaceSheetLine
    {
        public PlaceSheetLine(VectorImage icon, string name, int count)
        {
            Icon = icon;
            Name = name;
            Count = count;
        }

        public VectorImage Icon { get; }
        public string Name { get; }
        public int Count { get; }
    }
}
