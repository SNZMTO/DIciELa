using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les éléments que la fiche d'un lieu range (GDD, section 16) : une ligne de son contenu (un creux de lin, l'icône de
    /// la marchandise, son nom, combien), ses postes (le nom d'un poste, puis qui le tient) et un bouton (un feutre cousu,
    /// sa phrase), avec les classes de Fusion.uss et de Hud.uss. La fiche les place et les remplit.
    /// </summary>
    internal static class LocationSheetElements
    {
        private static readonly string[] LineClasses = { "creux", "pan-4", "fiche-ligne" };
        private const string IconClass = "fiche-icone";
        private static readonly string[] LineNameClasses = { "libelle", "fiche-ligne-nom" };
        private const string CountClass = "nombre";
        private static readonly string[] ActionClasses = { "feutre", "pan-10", "couture", "fiche-bouton" };
        private const string ActionTextClass = "action";
        private const string SlotNameClass = "nom";
        private const string SlotHolderClass = "sous-titre";

        /// <summary>Une ligne du contenu : l'icône de la marchandise, son nom, combien.</summary>
        public static VisualElement Line(LocationSheetLine line)
        {
            Facet row = Styled.With(new Facet { pickingMode = PickingMode.Ignore }, LineClasses);
            row.Add(Styled.With(new VisualElement { pickingMode = PickingMode.Ignore }, IconClass));
            row.Add(Styled.Text(line.Name, LineNameClasses));
            row.Add(Styled.Text(line.Count.ToString(CultureInfo.InvariantCulture), CountClass));
            ShowIcon(row, line.Icon);
            return row;
        }

        /// <summary>Remplit une ligne déjà faite d'un autre contenu, sans la refaire : l'icône, le nom, combien.</summary>
        public static void Refill(VisualElement row, LocationSheetLine line)
        {
            if (!ReferenceEquals(row.userData, line.Icon))
                ShowIcon(row, line.Icon);
            var name = (Label)row[1];
            if (name.text != line.Name)
                name.text = line.Name;
            ((Label)row[2]).text = line.Count.ToString(CultureInfo.InvariantCulture);
        }

        // L'icône d'une ligne ; la ligne la retient, pour ne la changer que si elle change.
        private static void ShowIcon(VisualElement row, VectorImage image)
        {
            row.userData = image;
            row[0].style.backgroundImage = image != null
                ? new StyleBackground(Background.FromVectorImage(image))
                : new StyleBackground(StyleKeyword.Null);
        }

        /// <summary>
        /// Écrit les postes du lieu dans ce bloc : le nom d'un poste une fois pour des postes pareils qui se suivent, puis
        /// qui tient chacun. Le bloc ne se refait que si ses postes changent ; sinon, qui les tient s'écrit sur place.
        /// </summary>
        public static void ShowSlots(VisualElement block, IReadOnlyList<LocationSheetSlot> slots)
        {
            if (!HasTheShapeOf(block, slots))
                block.Clear();
            int child = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (i == 0 || slots[i].Name != slots[i - 1].Name)
                {
                    if (child == block.childCount)
                        block.Add(Styled.Text(slots[i].Name, SlotNameClass));
                    child++;
                }
                if (child == block.childCount)
                    block.Add(Styled.Text(slots[i].Holder, SlotHolderClass));
                else if (((Label)block[child]).text != slots[i].Holder)
                    ((Label)block[child]).text = slots[i].Holder;
                child++;
            }
        }

        // Vrai si le bloc montre déjà ces postes : les mêmes noms aux mêmes places, autant de lignes.
        private static bool HasTheShapeOf(VisualElement block, IReadOnlyList<LocationSheetSlot> slots)
        {
            int child = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (i == 0 || slots[i].Name != slots[i - 1].Name)
                {
                    if (child >= block.childCount || !block[child].ClassListContains(SlotNameClass) ||
                        ((Label)block[child]).text != slots[i].Name)
                        return false;
                    child++;
                }
                if (child >= block.childCount || !block[child].ClassListContains(SlotHolderClass))
                    return false;
                child++;
            }
            return child == block.childCount;
        }

        /// <summary>Un bouton, sans pastel ni phrase encore ; clicked est appelé quand on le clique.</summary>
        public static FeltButton Button(Action clicked, out Label text)
        {
            FeltButton button = Styled.With(new FeltButton(), ActionClasses);
            button.Clicked += clicked;
            text = Styled.Text("", ActionTextClass);
            button.Add(text);
            return button;
        }
    }
}
