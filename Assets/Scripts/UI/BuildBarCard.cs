using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Une carte de la barre de construction, telle que la vue la montre : le nom traduit, la vignette (absente tant que
    /// la construction n'est pas photographiée), ou son icône faute de modèle (la Place), le pastel de sa famille, dont
    /// la carte prend la version pâle, ce qu'elle offre (ses lits, écrits : vide si elle n'en a pas), son coût (vide pour
    /// ce qui se dresse d'un coup ou ne coûte rien) et la touche qui la prend, écrite sur elle (vide : aucune).
    /// </summary>
    public readonly struct BuildBarCard
    {
        public readonly string Name;
        public readonly Texture2D Picture;
        public readonly VectorImage Icon;
        public readonly Pastel Pastel;
        public readonly string Beds;
        public readonly IReadOnlyList<GoodAmount> Cost;
        public readonly string Key;

        public BuildBarCard(string name, Texture2D picture, VectorImage icon, Pastel pastel, string beds,
            IReadOnlyList<GoodAmount> cost, string key)
        {
            Name = name;
            Picture = picture;
            Icon = icon;
            Pastel = pastel;
            Beds = beds;
            Cost = cost;
            Key = key;
        }
    }
}
