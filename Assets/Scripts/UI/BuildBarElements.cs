using System;
using System.Collections.Generic;
using System.Globalization;
using Diciela.Data;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les éléments que la barre de construction range (GDD, section 16) : l'onglet d'une famille (son pastel, son icône,
    /// son nom) et la carte d'une construction (sa vignette, son nom et ce qu'elle offre, ce qu'elle coûte, sa touche sur
    /// une étiquette de lin, comme les outils de la roue), en feutre, avec les classes de Fusion.uss et de Hud.uss. La
    /// barre les place et dit lesquels sont enfoncés.
    /// </summary>
    internal static class BuildBarElements
    {
        private const string PaleSuffix = "-pale";
        private static readonly string[] TabClasses = { "feutre", "pan-10", "couture", "onglet" };
        private static readonly string[] CardClasses = { "feutre", "pan-10", "carte" };
        private const string TabIconClass = "onglet-icone";
        private const string TabNameClass = "action";
        private const string CardPictureClass = "carte-vignette";
        private const string CardTitleClass = "carte-titre";
        private static readonly string[] CardNameClasses = { "t2", "carte-nom" };
        private const string CardBedsClass = "carte-lits";
        private const string CardBedsIconClass = "carte-lits-icone";
        private const string CardBedsTextClass = "libelle";
        private static readonly string[] CostClasses = { "cout", "carte-cout" };
        private const string CostLineClass = "carte-cout-ligne";
        private const string CostIconClass = "carte-cout-icone";
        private static readonly string[] KeyClasses = { "creux", "pan-4", "touche" };
        private const string KeyTextClass = "touche-texte";

        /// <summary>L'onglet d'une famille, dans son pastel ; clicked est appelé quand on le clique.</summary>
        public static FeltButton Tab(BuildBarFamily family, Action clicked)
        {
            FeltButton tab = Button(TabClasses, Pastels.ClassOf(family.Pastel), clicked);
            var icon = Styled.With(new VisualElement { pickingMode = PickingMode.Ignore }, TabIconClass);
            if (family.Icon != null)
                icon.style.backgroundImage = Background.FromVectorImage(family.Icon);
            tab.Add(icon);
            tab.Add(Styled.Text(family.Name, TabNameClass));
            return tab;
        }

        /// <summary>
        /// La carte d'une construction, dans le pastel pâle de sa famille ; clicked est appelé quand on la clique.
        /// </summary>
        public static FeltButton Card(BuildBarCard card, Action clicked)
        {
            FeltButton button = Button(CardClasses, Pastels.ClassOf(card.Pastel) + PaleSuffix, clicked);
            // Sa vignette, la photo de son modèle.
            if (card.Picture != null)
            {
                var picture = Styled.With(new VisualElement { pickingMode = PickingMode.Ignore }, CardPictureClass);
                picture.style.backgroundImage = new StyleBackground(card.Picture);
                button.Add(picture);
            }
            button.Add(Title(card));
            if (card.Cost != null && card.Cost.Count > 0)
                button.Add(Cost(card.Cost));
            if (!string.IsNullOrEmpty(card.Key))
            {
                Facet key = Styled.With(new Facet { pickingMode = PickingMode.Ignore }, KeyClasses);
                key.Add(Styled.Text(card.Key, KeyTextClass));
                button.Add(key);
            }
            return button;
        }

        /// <summary>
        /// Le titre d'une carte : son nom, et à côté ce qu'elle offre (ses lits, l'icône du lit devant), qui passe dessous
        /// quand la place manque, comme dans la maquette.
        /// </summary>
        private static VisualElement Title(BuildBarCard card)
        {
            var title = Styled.With(new VisualElement { pickingMode = PickingMode.Ignore }, CardTitleClass);
            title.Add(Styled.Text(card.Name, CardNameClasses));
            if (!string.IsNullOrEmpty(card.Beds))
            {
                var beds = Styled.With(new VisualElement { pickingMode = PickingMode.Ignore }, CardBedsClass);
                beds.Add(Styled.With(new VisualElement { pickingMode = PickingMode.Ignore }, CardBedsIconClass));
                beds.Add(Styled.Text(card.Beds, CardBedsTextClass));
                title.Add(beds);
            }
            return title;
        }

        /// <summary>Le coût d'une carte : chaque marchandise, son icône et combien.</summary>
        private static VisualElement Cost(IReadOnlyList<GoodAmount> cost)
        {
            var row = Styled.With(new VisualElement { pickingMode = PickingMode.Ignore }, CostClasses);
            foreach (GoodAmount goods in cost)
            {
                var line = Styled.With(new VisualElement { pickingMode = PickingMode.Ignore }, CostLineClass);
                var icon = Styled.With(new VisualElement { pickingMode = PickingMode.Ignore }, CostIconClass);
                if (goods.Good != null && goods.Good.Icon != null)
                    icon.style.backgroundImage = Background.FromVectorImage(goods.Good.Icon);
                line.Add(icon);
                line.Add(Styled.Text(goods.Amount.ToString(CultureInfo.InvariantCulture)));
                row.Add(line);
            }
            return row;
        }

        private static FeltButton Button(string[] classes, string pastel, Action clicked)
        {
            FeltButton button = Styled.With(new FeltButton(), classes);
            button.AddToClassList(pastel);
            button.Clicked += clicked;
            return button;
        }
    }
}
