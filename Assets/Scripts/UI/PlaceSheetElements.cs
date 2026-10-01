using System;
using System.Globalization;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les éléments que la fiche d'un lieu range (GDD, section 16) : une ligne de son contenu (un creux de lin, l'icône de
    /// la marchandise, son nom, combien) et un bouton (un feutre cousu, sa phrase), avec les classes de Fusion.uss et de
    /// Hud.uss. La fiche les place et les remplit.
    /// </summary>
    internal static class PlaceSheetElements
    {
        private static readonly string[] LineClasses = { "creux", "pan-4", "fiche-ligne" };
        private const string IconClass = "fiche-icone";
        private static readonly string[] LineNameClasses = { "libelle", "fiche-ligne-nom" };
        private const string CountClass = "nombre";
        private static readonly string[] ActionClasses = { "feutre", "pan-10", "couture", "fiche-bouton" };
        private const string ActionTextClass = "action";

        /// <summary>Une ligne du contenu : l'icône de la marchandise, son nom, combien.</summary>
        public static VisualElement Line(PlaceSheetLine line)
        {
            var row = new Facet { pickingMode = PickingMode.Ignore };
            foreach (string style in LineClasses)
                row.AddToClassList(style);
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList(IconClass);
            if (line.Icon != null)
                icon.style.backgroundImage = Background.FromVectorImage(line.Icon);
            row.Add(icon);
            row.Add(Text(line.Name, LineNameClasses));
            row.Add(Text(line.Count.ToString(CultureInfo.InvariantCulture), CountClass));
            return row;
        }

        /// <summary>Un bouton, sans pastel ni phrase encore ; clicked est appelé quand on le clique.</summary>
        public static FeltButton Button(Action clicked, out Label text)
        {
            var button = new FeltButton();
            foreach (string style in ActionClasses)
                button.AddToClassList(style);
            button.Clicked += clicked;
            text = Text("", ActionTextClass);
            button.Add(text);
            return button;
        }

        private static Label Text(string text, params string[] classes)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            foreach (string style in classes)
                label.AddToClassList(style);
            return label;
        }
    }
}
