using System;
using System.Collections.Generic;
using System.Globalization;
using Diciela.Data;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les éléments que la barre de construction range (GDD, section 16) : l'onglet d'une famille (son pastel, son icône,
    /// son nom) et la carte d'une construction (sa vignette, son nom, ce qu'elle coûte), en feutre, avec les classes de
    /// Fusion.uss et de Hud.uss. La barre les place et dit lesquels sont enfoncés.
    /// </summary>
    internal static class BuildBarElements
    {
        private const string PaleSuffix = "-pale";
        private static readonly string[] TabClasses = { "feutre", "pan-10", "couture", "onglet" };
        private static readonly string[] CardClasses = { "feutre", "pan-10", "carte" };
        private const string TabIconClass = "onglet-icone";
        private const string TabNameClass = "action";
        private const string CardPictureClass = "carte-vignette";
        private static readonly string[] CardNameClasses = { "t2", "carte-nom" };
        private static readonly string[] CostClasses = { "cout", "carte-cout" };
        private const string CostLineClass = "carte-cout-ligne";
        private const string CostIconClass = "carte-cout-icone";

        /// <summary>L'onglet d'une famille, dans son pastel ; clicked est appelé quand on le clique.</summary>
        public static FeltButton Tab(BuildBarFamily family, Action clicked)
        {
            FeltButton tab = Button(TabClasses, Pastels.ClassOf(family.Pastel), clicked);
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList(TabIconClass);
            if (family.Icon != null)
                icon.style.backgroundImage = Background.FromVectorImage(family.Icon);
            tab.Add(icon);
            tab.Add(Text(family.Name, TabNameClass));
            return tab;
        }

        /// <summary>
        /// La carte d'une construction, dans le pastel pâle de sa famille ; clicked est appelé quand on la clique.
        /// </summary>
        public static FeltButton Card(BuildBarCard card, Action clicked)
        {
            FeltButton button = Button(CardClasses, Pastels.ClassOf(card.Pastel) + PaleSuffix, clicked);
            if (card.Picture != null)
            {
                var picture = new VisualElement { pickingMode = PickingMode.Ignore };
                picture.AddToClassList(CardPictureClass);
                picture.style.backgroundImage = new StyleBackground(card.Picture);
                button.Add(picture);
            }
            button.Add(Text(card.Name, CardNameClasses));
            if (card.Cost != null && card.Cost.Count > 0)
                button.Add(Cost(card.Cost));
            return button;
        }

        /// <summary>Le coût d'une carte : chaque marchandise, son icône et combien.</summary>
        private static VisualElement Cost(IReadOnlyList<GoodAmount> cost)
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            foreach (string style in CostClasses)
                row.AddToClassList(style);
            foreach (GoodAmount goods in cost)
            {
                var line = new VisualElement { pickingMode = PickingMode.Ignore };
                line.AddToClassList(CostLineClass);
                var icon = new VisualElement { pickingMode = PickingMode.Ignore };
                icon.AddToClassList(CostIconClass);
                if (goods.Good != null && goods.Good.Icon != null)
                    icon.style.backgroundImage = Background.FromVectorImage(goods.Good.Icon);
                line.Add(icon);
                line.Add(Text(goods.Amount.ToString(CultureInfo.InvariantCulture)));
                row.Add(line);
            }
            return row;
        }

        private static FeltButton Button(string[] classes, string pastel, Action clicked)
        {
            var button = new FeltButton();
            foreach (string style in classes)
                button.AddToClassList(style);
            button.AddToClassList(pastel);
            button.Clicked += clicked;
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
