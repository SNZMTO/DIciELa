using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Ce qu'une construction offre et ce qu'elle coûte, écrits (GDD, section 16) : ses lits (« 2 lits » ; rien si elle
    /// n'en a pas), et son coût, chaque marchandise avec combien (« 10 Bois, 5 Pierre » ; rien pour ce qui se dresse
    /// d'un coup). La bulle de la roue les dit, la carte en fait des puces.
    /// </summary>
    internal static class StructureTexts
    {
        /// <summary>Les lits qu'offre cette construction (« 2 lits ») ; vide si elle n'en a pas.</summary>
        public static string Beds(StructureDefinition structure, TextTable texts)
        {
            if (structure.Beds <= 0)
                return "";
            return texts.Get(TextKeys.CardBeds)
                .Replace(TextKeys.BedsParameter, structure.Beds.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Ce que coûte cette construction (« 10 Bois, 5 Pierre ») ; vide si elle ne coûte rien.</summary>
        public static string Cost(StructureDefinition structure, TextTable texts)
        {
            IReadOnlyList<GoodAmount> cost = structure.Cost;
            var text = new StringBuilder();
            for (int i = 0; i < cost.Count; i++)
            {
                if (cost[i].Good == null)
                    continue;
                if (text.Length > 0)
                    text.Append(texts.Get(TextKeys.ListSeparator));
                text.Append(texts.Get(TextKeys.CostAmount)
                    .Replace(TextKeys.CountParameter, cost[i].Amount.ToString(CultureInfo.InvariantCulture))
                    .Replace(TextKeys.GoodParameter, texts.Get(cost[i].Good.NameKey)));
            }
            return text.ToString();
        }
    }
}
