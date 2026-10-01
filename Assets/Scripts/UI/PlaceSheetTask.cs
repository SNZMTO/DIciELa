using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>Une tâche du poste qu'affiche la fiche d'un lieu : son icône, et son nom dans l'infobulle.</summary>
    public readonly struct PlaceSheetTask
    {
        public PlaceSheetTask(VectorImage icon, string name)
        {
            Icon = icon;
            Name = name;
        }

        public VectorImage Icon { get; }
        public string Name { get; }
    }
}
